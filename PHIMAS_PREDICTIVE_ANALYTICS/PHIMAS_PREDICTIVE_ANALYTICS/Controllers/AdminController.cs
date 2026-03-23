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

    public AdminController(
        AppDbContext context,
        PredictiveAnalyticsService predictiveAnalyticsService,
        AIAssistantService aiAssistantService) : base(context)
    {
        _predictiveAnalyticsService = predictiveAnalyticsService;
        _aiAssistantService = aiAssistantService;
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
        if (user != null)
        {
            Context.Users.Remove(user);
            await Context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Admin));
    }

    public async Task<IActionResult> Households(string? search)
    {
        ViewData["Active"] = "Households";
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

        return View(new HouseholdsPageViewModel
        {
            Households = await query.OrderByDescending(household => household.RiskScore).ToListAsync(),
            Search = search
        });
    }

    [HttpPost]
    public IActionResult SaveHousehold(HouseholdsPageViewModel model)
    {
        TempData["Error"] = "Household creation and editing are restricted to BHW submissions.";
        return RedirectToAction(nameof(Households));
    }

    [HttpPost]
    public IActionResult DeleteHousehold(int id)
    {
        TempData["Error"] = "Household deletion is disabled to preserve the household source of truth.";
        return RedirectToAction(nameof(Households));
    }

    public async Task<IActionResult> HealthRecords(string? search)
    {
        ViewData["Active"] = "HealthRecords";
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
        var patients = BuildPatientDirectory(households);

        return View(new HealthRecordsPageViewModel
        {
            HealthRecords = await query.OrderByDescending(record => record.DateRecorded).ToListAsync(),
            Patients = patients,
            BHWs = await Context.Users.Where(user => user.Role == "BHW").OrderBy(user => user.FullName).ToListAsync(),
            Search = search,
            Form = new CreateHealthRecordViewModel
            {
                DateRecorded = DateTime.Today
            }
        });
    }

    [HttpPost]
    public IActionResult SaveHealthRecord(HealthRecordsPageViewModel model)
    {
        TempData["Error"] = "Health records are read-only for admin. New entries must come from BHW submissions.";
        return RedirectToAction(nameof(HealthRecords));
    }

    [HttpPost]
    public IActionResult DeleteHealthRecord(int id)
    {
        TempData["Error"] = "Health record deletion is disabled to preserve linked household data.";
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
        await _predictiveAnalyticsService.RecalculateHouseholdRisksAsync();

        var reportsQuery = Context.Reports
            .Include(report => report.Patient)
            .ThenInclude(patient => patient!.Household)
            .ThenInclude(household => household!.Members)
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
        var patients = BuildPatientDirectory(households);

        return View(new ReportsPageViewModel
        {
            Reports = await reportsQuery.OrderByDescending(report => report.DateGenerated).ToListAsync(),
            Users = await Context.Users.OrderBy(user => user.FullName).ToListAsync(),
            Patients = patients,
            Insights = await _aiAssistantService.BuildInsightsAsync(),
            Search = search,
            TotalScope = await Context.Households.CountAsync(),
            DataPoints = await Context.HealthRecords.CountAsync(),
            RiskIndex = await BuildRiskIndexAsync(),
            Form = new CreateReportViewModel
            {
                DateGenerated = DateTime.Today,
                ReportType = "Consultation Log"
            }
        });
    }

    [HttpPost]
    public IActionResult SaveReport(ReportsPageViewModel model)
    {
        TempData["Error"] = "Reports are read-only for admin. New field-linked reports must come from BHW submissions.";
        return RedirectToAction(nameof(Reports));
    }

    [HttpPost]
    public IActionResult GenerateAutomatedReport()
    {
        TempData["Error"] = "Automated report generation is disabled because reports must stay linked to households and patients.";
        return RedirectToAction(nameof(Reports));
    }

    [HttpPost]
    public IActionResult DeleteReport(int id)
    {
        TempData["Error"] = "Report deletion is disabled to preserve linked household data.";
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
            currentUser.FullName = updated.FullName;
            currentUser.Email = updated.Email;
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

    private async Task<string> BuildRiskIndexAsync()
    {
        var averageRisk = await Context.Households.AverageAsync(household => household.RiskScore ?? 0);
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
