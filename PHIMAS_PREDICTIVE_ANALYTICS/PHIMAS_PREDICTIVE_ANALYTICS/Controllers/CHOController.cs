using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PHIMAS_PREDICTIVE_ANALYTICS.Data;
using PHIMAS_PREDICTIVE_ANALYTICS.Helpers;
using PHIMAS_PREDICTIVE_ANALYTICS.Models;
using PHIMAS_PREDICTIVE_ANALYTICS.Models.ViewModels;
using PHIMAS_PREDICTIVE_ANALYTICS.Services;

namespace PHIMAS_PREDICTIVE_ANALYTICS.Controllers;

[Authorize(Roles = "CHO")]
public class CHOController : AppControllerBase
{
    private readonly PredictiveAnalyticsService _predictiveAnalyticsService;
    private readonly AIAssistantService _aiAssistantService;

    public CHOController(
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
        var forecasts = await _predictiveAnalyticsService.GetForecastsAsync();
        var highRiskBarangaySnapshot = await _predictiveAnalyticsService.GetHighRiskBarangaySnapshotAsync();

        var model = new ChoDashboardViewModel
        {
            Cards =
            [
                new() { Label = "Disease Cases", Subtitle = "All captured records", Value = await Context.HealthRecords.CountAsync(), AccentClass = "accent-red" },
                new() { Label = "High-Risk Barangays", Subtitle = "Latest predicted outbreak areas", Value = highRiskBarangaySnapshot.TotalBarangays, AccentClass = "accent-amber" },
                new() { Label = "Active BHWs", Subtitle = "Available workers in field", Value = await Context.Users.CountAsync(user => user.Role == "BHW" && user.IsAvailable) },
                new() { Label = "Outbreak Alerts", Subtitle = "Forecast confidence >= 80%", Value = forecasts.Count(forecast => forecast.ConfidenceScore >= 0.8f), AccentClass = "accent-purple" }
            ],
            DiseaseTrend = await _predictiveAnalyticsService.GetDiseaseTrendAsync(),
            HighRiskBarangayTrend = highRiskBarangaySnapshot.Barangays
                .Select(item => new ChartPointViewModel
                {
                    Label = item.Barangay,
                    Value = item.TotalCases
                })
                .ToList(),
            Forecasts = forecasts,
            HighRiskBarangaySnapshot = highRiskBarangaySnapshot,
            Insights = await _aiAssistantService.BuildInsightsAsync()
        };

        return View("~/Views/Core/Dashboard.cshtml", model);
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

        return View("~/Views/Core/Households.cshtml", new HouseholdsPageViewModel
        {
            Households = await query.OrderByDescending(household => household.RiskScore).ToListAsync(),
            Search = search
        });
    }

    [HttpPost]
    public IActionResult SaveHousehold(HouseholdsPageViewModel model)
    {
        TempData["Error"] = "CHO access is read-only for households.";
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

        return View("~/Views/Core/HealthRecords.cshtml", new HealthRecordsPageViewModel
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
        TempData["Error"] = "CHO access is read-only for health records.";
        return RedirectToAction(nameof(HealthRecords));
    }

    public async Task<IActionResult> TaskMonitoring()
    {
        ViewData["Active"] = "TaskMonitoring";
        var tasks = await Context.TaskAssignments
            .Include(task => task.BHW)
            .Include(task => task.Household)
            .OrderByDescending(task => task.TaskDate)
            .ToListAsync();

        return View("~/Views/Core/TaskMonitoring.cshtml", new TaskMonitoringPageViewModel
        {
            Tasks = tasks.Select(task => new TaskListItemViewModel
            {
                TaskID = task.TaskID,
                Title = task.Title ?? $"Visit Household #{task.HouseholdID}",
                Description = task.Description ?? string.Empty,
                Priority = task.Priority,
                Status = task.Status,
                TaskDate = task.TaskDate,
                BHWID = task.BHWID,
                BHWName = task.BHW?.FullName ?? "Unassigned",
                HouseholdID = task.HouseholdID,
                HouseholdName = task.Household?.HouseholdMember ?? "Unknown household",
                HouseholdAddress = task.Household?.Address ?? string.Empty
            }).ToList(),
            BHWs = await Context.Users.Where(user => user.Role == "BHW").OrderBy(user => user.FullName).ToListAsync(),
            Households = await Context.Households.OrderByDescending(household => household.RiskScore).ToListAsync(),
            Recommendations = await _aiAssistantService.BuildAssignmentRecommendationsAsync()
        });
    }

    [HttpPost]
    public async Task<IActionResult> SaveTask(TaskMonitoringPageViewModel model)
    {
        if (model.Form.TaskID == 0)
        {
            Context.TaskAssignments.Add(model.Form);
        }
        else
        {
            var existingTask = await Context.TaskAssignments.FindAsync(model.Form.TaskID);
            if (existingTask != null)
            {
                existingTask.BHWID = model.Form.BHWID;
                existingTask.HouseholdID = model.Form.HouseholdID;
                existingTask.TaskDate = model.Form.TaskDate;
                existingTask.Priority = model.Form.Priority;
                existingTask.Status = model.Form.Status;
                existingTask.Title = model.Form.Title;
                existingTask.Description = model.Form.Description;
            }
        }

        await Context.SaveChangesAsync();
        return RedirectToAction(nameof(TaskMonitoring));
    }

    public async Task<IActionResult> Reports(string? search)
    {
        ViewData["Active"] = "Reports";
        await _predictiveAnalyticsService.RecalculateHouseholdRisksAsync();

        var query = Context.Reports
            .Include(report => report.Patient)
            .ThenInclude(patient => patient!.Household)
            .ThenInclude(household => household!.Members)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(report =>
                report.ReportType.Contains(search) ||
                report.Content.Contains(search) ||
                (report.Patient != null && report.Patient.FullName.Contains(search)) ||
                (report.Patient != null &&
                 report.Patient.Household != null &&
                 report.Patient.Household.Address.Contains(search)));
        }

        var averageRisk = await Context.Households.AverageAsync(household => household.RiskScore ?? 0);
        var riskIndex = averageRisk >= 70 ? $"High ({averageRisk:0.0})" : averageRisk >= 40 ? $"Moderate ({averageRisk:0.0})" : $"Low ({averageRisk:0.0})";
        var households = await Context.Households
            .Include(household => household.Members)
            .OrderBy(household => household.Address)
            .ToListAsync();
        var patients = BuildPatientDirectory(households);

        return View("~/Views/Core/Reports.cshtml", new ReportsPageViewModel
        {
            Reports = await query.OrderByDescending(report => report.DateGenerated).ToListAsync(),
            Users = await Context.Users.OrderBy(user => user.FullName).ToListAsync(),
            Patients = patients,
            Insights = await _aiAssistantService.BuildInsightsAsync(),
            Search = search,
            TotalScope = await Context.Households.CountAsync(),
            DataPoints = await Context.HealthRecords.CountAsync(),
            RiskIndex = riskIndex,
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
        TempData["Error"] = "CHO access is read-only for reports.";
        return RedirectToAction(nameof(Reports));
    }

    public async Task<IActionResult> PredictiveAnalytics()
    {
        ViewData["Active"] = "PredictiveAnalytics";
        var model = await _predictiveAnalyticsService.BuildPredictivePageAsync();
        model.Insights = await _aiAssistantService.BuildInsightsAsync();
        return View("~/Views/Core/PredictiveAnalytics.cshtml", model);
    }

    public async Task<IActionResult> AccountSet()
    {
        ViewData["Active"] = "AccountSet";
        return View("~/Views/Core/AccountSet.cshtml", await GetCurrentUserAsync() ?? new User());
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
}
