using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PHIMAS_PREDICTIVE_ANALYTICS.Data;
using PHIMAS_PREDICTIVE_ANALYTICS.Helpers;
using PHIMAS_PREDICTIVE_ANALYTICS.Models;
using PHIMAS_PREDICTIVE_ANALYTICS.Models.ViewModels;
using PHIMAS_PREDICTIVE_ANALYTICS.Services;

namespace PHIMAS_PREDICTIVE_ANALYTICS.Controllers;

[Authorize(Roles = "BHW")]
public class BHWController : AppControllerBase
{
    private readonly AIAssistantService _aiAssistantService;
    private readonly FieldSubmissionService _fieldSubmissionService;

    public BHWController(
        AppDbContext context,
        AIAssistantService aiAssistantService,
        FieldSubmissionService fieldSubmissionService) : base(context)
    {
        _aiAssistantService = aiAssistantService;
        _fieldSubmissionService = fieldSubmissionService;
    }

    public async Task<IActionResult> Dashboard()
    {
        ViewData["Active"] = "Dashboard";
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null)
        {
            return RedirectToAction("SignIn", "Account");
        }

        var tasks = await Context.TaskAssignments
            .Include(task => task.Household)
            .Where(task => task.BHWID == currentUser.UserID)
            .OrderBy(task => task.TaskDate)
            .ToListAsync();

        var assigned = tasks.Count;
        var highPriority = tasks.Count(task => task.Priority == "High");
        var dueToday = tasks.Count(task => task.TaskDate.Date == DateTime.Today);
        var completed = tasks.Count(task => task.Status == "Done");
        var progress = assigned > 0 ? (completed * 100) / assigned : 0;

        var model = new BhwDashboardViewModel
        {
            Worker = currentUser,
            AssignedTasks = assigned,
            HighPriorityTasks = highPriority,
            DueToday = dueToday,
            CompletedTasks = completed,
            ProgressPercent = progress,
            TodayTasks = tasks.Where(task => task.TaskDate.Date == DateTime.Today).Select(MapTask).ToList(),
            RecentConsultations = await Context.Reports
                .Include(report => report.Patient)
                .ThenInclude(patient => patient!.Household)
                .ThenInclude(household => household!.Members)
                .Where(report => report.GeneratedBy == currentUser.UserID)
                .OrderByDescending(report => report.DateGenerated)
                .Take(4)
                .ToListAsync(),
            Insights = (await _aiAssistantService.BuildInsightsAsync()).Take(3).ToList()
        };

        return View(model);
    }

    public async Task<IActionResult> Task()
    {
        ViewData["Active"] = "Task";
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null)
        {
            return RedirectToAction("SignIn", "Account");
        }

        var tasks = await Context.TaskAssignments
            .Include(task => task.Household)
            .Where(task => task.BHWID == currentUser.UserID)
            .OrderBy(task => task.TaskDate)
            .ToListAsync();

        return View(tasks.Select(MapTask).ToList());
    }

    public async Task<IActionResult> HealthRecords(string? search)
    {
        ViewData["Active"] = "HealthRecords";
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null)
        {
            return RedirectToAction("SignIn", "Account");
        }

        return View(await BuildHealthRecordsPageAsync(currentUser, search));
    }

    [HttpPost]
    public async Task<IActionResult> AddHealthRecord(HealthRecordsPageViewModel model)
    {
        ViewData["Active"] = "HealthRecords";
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null)
        {
            return RedirectToAction("SignIn", "Account");
        }

        if (!ModelState.IsValid)
        {
            return View("HealthRecords", await BuildHealthRecordsPageAsync(currentUser, null, model.Form));
        }

        try
        {
            await _fieldSubmissionService.CreateHealthRecordAsync(currentUser.UserID, model.Form);
            TempData["Message"] = "Health record submitted successfully.";
            return RedirectToAction(nameof(HealthRecords));
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View("HealthRecords", await BuildHealthRecordsPageAsync(currentUser, null, model.Form));
        }
    }

    public async Task<IActionResult> Reports(string? search)
    {
        ViewData["Active"] = "Reports";
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null)
        {
            return RedirectToAction("SignIn", "Account");
        }

        return View(await BuildReportsPageAsync(currentUser, search));
    }

    [HttpPost]
    public async Task<IActionResult> SubmitReport(ReportsPageViewModel model)
    {
        ViewData["Active"] = "Reports";
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null)
        {
            return RedirectToAction("SignIn", "Account");
        }

        if (!ModelState.IsValid)
        {
            return View("Reports", await BuildReportsPageAsync(currentUser, null, model.Form));
        }

        try
        {
            await _fieldSubmissionService.CreateReportAsync(currentUser.UserID, model.Form);
            TempData["Message"] = "Consultation log submitted successfully.";
            return RedirectToAction(nameof(Reports));
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View("Reports", await BuildReportsPageAsync(currentUser, null, model.Form));
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateTaskStatus(int taskId, string status)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null)
        {
            return RedirectToAction("SignIn", "Account");
        }

        var task = await Context.TaskAssignments.FirstOrDefaultAsync(item => item.TaskID == taskId && item.BHWID == currentUser.UserID);
        if (task != null)
        {
            task.Status = status;
            await Context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Task));
    }

    [HttpPost]
    public async Task<IActionResult> ToggleAvailability(bool available)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser != null)
        {
            currentUser.IsAvailable = available;
            await Context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Dashboard));
    }

    public async Task<IActionResult> Account()
    {
        ViewData["Active"] = "Account";
        return View(await GetCurrentUserAsync() ?? new User());
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProfile(User updated)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser != null)
        {
            currentUser.FullName = updated.FullName;
            currentUser.Email = updated.Email;
            currentUser.ContactNumber = updated.ContactNumber;
            currentUser.AssignedArea = updated.AssignedArea;
            await Context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Account));
    }

    [HttpPost]
    public async Task<IActionResult> UpdatePassword(string currentPassword, string newPassword)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser != null && PasswordHelper.VerifyPassword(currentPassword, currentUser.Password))
        {
            currentUser.Password = PasswordHelper.HashPassword(newPassword);
            await Context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Account));
    }

    [HttpPost]
    public async Task<IActionResult> UploadProfilePicture(IFormFile? picture)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null || picture == null || picture.Length == 0)
        {
            return RedirectToAction(nameof(Account));
        }

        var profilesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles");
        Directory.CreateDirectory(profilesDirectory);

        var fileName = $"{currentUser.UserID}_{Path.GetFileName(picture.FileName)}";
        var savePath = Path.Combine(profilesDirectory, fileName);
        await using var stream = System.IO.File.Create(savePath);
        await picture.CopyToAsync(stream);

        currentUser.ProfilePicture = $"/images/profiles/{fileName}";
        await Context.SaveChangesAsync();

        return RedirectToAction(nameof(Account));
    }

    public IActionResult Logout()
    {
        return RedirectToAction("SignOut", "Account");
    }

    private async Task<HealthRecordsPageViewModel> BuildHealthRecordsPageAsync(
        User currentUser,
        string? search,
        CreateHealthRecordViewModel? form = null)
    {
        var query = Context.HealthRecords
            .Include(record => record.BHW)
            .Include(record => record.Patient)
            .ThenInclude(patient => patient!.Household)
            .ThenInclude(household => household!.Members)
            .Where(record => record.BHWID == currentUser.UserID)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(record =>
                (record.Patient != null && (
                    record.Patient.FullName.Contains(search) ||
                    record.Patient.ContactNumber.Contains(search))) ||
                record.Disease.Contains(search) ||
                record.Status.Contains(search) ||
                (record.Patient != null &&
                 record.Patient.Household != null &&
                 (record.Patient.Household.Address.Contains(search) ||
                  record.Patient.Household.Members.Any(member =>
                      member.IsEmergencyContact &&
                      (member.FullName.Contains(search) ||
                       member.ContactNumber.Contains(search))))));
        }

        return new HealthRecordsPageViewModel
        {
            HealthRecords = await query.OrderByDescending(record => record.DateRecorded).ToListAsync(),
            BHWs = [currentUser],
            AssignedArea = currentUser.AssignedArea?.Trim() ?? string.Empty,
            Search = search,
            Form = form ?? new CreateHealthRecordViewModel
            {
                DateRecorded = DateTime.Today,
                Address = currentUser.AssignedArea?.Trim() ?? string.Empty,
                Status = "Submitted"
            }
        };
    }

    private async Task<ReportsPageViewModel> BuildReportsPageAsync(
        User currentUser,
        string? search,
        CreateReportViewModel? form = null)
    {
        var query = Context.Reports
            .Include(report => report.Patient)
            .ThenInclude(patient => patient!.Household)
            .ThenInclude(household => household!.Members)
            .Where(report => report.GeneratedBy == currentUser.UserID)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(report =>
                report.ReportType.Contains(search) ||
                report.Content.Contains(search) ||
                (report.Patient != null && (
                    report.Patient.FullName.Contains(search) ||
                    report.Patient.ContactNumber.Contains(search))) ||
                (report.Patient != null &&
                 report.Patient.Household != null &&
                 (report.Patient.Household.Address.Contains(search) ||
                  report.Patient.Household.Members.Any(member =>
                      member.IsEmergencyContact &&
                      (member.FullName.Contains(search) ||
                       member.ContactNumber.Contains(search))))));
        }

        return new ReportsPageViewModel
        {
            Reports = await query.OrderByDescending(report => report.DateGenerated).ToListAsync(),
            Users = [currentUser],
            Insights = (await _aiAssistantService.BuildInsightsAsync()).Take(2).ToList(),
            AssignedArea = currentUser.AssignedArea?.Trim() ?? string.Empty,
            Search = search,
            TotalScope = await Context.Households.CountAsync(),
            DataPoints = await Context.HealthRecords.CountAsync(),
            RiskIndex = "Field View",
            Form = form ?? new CreateReportViewModel
            {
                DateGenerated = DateTime.Today,
                ReportType = "Consultation Log",
                Address = currentUser.AssignedArea?.Trim() ?? string.Empty
            }
        };
    }

    private static TaskListItemViewModel MapTask(TaskAssignment task)
    {
        return new TaskListItemViewModel
        {
            TaskID = task.TaskID,
            Title = task.Title ?? $"Visit {task.Household?.HouseholdMember ?? $"Household #{task.HouseholdID}"}",
            Description = string.IsNullOrWhiteSpace(task.Description)
                ? task.Household?.Address ?? "No task details provided."
                : task.Description,
            Priority = task.Priority,
            Status = task.Status,
            TaskDate = task.TaskDate,
            BHWID = task.BHWID,
            BHWName = task.BHW?.FullName ?? string.Empty,
            HouseholdID = task.HouseholdID,
            HouseholdName = task.Household?.HouseholdMember ?? string.Empty,
            HouseholdAddress = task.Household?.Address ?? string.Empty
        };
    }

    private async Task<List<int>> GetAccessibleHouseholdIdsAsync(int userId)
    {
        return await Context.TaskAssignments
            .Where(task => task.BHWID == userId && task.HouseholdID.HasValue)
            .Select(task => task.HouseholdID!.Value)
            .Distinct()
            .ToListAsync();
    }

    private async Task<List<Household>> GetAccessibleHouseholdsAsync(int userId)
    {
        var assignedHouseholdIds = await GetAccessibleHouseholdIdsAsync(userId);

        var query = Context.Households.AsQueryable();
        if (assignedHouseholdIds.Count > 0)
        {
            query = query.Where(household => assignedHouseholdIds.Contains(household.HouseholdID));
        }

        return await query
            .Include(household => household.Members)
            .OrderBy(household => household.Address)
            .ToListAsync();
    }
}
