using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PHIMAS_PREDICTIVE_ANALYTICS.Data;
using PHIMAS_PREDICTIVE_ANALYTICS.Helpers;
using PHIMAS_PREDICTIVE_ANALYTICS.Models;
using PHIMAS_PREDICTIVE_ANALYTICS.Models.ViewModels;
using PHIMAS_PREDICTIVE_ANALYTICS.Services;

namespace PHIMAS_PREDICTIVE_ANALYTICS.Controllers;

[Authorize(Roles = "ADMIN")]
public class AdminController : AppControllerBase
{
    private readonly PredictiveAnalyticsService _predictiveAnalyticsService;
    private readonly AIAssistantService _aiAssistantService;
    private readonly FieldSubmissionService _fieldSubmissionService;

    public AdminController(
        AppDbContext context,
        PredictiveAnalyticsService predictiveAnalyticsService,
        AIAssistantService aiAssistantService,
        FieldSubmissionService fieldSubmissionService) : base(context)
    {
        _predictiveAnalyticsService = predictiveAnalyticsService;
        _aiAssistantService = aiAssistantService;
        _fieldSubmissionService = fieldSubmissionService;
    }

    public async Task<IActionResult> Dashboard()
    {
        ViewData["Active"] = "Dashboard";
        await _predictiveAnalyticsService.RecalculateHouseholdRisksAsync();

        var lowStockItems = await Context.Inventory
            .Where(item => (item.CurrentStock ?? 0) <= (item.MinimumThreshold ?? 0))
            .OrderBy(item => item.CurrentStock)
            .ToListAsync();

        var model = new AdminDashboardViewModel
        {
            Cards =
            [
                new() { Label = "Registered Users", Subtitle = "Active PHIMAS accounts", Value = await Context.Users.CountAsync() },
                new() { Label = "Households", Subtitle = "Tracked households", Value = await Context.Households.CountAsync() },
                new() { Label = "Open Health Cases", Subtitle = "Non-completed health records", Value = await Context.HealthRecords.CountAsync(record => record.Status != "Recovered" && record.Status != "Resolved"), AccentClass = "accent-red" },
                new() { Label = "Pending Tasks", Subtitle = "Field tasks requiring work", Value = await Context.TaskAssignments.CountAsync(task => task.Status != "Done"), AccentClass = "accent-amber" }
            ],
            RecentTasks = await BuildTaskListAsync(Context.TaskAssignments.OrderByDescending(task => task.TaskDate).Take(6)),
            RecentReports = await Context.Reports
                .Include(report => report.Patient)
                .ThenInclude(patient => patient!.Household)
                .ThenInclude(household => household!.Members)
                .OrderByDescending(report => report.DateGenerated)
                .Take(5)
                .ToListAsync(),
            LowStockItems = lowStockItems,
            ActivitySummary = await _aiAssistantService.BuildInsightsAsync()
        };

        return View(model);
    }

    public async Task<IActionResult> Admin(string? search)
    {
        ViewData["Active"] = "Admin";
        var usersQuery = Context.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            usersQuery = usersQuery.Where(user =>
                user.FullName.Contains(search) ||
                user.Email.Contains(search) ||
                user.Role.Contains(search));
        }

        var model = new UsersPageViewModel
        {
            Users = await usersQuery.OrderBy(user => user.Role).ThenBy(user => user.FullName).ToListAsync(),
            Search = search,
            Form = new User { Role = "BHW" }
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> SaveUser(UsersPageViewModel model)
    {
        var form = model.Form;
        form.Role = form.Role.Trim().ToUpperInvariant();
        form.Email = form.Email.Trim().ToLowerInvariant();

        if (form.Role is not ("ADMIN" or "CHO" or "BHW"))
        {
            TempData["Error"] = "Invalid role selected.";
            return RedirectToAction(nameof(Admin));
        }

        if (form.UserID == 0)
        {
            if (string.IsNullOrWhiteSpace(form.Password))
            {
                TempData["Error"] = "Password is required for a new user.";
                return RedirectToAction(nameof(Admin));
            }

            if (await Context.Users.AnyAsync(user => user.Email == form.Email))
            {
                TempData["Error"] = "Email already exists.";
                return RedirectToAction(nameof(Admin));
            }

            form.Password = PasswordHelper.HashPassword(form.Password);
            form.IsAvailable = form.Role == "BHW" && form.IsAvailable;
            Context.Users.Add(form);
        }
        else
        {
            var existingUser = await Context.Users.FindAsync(form.UserID);
            if (existingUser == null)
            {
                return RedirectToAction(nameof(Admin));
            }

            var emailInUse = await Context.Users.AnyAsync(user =>
                user.UserID != form.UserID &&
                user.Email == form.Email);
            if (emailInUse)
            {
                TempData["Error"] = "Email already exists.";
                return RedirectToAction(nameof(Admin));
            }

            existingUser.FullName = form.FullName;
            existingUser.Role = form.Role;
            existingUser.Email = form.Email;
            existingUser.ContactNumber = form.ContactNumber;
            existingUser.IsAvailable = form.Role == "BHW" && form.IsAvailable;
            existingUser.AssignedArea = form.AssignedArea;

            if (!string.IsNullOrWhiteSpace(form.Password))
            {
                existingUser.Password = PasswordHelper.HashPassword(form.Password);
            }
        }

        await Context.SaveChangesAsync();
        return RedirectToAction(nameof(Admin));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await Context.Users.FindAsync(id);
        if (user == null)
        {
            return RedirectToAction(nameof(Admin));
        }

        if (CurrentUserId == user.UserID)
        {
            TempData["Error"] = "You cannot delete the account that is currently signed in.";
            return RedirectToAction(nameof(Admin));
        }

        var hasLinkedHealthRecords = await Context.HealthRecords.AnyAsync(record => record.BHWID == user.UserID);
        var hasLinkedReports = await Context.Reports.AnyAsync(report => report.GeneratedBy == user.UserID);
        if (hasLinkedHealthRecords || hasLinkedReports)
        {
            TempData["Error"] = "User cannot be deleted while linked health records or reports still exist.";
            return RedirectToAction(nameof(Admin));
        }

        try
        {
            Context.Users.Remove(user);
            await Context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "User could not be deleted because it is still referenced by other records.";
        }

        return RedirectToAction(nameof(Admin));
    }

    public async Task<IActionResult> Households(string? search)
    {
        ViewData["Active"] = "Households";
        return View(await BuildHouseholdsPageAsync(search));
    }

    [HttpPost]
    public async Task<IActionResult> SaveHousehold(HouseholdsPageViewModel model)
    {
        var form = model.Form;
        if (string.IsNullOrWhiteSpace(form.Address) || string.IsNullOrWhiteSpace(form.HouseholdMember))
        {
            ModelState.AddModelError(string.Empty, "Household head and address are required.");
            ViewData["Active"] = "Households";
            return View("Households", await BuildHouseholdsPageAsync(null, form));
        }

        Household household;
        if (form.HouseholdID == 0)
        {
            household = new Household();
            Context.Households.Add(household);
        }
        else
        {
            household = await Context.Households
                .FirstOrDefaultAsync(item => item.HouseholdID == form.HouseholdID)
                ?? new Household();

            if (household.HouseholdID == 0)
            {
                TempData["Error"] = "Household not found.";
                return RedirectToAction(nameof(Households));
            }
        }

        household.Address = form.Address.Trim();
        household.RiskScore = form.RiskScore ?? 0f;
        household.HouseholdMember = form.HouseholdMember.Trim();
        household.MembersInput = form.MembersInput?.Trim() ?? string.Empty;

        try
        {
            await Context.SaveChangesAsync();
            await SyncHouseholdMembersAsync(household);
            await Context.SaveChangesAsync();
            TempData["Message"] = form.HouseholdID == 0
                ? "Household created successfully."
                : "Household updated successfully.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Household could not be saved because it conflicts with existing patient data.";
        }

        return RedirectToAction(nameof(Households));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteHousehold(int id)
    {
        var memberIds = await Context.HouseholdMembers
            .Where(member => member.HouseholdID == id)
            .Select(member => member.MemberID)
            .ToListAsync();

        if (memberIds.Count == 0)
        {
            var missingHousehold = await Context.Households.FindAsync(id);
            if (missingHousehold != null)
            {
                Context.Households.Remove(missingHousehold);
                await Context.SaveChangesAsync();
                TempData["Message"] = "Household deleted successfully.";
            }

            return RedirectToAction(nameof(Households));
        }

        var hasLinkedRecords = await Context.HealthRecords.AnyAsync(record =>
            record.PatientID.HasValue && memberIds.Contains(record.PatientID.Value));
        var hasLinkedReports = await Context.Reports.AnyAsync(report =>
            report.PatientID.HasValue && memberIds.Contains(report.PatientID.Value));

        if (hasLinkedRecords || hasLinkedReports)
        {
            TempData["Error"] = "Household cannot be deleted while linked reports or health records still exist.";
            return RedirectToAction(nameof(Households));
        }

        var household = await Context.Households.FindAsync(id);
        if (household != null)
        {
            Context.Households.Remove(household);
            await Context.SaveChangesAsync();
            TempData["Message"] = "Household deleted successfully.";
        }

        return RedirectToAction(nameof(Households));
    }

    public async Task<IActionResult> HealthRecords(string? search)
    {
        ViewData["Active"] = "HealthRecords";
        return View(await BuildHealthRecordsPageAsync(search));
    }

    [HttpPost]
    public async Task<IActionResult> SaveHealthRecord(HealthRecordsPageViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Active"] = "HealthRecords";
            return View("HealthRecords", await BuildHealthRecordsPageAsync(null, model.Form));
        }

        if (model.Form.BHWID is not int bhwId ||
            !await Context.Users.AnyAsync(user => user.UserID == bhwId && user.Role == "BHW"))
        {
            ModelState.AddModelError(string.Empty, "A valid BHW is required.");
            ViewData["Active"] = "HealthRecords";
            return View("HealthRecords", await BuildHealthRecordsPageAsync(null, model.Form));
        }

        try
        {
            await _fieldSubmissionService.UpsertHealthRecordAsync(model.Form.RecordID, bhwId, model.Form);
            TempData["Message"] = model.Form.RecordID > 0
                ? "Health record updated successfully."
                : "Health record created successfully.";
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            ViewData["Active"] = "HealthRecords";
            return View("HealthRecords", await BuildHealthRecordsPageAsync(null, model.Form));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Health record could not be saved because of a conflicting patient or household reference.");
            ViewData["Active"] = "HealthRecords";
            return View("HealthRecords", await BuildHealthRecordsPageAsync(null, model.Form));
        }

        return RedirectToAction(nameof(HealthRecords));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteHealthRecord(int id)
    {
        var record = await Context.HealthRecords.FindAsync(id);
        if (record != null)
        {
            Context.HealthRecords.Remove(record);
            await Context.SaveChangesAsync();
            await _predictiveAnalyticsService.RecalculateHouseholdRisksAsync();
            TempData["Message"] = "Health record deleted successfully.";
        }

        return RedirectToAction(nameof(HealthRecords));
    }

    public async Task<IActionResult> Inventory(string? search)
    {
        ViewData["Active"] = "Inventory";
        var query = Context.Inventory.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(item => item.ItemName.Contains(search) || item.Unit.Contains(search));
        }

        return View(new InventoryPageViewModel
        {
            Items = await query.OrderBy(item => item.ItemName).ToListAsync(),
            Search = search
        });
    }

    [HttpPost]
    public async Task<IActionResult> SaveInventory(InventoryPageViewModel model)
    {
        if (model.Form.ItemID == 0)
        {
            Context.Inventory.Add(model.Form);
        }
        else
        {
            var item = await Context.Inventory.FindAsync(model.Form.ItemID);
            if (item != null)
            {
                item.ItemName = model.Form.ItemName;
                item.Unit = model.Form.Unit;
                item.MinimumThreshold = model.Form.MinimumThreshold;
                item.CurrentStock = model.Form.CurrentStock;
            }
        }

        await Context.SaveChangesAsync();
        return RedirectToAction(nameof(Inventory));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteInventory(int id)
    {
        var item = await Context.Inventory.FindAsync(id);
        if (item != null)
        {
            Context.Inventory.Remove(item);
            await Context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Inventory));
    }

    public async Task<IActionResult> Reports(string? search)
    {
        ViewData["Active"] = "Reports";
        return View(await BuildReportsPageAsync(search));
    }

    [HttpPost]
    public async Task<IActionResult> SaveReport(ReportsPageViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Active"] = "Reports";
            return View("Reports", await BuildReportsPageAsync(null, model.Form));
        }

        if (model.Form.GeneratedBy is not int generatedBy ||
            !await Context.Users.AnyAsync(user => user.UserID == generatedBy))
        {
            ModelState.AddModelError(string.Empty, "A valid report owner is required.");
            ViewData["Active"] = "Reports";
            return View("Reports", await BuildReportsPageAsync(null, model.Form));
        }

        try
        {
            await _fieldSubmissionService.UpsertReportAsync(model.Form.ReportID, generatedBy, model.Form);
            TempData["Message"] = model.Form.ReportID > 0
                ? "Report updated successfully."
                : "Report created successfully.";
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            ViewData["Active"] = "Reports";
            return View("Reports", await BuildReportsPageAsync(null, model.Form));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Report could not be saved because of a conflicting patient or user reference.");
            ViewData["Active"] = "Reports";
            return View("Reports", await BuildReportsPageAsync(null, model.Form));
        }

        return RedirectToAction(nameof(Reports));
    }

    [HttpPost]
    public IActionResult GenerateAutomatedReport()
    {
        TempData["Error"] = "Automated report generation is disabled because reports must stay linked to households and patients.";
        return RedirectToAction(nameof(Reports));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteReport(int id)
    {
        var report = await Context.Reports.FindAsync(id);
        if (report != null)
        {
            Context.Reports.Remove(report);
            await Context.SaveChangesAsync();
            TempData["Message"] = "Report deleted successfully.";
        }

        return RedirectToAction(nameof(Reports));
    }

    public async Task<IActionResult> TaskMonitoring()
    {
        ViewData["Active"] = "TaskMonitoring";
        return View(new TaskMonitoringPageViewModel
        {
            Tasks = await BuildTaskListAsync(Context.TaskAssignments.OrderByDescending(task => task.TaskDate)),
            BHWs = await Context.Users.Where(user => user.Role == "BHW").OrderBy(user => user.FullName).ToListAsync(),
            Households = await Context.Households.OrderByDescending(household => household.RiskScore).ToListAsync(),
            Recommendations = await _aiAssistantService.BuildAssignmentRecommendationsAsync()
        });
    }

    [HttpPost]
    public async Task<IActionResult> SaveTask(TaskMonitoringPageViewModel model)
    {
        if (model.Form.BHWID == null && model.Form.HouseholdID.HasValue)
        {
            var recommendation = (await _aiAssistantService.BuildAssignmentRecommendationsAsync())
                .FirstOrDefault(item => item.HouseholdID == model.Form.HouseholdID.Value);
            model.Form.BHWID = recommendation?.RecommendedBHWID;
            if (!string.IsNullOrWhiteSpace(recommendation?.Reason))
            {
                model.Form.Description ??= recommendation.Reason;
            }
        }

        if (model.Form.TaskID == 0)
        {
            Context.TaskAssignments.Add(model.Form);
        }
        else
        {
            var task = await Context.TaskAssignments.FindAsync(model.Form.TaskID);
            if (task != null)
            {
                task.BHWID = model.Form.BHWID;
                task.HouseholdID = model.Form.HouseholdID;
                task.TaskDate = model.Form.TaskDate;
                task.Priority = model.Form.Priority;
                task.Status = model.Form.Status;
                task.Title = model.Form.Title;
                task.Description = string.IsNullOrWhiteSpace(model.Form.Description)
                    ? task.Description
                    : model.Form.Description;
            }
        }

        await Context.SaveChangesAsync();
        return RedirectToAction(nameof(TaskMonitoring));
    }

    [HttpPost]
    public async Task<IActionResult> AutoAssignHighRisk()
    {
        await _aiAssistantService.AutoAssignHighestRiskHouseholdAsync();
        return RedirectToAction(nameof(TaskMonitoring));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var task = await Context.TaskAssignments.FindAsync(id);
        if (task != null)
        {
            Context.TaskAssignments.Remove(task);
            await Context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(TaskMonitoring));
    }

    public async Task<IActionResult> PredictiveAnalytics()
    {
        ViewData["Active"] = "PredictiveAnalytics";
        var model = await _predictiveAnalyticsService.BuildPredictivePageAsync();
        model.Insights = await _aiAssistantService.BuildInsightsAsync();
        return View(model);
    }

    public async Task<IActionResult> AccountSet()
    {
        ViewData["Active"] = "AccountSet";
        var currentUser = await GetCurrentUserAsync();
        return View(currentUser ?? new User());
    }

    [HttpPost]
    public async Task<IActionResult> AccountSet(User updated)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser != null)
        {
            var normalizedEmail = updated.Email.Trim().ToLowerInvariant();
            var emailInUse = await Context.Users.AnyAsync(user =>
                user.UserID != currentUser.UserID &&
                user.Email == normalizedEmail);
            if (emailInUse)
            {
                TempData["Error"] = "Email already exists.";
                return RedirectToAction(nameof(AccountSet));
            }

            currentUser.FullName = updated.FullName;
            currentUser.Email = normalizedEmail;
            currentUser.ContactNumber = updated.ContactNumber;
            currentUser.AssignedArea = updated.AssignedArea;

            if (!string.IsNullOrWhiteSpace(updated.Password))
            {
                currentUser.Password = PasswordHelper.HashPassword(updated.Password);
            }

            await Context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(AccountSet));
    }

    private async Task<HouseholdsPageViewModel> BuildHouseholdsPageAsync(string? search, Household? form = null)
    {
        var query = Context.Households
            .Include(household => household.Members)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(household =>
                household.Address.Contains(search) ||
                household.Members.Any(member =>
                    member.FullName.Contains(search) ||
                    member.ContactNumber.Contains(search)));
        }

        return new HouseholdsPageViewModel
        {
            Households = await query
                .OrderByDescending(household => household.RiskScore ?? 0)
                .ThenBy(household => household.Address)
                .ToListAsync(),
            Search = search,
            Form = form ?? new Household()
        };
    }

    private async Task<HealthRecordsPageViewModel> BuildHealthRecordsPageAsync(
        string? search,
        CreateHealthRecordViewModel? form = null)
    {
        var query = Context.HealthRecords
            .Include(record => record.Patient)
            .ThenInclude(patient => patient!.Household)
            .ThenInclude(household => household!.Members)
            .Include(record => record.BHW)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(record =>
                (record.Patient != null && record.Patient.FullName.Contains(search)) ||
                record.Disease.Contains(search) ||
                record.Status.Contains(search) ||
                (record.Patient != null &&
                 record.Patient.Household != null && (
                    record.Patient.Household.Address.Contains(search) ||
                    record.Patient.Household.Members.Any(member =>
                        member.IsEmergencyContact &&
                        (member.FullName.Contains(search) ||
                         member.ContactNumber.Contains(search))))));
        }

        var households = await Context.Households
            .Include(household => household.Members)
            .OrderBy(household => household.Address)
            .ToListAsync();
        var bhws = await Context.Users
            .Where(user => user.Role == "BHW")
            .OrderBy(user => user.FullName)
            .ToListAsync();
        var resolvedForm = form ?? new CreateHealthRecordViewModel
        {
            DateRecorded = DateTime.Today,
            Status = "Submitted",
            BHWID = bhws.FirstOrDefault()?.UserID
        };

        return new HealthRecordsPageViewModel
        {
            HealthRecords = await query.OrderByDescending(record => record.DateRecorded).ToListAsync(),
            Patients = BuildPatientDirectory(households),
            BHWs = bhws,
            Search = search,
            Form = resolvedForm
        };
    }

    private async Task<ReportsPageViewModel> BuildReportsPageAsync(
        string? search,
        CreateReportViewModel? form = null)
    {
        await _predictiveAnalyticsService.RecalculateHouseholdRisksAsync();

        var reportsQuery = Context.Reports
            .Include(report => report.Patient)
            .ThenInclude(patient => patient!.Household)
            .ThenInclude(household => household!.Members)
            .Include(report => report.GeneratedByUser)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            reportsQuery = reportsQuery.Where(report =>
                report.ReportType.Contains(search) ||
                report.Content.Contains(search) ||
                (report.Patient != null && report.Patient.FullName.Contains(search)) ||
                (report.Patient != null &&
                 report.Patient.Household != null &&
                 report.Patient.Household.Address.Contains(search)));
        }

        var households = await Context.Households
            .Include(household => household.Members)
            .OrderBy(household => household.Address)
            .ToListAsync();
        var users = await Context.Users.OrderBy(user => user.FullName).ToListAsync();
        var resolvedForm = form ?? new CreateReportViewModel
        {
            DateGenerated = DateTime.Today,
            ReportType = "Consultation Log",
            GeneratedBy = CurrentUserId ?? users.FirstOrDefault()?.UserID
        };

        return new ReportsPageViewModel
        {
            Reports = await reportsQuery.OrderByDescending(report => report.DateGenerated).ToListAsync(),
            Users = users,
            Patients = BuildPatientDirectory(households),
            Insights = await _aiAssistantService.BuildInsightsAsync(),
            Search = search,
            TotalScope = await Context.Households.CountAsync(),
            DataPoints = await Context.HealthRecords.CountAsync(),
            RiskIndex = await BuildRiskIndexAsync(),
            Form = resolvedForm
        };
    }

    private async Task<string> BuildRiskIndexAsync()
    {
        var averageRisk = await Context.Households
            .Select(household => (double?)(household.RiskScore ?? 0))
            .AverageAsync() ?? 0d;

        return averageRisk switch
        {
            >= 70 => $"High ({averageRisk:0.0})",
            >= 40 => $"Moderate ({averageRisk:0.0})",
            _ => $"Low ({averageRisk:0.0})"
        };
    }

    private async Task<List<TaskListItemViewModel>> BuildTaskListAsync(IQueryable<TaskAssignment> query)
    {
        var tasks = await query
            .Include(task => task.BHW)
            .Include(task => task.Household)
            .ToListAsync();

        return tasks.Select(task => new TaskListItemViewModel
        {
            TaskID = task.TaskID,
            Title = task.Title ?? $"Visit Household #{task.HouseholdID}",
            Description = task.Description ?? "Field task assigned through PHIMAS.",
            Priority = task.Priority,
            Status = task.Status,
            TaskDate = task.TaskDate,
            BHWID = task.BHWID,
            BHWName = task.BHW?.FullName ?? "Unassigned",
            HouseholdID = task.HouseholdID,
            HouseholdName = task.Household?.HouseholdMember ?? "Unknown household",
            HouseholdAddress = task.Household?.Address ?? "No address"
        }).ToList();
    }
}
