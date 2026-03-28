using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PHIMAS_PREDICTIVE_ANALYTICS.Data;
using PHIMAS_PREDICTIVE_ANALYTICS.Helpers;
using PHIMAS_PREDICTIVE_ANALYTICS.Models;
using PHIMAS_PREDICTIVE_ANALYTICS.Models.ViewModels;
using PHIMAS_PREDICTIVE_ANALYTICS.Services;

namespace PHIMAS_PREDICTIVE_ANALYTICS.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BHWApiController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly PredictiveAnalyticsService _predictiveAnalyticsService;
    private readonly AIAssistantService _aiAssistantService;
    private readonly FieldSubmissionService _fieldSubmissionService;

    public BHWApiController(
        AppDbContext context,
        PredictiveAnalyticsService predictiveAnalyticsService,
        AIAssistantService aiAssistantService,
        FieldSubmissionService fieldSubmissionService)
    {
        _context = context;
        _predictiveAnalyticsService = predictiveAnalyticsService;
        _aiAssistantService = aiAssistantService;
        _fieldSubmissionService = fieldSubmissionService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { success = false, message = "Username and password are required." });
        }

        var normalizedUsername = request.Username.Trim().ToLowerInvariant();
        var user = await _context.Users.FirstOrDefaultAsync(existingUser =>
            existingUser.Role == "BHW" &&
            (existingUser.Email.ToLower() == normalizedUsername ||
             existingUser.Email.ToLower().StartsWith(normalizedUsername + "@")));

        if (user == null || !PasswordHelper.VerifyPassword(request.Password, user.Password))
        {
            return Unauthorized(new { success = false, message = "Invalid credentials." });
        }

        return Ok(MapUserProfile(user, includeSuccess: true));
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile([FromQuery] int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.Role != "BHW")
        {
            return NotFound(new { success = false, message = "BHW not found." });
        }

        return Ok(MapUserProfile(user));
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard([FromQuery] int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.Role != "BHW")
        {
            return NotFound(new { success = false, message = "BHW not found." });
        }

        var tasks = await _context.TaskAssignments
            .Where(task => task.BHWID == userId)
            .ToListAsync();

        return Ok(new
        {
            assignedTasks = tasks.Count,
            priorityTasks = tasks.Count(task => string.Equals(task.Priority, "High", StringComparison.OrdinalIgnoreCase)),
            dueTasks = tasks.Count(task => task.TaskDate.Date == DateTime.Today),
            completedTasks = tasks.Count(task => string.Equals(task.Status, "Done", StringComparison.OrdinalIgnoreCase)),
            progress = tasks.Count > 0
                ? (tasks.Count(task => string.Equals(task.Status, "Done", StringComparison.OrdinalIgnoreCase)) * 100) / tasks.Count
                : 0,
            bhwName = user.FullName,
            isAvailable = user.IsAvailable
        });
    }

    [HttpGet("tasks")]
    public async Task<IActionResult> GetTasks([FromQuery] int userId)
    {
        var tasks = await _context.TaskAssignments
            .Include(task => task.Household)
            .Where(task => task.BHWID == userId)
            .OrderBy(task => task.TaskDate)
            .ToListAsync();

        return Ok(tasks.Select(task => new
        {
            id = task.TaskID,
            title = task.Title ?? $"Visit {task.Household?.HouseholdMember ?? $"Household #{task.HouseholdID}"}",
            description = string.IsNullOrWhiteSpace(task.Description)
                ? task.Household?.Address ?? "No description available."
                : task.Description,
            priority = task.Priority,
            status = task.Status,
            taskDate = task.TaskDate,
            householdId = task.HouseholdID,
            householdName = task.Household?.HouseholdMember,
            householdAddress = task.Household?.Address
        }));
    }

    [HttpPost("updateTaskStatus")]
    public async Task<IActionResult> UpdateTaskStatus([FromBody] UpdateTaskStatusRequest request)
    {
        if (request == null || (request.TaskID <= 0 && request.Id <= 0) || string.IsNullOrWhiteSpace(request.Status))
        {
            return BadRequest(new { success = false, message = "Invalid request." });
        }

        var taskId = request.TaskID > 0 ? request.TaskID : request.Id;
        var task = await _context.TaskAssignments.FindAsync(taskId);
        if (task == null)
        {
            return NotFound(new { success = false, message = "Task not found." });
        }

        task.Status = request.Status.Trim();
        await _context.SaveChangesAsync();
        return Ok(new { success = true, message = "Task status updated successfully." });
    }

    [HttpGet("patients")]
    public async Task<IActionResult> GetPatients([FromQuery] int userId)
    {
        var households = await GetAccessibleHouseholdsAsync(userId);
        return Ok(BuildPatientDirectory(households));
    }

    [HttpGet("households")]
    public async Task<IActionResult> GetHouseholds([FromQuery] int userId)
    {
        var households = await GetAccessibleHouseholdsAsync(userId);
        return Ok(households.Select(MapHousehold));
    }

    [HttpGet("healthrecords")]
    public async Task<IActionResult> GetHealthRecords([FromQuery] int userId)
    {
        var records = await _context.HealthRecords
            .Where(record => record.BHWID == userId)
            .Include(record => record.Patient)
            .ThenInclude(patient => patient!.Household)
            .ThenInclude(household => household!.Members)
            .OrderByDescending(record => record.DateRecorded)
            .ToListAsync();

        return Ok(records.Select(MapHealthRecord));
    }

    [HttpPost("healthrecords")]
    public async Task<IActionResult> SubmitHealthRecord([FromBody] SubmitHealthRecordRequest request)
    {
        if (request == null || request.UserID <= 0)
        {
            return BadRequest(new { success = false, message = "User is required." });
        }

        var user = await _context.Users.FindAsync(request.UserID);
        if (user == null || user.Role != "BHW")
        {
            return NotFound(new { success = false, message = "BHW not found." });
        }

        var form = new CreateHealthRecordViewModel
        {
            PatientName = request.PatientName ?? string.Empty,
            ContactNumber = request.ContactNumber ?? string.Empty,
            Address = ResolveAddress(request.Address, request.HouseholdAddress, request.VisitAddress, user.AssignedArea),
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactNumber = request.EmergencyContactNumber,
            DateRecorded = request.DateRecorded ?? DateTime.UtcNow,
            Disease = string.IsNullOrWhiteSpace(request.Disease) ? "Undisclosed Condition" : request.Disease.Trim(),
            Symptoms = request.Symptoms ?? string.Empty,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Submitted" : request.Status.Trim()
        };

        var validationError = GetValidationError(form);
        if (validationError != null)
        {
            return BadRequest(new { success = false, message = validationError });
        }

        var record = await _fieldSubmissionService.CreateHealthRecordAsync(user.UserID, form);
        return Ok(new
        {
            success = true,
            message = "Health record submitted successfully.",
            recordId = record.RecordID,
            healthRecord = MapHealthRecord(record)
        });
    }

    [HttpGet("reports/recent")]
    public async Task<IActionResult> GetRecentReports([FromQuery] int userId)
    {
        var reports = await _context.Reports
            .Include(report => report.Patient)
            .ThenInclude(patient => patient!.Household)
            .ThenInclude(household => household!.Members)
            .Where(report => report.GeneratedBy == userId)
            .OrderByDescending(report => report.DateGenerated)
            .Take(10)
            .ToListAsync();

        return Ok(reports.Select(MapReport));
    }

    [HttpGet("consultationlogs")]
    public async Task<IActionResult> GetConsultationLogs([FromQuery] int userId)
    {
        var reports = await _context.Reports
            .Include(report => report.Patient)
            .ThenInclude(patient => patient!.Household)
            .ThenInclude(household => household!.Members)
            .Where(report => report.GeneratedBy == userId)
            .OrderByDescending(report => report.DateGenerated)
            .ToListAsync();

        return Ok(new { data = reports.Select(MapConsultationLog).ToList() });
    }

    [HttpPost("submitConsultation")]
    public async Task<IActionResult> SubmitConsultation([FromBody] SubmitConsultationRequest request)
    {
        if (request == null || request.UserID <= 0)
        {
            return BadRequest(new { success = false, message = "User is required." });
        }

        var user = await _context.Users.FindAsync(request.UserID);
        if (user == null || user.Role != "BHW")
        {
            return NotFound(new { success = false, message = "BHW not found." });
        }

        var form = new CreateReportViewModel
        {
            PatientName = request.PatientName ?? string.Empty,
            ContactNumber = request.ContactNumber ?? string.Empty,
            Address = ResolveAddress(request.Address, request.HouseholdAddress, request.VisitAddress, user.AssignedArea),
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactNumber = request.EmergencyContactNumber,
            DateGenerated = request.DateGenerated ?? DateTime.UtcNow,
            ReportType = string.IsNullOrWhiteSpace(request.ReportType) ? "Consultation Log" : request.ReportType.Trim(),
            Content = request.Content ?? string.Empty
        };

        var validationError = GetValidationError(form);
        if (validationError != null)
        {
            return BadRequest(new { success = false, message = validationError });
        }

        var report = await _fieldSubmissionService.CreateReportAsync(user.UserID, form);
        return Ok(new
        {
            success = true,
            message = "Consultation log submitted successfully.",
            reportId = report.ReportID,
            report = MapReport(report)
        });
    }

    [HttpPost("report")]
    public async Task<IActionResult> SubmitLegacyReport([FromBody] LegacyMobileReportRequest request)
    {
        if (request == null || request.UserID <= 0)
        {
            return BadRequest(new { success = false, message = "User is required." });
        }

        var user = await _context.Users.FindAsync(request.UserID);
        if (user == null || user.Role != "BHW")
        {
            return NotFound(new { success = false, message = "BHW not found." });
        }

        if (request.PatientID.HasValue)
        {
            var patient = await ResolvePatientAsync(request.PatientID.Value, request.UserID);
            if (patient == null)
            {
                return BadRequest(new { success = false, message = "Selected patient was not found." });
            }

            var content = string.IsNullOrWhiteSpace(request.Content) ? request.Symptoms : request.Content;
            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest(new { success = false, message = "Report content is required." });
            }

            var report = new Report
            {
                GeneratedBy = user.UserID,
                PatientID = patient.PatientID,
                DateGenerated = request.DateGenerated ?? DateTime.UtcNow,
                ReportType = string.IsNullOrWhiteSpace(request.ReportType)
                    ? (string.IsNullOrWhiteSpace(request.Disease) ? "Consultation Log" : $"{request.Disease.Trim()} Report")
                    : request.ReportType.Trim(),
                Content = content.Trim()
            };

            _context.Reports.Add(report);

            if (!string.IsNullOrWhiteSpace(request.Symptoms))
            {
                _context.HealthRecords.Add(new HealthRecord
                {
                    BHWID = user.UserID,
                    PatientID = patient.PatientID,
                    DateRecorded = request.DateRecorded ?? DateTime.UtcNow,
                    Disease = string.IsNullOrWhiteSpace(request.Disease) ? "Undisclosed Condition" : request.Disease.Trim(),
                    Symptoms = request.Symptoms.Trim(),
                    Status = string.IsNullOrWhiteSpace(request.Status) ? "Submitted" : request.Status.Trim()
                });
            }

            await _context.SaveChangesAsync();
            await _predictiveAnalyticsService.RecalculateHouseholdRisksAsync();

            report.Patient = patient;
            return Ok(new
            {
                success = true,
                message = "Report submitted successfully.",
                reportId = report.ReportID,
                report = MapReport(report)
            });
        }

        if (string.IsNullOrWhiteSpace(request.PatientName) ||
            string.IsNullOrWhiteSpace(request.ContactNumber) ||
            string.IsNullOrWhiteSpace(request.Address))
        {
            return BadRequest(new { success = false, message = "Patient ID or complete inline patient details are required." });
        }

        var reportForm = new CreateReportViewModel
        {
            PatientName = request.PatientName,
            ContactNumber = request.ContactNumber,
            Address = ResolveAddress(request.Address, request.HouseholdAddress, request.VisitAddress, user.AssignedArea),
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactNumber = request.EmergencyContactNumber,
            DateGenerated = request.DateGenerated ?? DateTime.UtcNow,
            ReportType = string.IsNullOrWhiteSpace(request.ReportType) ? "Consultation Log" : request.ReportType.Trim(),
            Content = string.IsNullOrWhiteSpace(request.Content) ? request.Symptoms ?? string.Empty : request.Content
        };

        var reportValidationError = GetValidationError(reportForm);
        if (reportValidationError != null)
        {
            return BadRequest(new { success = false, message = reportValidationError });
        }

        var createdReport = await _fieldSubmissionService.CreateReportAsync(user.UserID, reportForm);

        if (!string.IsNullOrWhiteSpace(request.Symptoms))
        {
            _context.HealthRecords.Add(new HealthRecord
            {
                BHWID = user.UserID,
                PatientID = createdReport.PatientID,
                DateRecorded = request.DateRecorded ?? DateTime.UtcNow,
                Disease = string.IsNullOrWhiteSpace(request.Disease) ? "Undisclosed Condition" : request.Disease.Trim(),
                Symptoms = request.Symptoms.Trim(),
                Status = string.IsNullOrWhiteSpace(request.Status) ? "Submitted" : request.Status.Trim()
            });

            await _context.SaveChangesAsync();
            await _predictiveAnalyticsService.RecalculateHouseholdRisksAsync();
        }

        return Ok(new
        {
            success = true,
            message = "Report submitted successfully.",
            reportId = createdReport.ReportID,
            report = MapReport(createdReport)
        });
    }

    [HttpPost("updateProfile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        if (request == null)
        {
            return BadRequest(new { success = false, message = "Invalid request." });
        }

        var user = await ResolveUserAsync(request.UserID, request.Username);
        if (user == null)
        {
            return NotFound(new { success = false, message = "User not found." });
        }

        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            user.FullName = request.FullName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            user.Email = request.Email.Trim().ToLowerInvariant();
        }

        if (request.ContactNumber != null)
        {
            user.ContactNumber = request.ContactNumber.Trim();
        }

        if (request.AssignedArea != null)
        {
            user.AssignedArea = request.AssignedArea.Trim();
        }

        await _context.SaveChangesAsync();
        return Ok(new
        {
            success = true,
            message = "Profile updated successfully.",
            profile = MapUserProfile(user)
        });
    }

    [HttpPost("changePassword")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return BadRequest(new { success = false, message = "Invalid request." });
        }

        var user = await ResolveUserAsync(request.UserID, request.Username);
        if (user == null)
        {
            return NotFound(new { success = false, message = "User not found." });
        }

        var currentPassword = string.IsNullOrWhiteSpace(request.OldPassword) ? request.CurrentPassword : request.OldPassword;
        if (!string.IsNullOrWhiteSpace(currentPassword) && !PasswordHelper.VerifyPassword(currentPassword, user.Password))
        {
            return BadRequest(new { success = false, message = "Current password is incorrect." });
        }

        user.Password = PasswordHelper.HashPassword(request.NewPassword);
        await _context.SaveChangesAsync();
        return Ok(new { success = true, message = "Password changed successfully." });
    }

    [HttpPost("availability")]
    public async Task<IActionResult> UpdateAvailability([FromBody] UpdateAvailabilityRequest request)
    {
        var user = await ResolveUserAsync(request.UserID, request.Username);
        if (user == null)
        {
            return NotFound(new { success = false, message = "User not found." });
        }

        user.IsAvailable = request.IsAvailable;
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Availability updated." });
    }

    [HttpGet("insights")]
    public async Task<IActionResult> GetInsights([FromQuery] int userId)
    {
        var insights = await _aiAssistantService.BuildBhwInsightsAsync();
        return Ok(insights.Select((insight, index) => new
        {
            id = index + 1,
            title = insight.Title,
            description = insight.Description,
            severity = insight.Severity
        }));
    }

    [HttpPost("uploadProfilePic")]
    public async Task<IActionResult> UploadProfilePicture([FromBody] UploadProfilePictureRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Image))
        {
            return BadRequest(new { success = false, message = "Image is required." });
        }

        var user = await ResolveUserAsync(request.UserID, request.Username);
        if (user == null)
        {
            return NotFound(new { success = false, message = "User not found." });
        }

        try
        {
            var payload = request.Image.Trim();
            var commaIndex = payload.IndexOf(',');
            if (commaIndex >= 0)
            {
                payload = payload[(commaIndex + 1)..];
            }

            var bytes = Convert.FromBase64String(payload);
            var profilesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles");
            Directory.CreateDirectory(profilesDirectory);

            var fileName = $"{user.UserID}_{DateTime.UtcNow:yyyyMMddHHmmss}.png";
            var savePath = Path.Combine(profilesDirectory, fileName);
            await System.IO.File.WriteAllBytesAsync(savePath, bytes);

            user.ProfilePicture = $"/images/profiles/{fileName}";
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Profile picture uploaded.",
                profilePicture = user.ProfilePicture,
                profile = MapUserProfile(user)
            });
        }
        catch
        {
            return BadRequest(new { success = false, message = "Invalid image payload." });
        }
    }

    private async Task<User?> ResolveUserAsync(int? userId, string? username)
    {
        if (userId is int resolvedUserId && resolvedUserId > 0)
        {
            return await _context.Users.FindAsync(resolvedUserId);
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        var normalizedUsername = username.Trim().ToLowerInvariant();
        return await _context.Users.FirstOrDefaultAsync(user =>
            user.Role == "BHW" &&
            (user.Email.ToLower() == normalizedUsername ||
             user.Email.ToLower().StartsWith(normalizedUsername + "@")));
    }

    private async Task<List<int>> GetAssignedHouseholdIdsAsync(int userId)
    {
        return await _context.TaskAssignments
            .Where(task => task.BHWID == userId && task.HouseholdID.HasValue)
            .Select(task => task.HouseholdID!.Value)
            .Distinct()
            .ToListAsync();
    }

    private async Task<List<Household>> GetAccessibleHouseholdsAsync(int userId)
    {
        var assignedHouseholdIds = await GetAssignedHouseholdIdsAsync(userId);
        var query = _context.Households
            .Include(household => household.Members)
            .AsQueryable();

        if (assignedHouseholdIds.Count > 0)
        {
            query = query.Where(household => assignedHouseholdIds.Contains(household.HouseholdID));
        }

        return await query
            .OrderByDescending(household => household.RiskScore)
            .ThenBy(household => household.Address)
            .ToListAsync();
    }

    private async Task<HouseholdMember?> ResolvePatientAsync(int? patientId, int? userId = null)
    {
        if (patientId is not int resolvedPatientId || resolvedPatientId <= 0)
        {
            return null;
        }

        var query = _context.HouseholdMembers
            .Include(patient => patient.Household!)
            .ThenInclude(household => household.Members)
            .Where(patient => patient.MemberID == resolvedPatientId);

        if (userId is int resolvedUserId && resolvedUserId > 0)
        {
            var assignedHouseholdIds = await GetAssignedHouseholdIdsAsync(resolvedUserId);
            if (assignedHouseholdIds.Count > 0)
            {
                query = query.Where(patient => assignedHouseholdIds.Contains(patient.HouseholdID));
            }
        }

        return await query.FirstOrDefaultAsync();
    }

    private static object MapHousehold(Household household)
    {
        return new
        {
            id = household.HouseholdID,
            householdId = household.HouseholdID,
            name = household.HouseholdMember,
            memberCount = household.NumberOfMembers,
            risk = household.RiskScore ?? 0,
            address = household.Address,
            patients = household.GetOrderedMembers().Select(member => new
            {
                patientId = member.PatientID,
                patientName = member.FullName,
                contactNumber = member.ContactNumber,
                isEmergencyContact = member.IsEmergencyContact
            }).ToList()
        };
    }

    private static object MapHealthRecord(HealthRecord record)
    {
        var emergencyContact = record.Household?.GetEmergencyContact(record.PatientID);
        return new
        {
            id = record.RecordID,
            recordId = record.RecordID,
            householdId = record.Patient?.HouseholdID,
            householdName = record.Household?.HouseholdMember,
            householdAddress = record.Household?.Address,
            address = record.DisplayAddress,
            visitAddress = record.DisplayAddress,
            patientId = record.PatientID,
            patientName = record.Patient?.FullName,
            emergencyContactName = emergencyContact?.FullName,
            emergencyContactNumber = emergencyContact?.ContactNumber,
            dateRecorded = record.DateRecorded,
            disease = record.Disease,
            symptoms = record.Symptoms,
            status = record.Status
        };
    }

    private static object MapReport(Report report)
    {
        var emergencyContact = report.Household?.GetEmergencyContact(report.PatientID);
        return new
        {
            id = report.ReportID,
            reportId = report.ReportID,
            householdId = report.Patient?.HouseholdID,
            householdName = report.Household?.HouseholdMember,
            householdAddress = report.Household?.Address,
            address = report.DisplayAddress,
            visitAddress = report.DisplayAddress,
            patientId = report.PatientID,
            patientName = report.Patient?.FullName,
            emergencyContactName = emergencyContact?.FullName,
            emergencyContactNumber = emergencyContact?.ContactNumber,
            reportType = report.ReportType,
            content = report.Content,
            dateGenerated = report.DateGenerated
        };
    }

    private static object MapConsultationLog(Report report)
    {
        var emergencyContact = report.Household?.GetEmergencyContact(report.PatientID);
        return new
        {
            id = report.ReportID,
            reportId = report.ReportID,
            householdId = report.Patient?.HouseholdID,
            householdName = report.Household?.HouseholdMember,
            householdAddress = report.Household?.Address,
            address = report.DisplayAddress,
            visitAddress = report.DisplayAddress,
            patientId = report.PatientID,
            patientName = report.Patient?.FullName,
            emergencyContactName = emergencyContact?.FullName,
            emergencyContactNumber = emergencyContact?.ContactNumber,
            reportType = report.ReportType,
            content = report.Content,
            dateSubmitted = report.DateGenerated.ToString("g")
        };
    }

    private static IEnumerable<object> BuildPatientDirectory(IEnumerable<Household> households)
    {
        return households
            .OrderBy(household => household.HouseholdMember)
            .ThenBy(household => household.HouseholdID)
            .SelectMany(household => household.GetOrderedMembers()
                .Where(patient => patient.PatientID > 0)
                .Select(patient =>
                {
                    var emergencyContact = household.GetEmergencyContact(patient.PatientID);
                    return new
                    {
                        patientId = patient.PatientID,
                        patientName = patient.FullName,
                        householdId = household.HouseholdID,
                        householdName = household.HouseholdMember,
                        householdAddress = household.Address,
                        emergencyContactName = emergencyContact?.FullName,
                        emergencyContactNumber = emergencyContact?.ContactNumber,
                        searchText = string.Join(
                            " ",
                            new[]
                            {
                                patient.FullName,
                                household.HouseholdMember,
                                household.Address,
                                patient.ContactNumber,
                                emergencyContact?.FullName ?? string.Empty,
                                emergencyContact?.ContactNumber ?? string.Empty
                            }.Where(value => !string.IsNullOrWhiteSpace(value)))
                    };
                }))
            .ToList();
    }

    private static object MapUserProfile(User user, bool includeSuccess = false)
    {
        if (includeSuccess)
        {
            return new
            {
                success = true,
                userId = user.UserID,
                username = user.Email.Split('@')[0],
                bhwName = user.FullName,
                email = user.Email,
                contactNumber = user.ContactNumber,
                assignedArea = user.AssignedArea,
                isAvailable = user.IsAvailable,
                profilePicture = user.ProfilePicture
            };
        }

        return new
        {
            userId = user.UserID,
            username = user.Email.Split('@')[0],
            bhwName = user.FullName,
            email = user.Email,
            contactNumber = user.ContactNumber,
            assignedArea = user.AssignedArea,
            isAvailable = user.IsAvailable,
            profilePicture = user.ProfilePicture
        };
    }

    private static string? GetValidationError(object model)
    {
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, new ValidationContext(model), results, true);
        return isValid ? null : results[0].ErrorMessage;
    }

    private static string ResolveAddress(string? address, string? householdAddress, string? legacyVisitAddress, string? assignedArea)
    {
        return HouseholdIntakeHelper.NormalizeOptional(address)
            ?? HouseholdIntakeHelper.NormalizeOptional(householdAddress)
            ?? HouseholdIntakeHelper.NormalizeOptional(legacyVisitAddress)
            ?? HouseholdIntakeHelper.NormalizeOptional(assignedArea)
            ?? string.Empty;
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UpdateTaskStatusRequest
{
    public int TaskID { get; set; }
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class UpdateProfileRequest
{
    public int? UserID { get; set; }
    public string? Username { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? ContactNumber { get; set; }
    public string? AssignedArea { get; set; }
}

public class ChangePasswordRequest
{
    public int? UserID { get; set; }
    public string? Username { get; set; }
    public string? OldPassword { get; set; }
    public string? CurrentPassword { get; set; }
    public string NewPassword { get; set; } = string.Empty;
}

public class UpdateAvailabilityRequest
{
    public int? UserID { get; set; }
    public string? Username { get; set; }
    public bool IsAvailable { get; set; }
}

public class SubmitHealthRecordRequest
{
    public int UserID { get; set; }
    public string? PatientName { get; set; }
    public string? ContactNumber { get; set; }
    public DateTime? DateRecorded { get; set; }
    public string? Disease { get; set; }
    public string? Symptoms { get; set; }
    public string? Status { get; set; }
    public string? Address { get; set; }
    public string? HouseholdAddress { get; set; }
    public string? VisitAddress { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactNumber { get; set; }
}

public class SubmitConsultationRequest
{
    public int UserID { get; set; }
    public string? PatientName { get; set; }
    public string? ContactNumber { get; set; }
    public string? Address { get; set; }
    public string? HouseholdAddress { get; set; }
    public string? VisitAddress { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactNumber { get; set; }
    public DateTime? DateGenerated { get; set; }
    public string? ReportType { get; set; }
    public string? Content { get; set; }
}

public class LegacyMobileReportRequest
{
    public int UserID { get; set; }
    public int? PatientID { get; set; }
    public string? PatientName { get; set; }
    public string? ContactNumber { get; set; }
    public string? Address { get; set; }
    public string? HouseholdAddress { get; set; }
    public string? VisitAddress { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactNumber { get; set; }
    public DateTime? DateRecorded { get; set; }
    public DateTime? DateGenerated { get; set; }
    public string? Disease { get; set; }
    public string? Symptoms { get; set; }
    public string? Status { get; set; }
    public string? ReportType { get; set; }
    public string? Content { get; set; }
}

public class UploadProfilePictureRequest
{
    public int? UserID { get; set; }
    public string? Username { get; set; }
    public string Image { get; set; } = string.Empty;
}
