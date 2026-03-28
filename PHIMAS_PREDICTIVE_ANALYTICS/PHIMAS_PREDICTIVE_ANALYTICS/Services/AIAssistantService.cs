using Microsoft.EntityFrameworkCore;
using PHIMAS_PREDICTIVE_ANALYTICS.Data;
using PHIMAS_PREDICTIVE_ANALYTICS.Models;
using PHIMAS_PREDICTIVE_ANALYTICS.Models.ViewModels;

namespace PHIMAS_PREDICTIVE_ANALYTICS.Services;

public class AIAssistantService
{
    private readonly AppDbContext _context;
    private readonly PredictiveAnalyticsService _predictiveAnalyticsService;

    public AIAssistantService(AppDbContext context, PredictiveAnalyticsService predictiveAnalyticsService)
    {
        _context = context;
        _predictiveAnalyticsService = predictiveAnalyticsService;
    }

    public async Task<List<AssistantInsightViewModel>> BuildInsightsAsync()
    {
        var trendInsight = await BuildHealthTrendInsightAsync();
        var overdueTasks = await _context.TaskAssignments.CountAsync(task => task.TaskDate < DateTime.Today && task.Status != "Done");
        var lowStock = await _context.Inventory.CountAsync(item => (item.CurrentStock ?? 0) <= (item.MinimumThreshold ?? 0));
        var topForecast = await _predictiveAnalyticsService.GetLatestStoredAnalysisAsync();

        var insights = new List<AssistantInsightViewModel>
        {
            trendInsight,
            new()
            {
                Type = "Task",
                Title = "Field deployment recommendation",
                Description = $"{overdueTasks} tasks need reassignment or follow-up to keep BHW response times within target.",
                Severity = overdueTasks > 0 ? "Medium" : "Info"
            },
            new()
            {
                Type = "Supply",
                Title = "Inventory alert",
                Description = $"{lowStock} inventory items are at or below threshold and should be replenished.",
                Severity = lowStock > 0 ? "High" : "Info"
            }
        };

        if (topForecast != null)
        {
            insights.Add(new AssistantInsightViewModel
            {
                Type = "Forecast",
                Title = "Potential outbreak signal",
                Description = $"{topForecast.Disease} shows the strongest projected increase in {topForecast.HighRiskBarangay} with {(topForecast.ConfidenceScore * 100):0}% confidence.",
                Severity = topForecast.ConfidenceScore >= 0.8f ? "High" : "Medium"
            });
        }

        return insights;
    }

    public async Task<List<AssistantInsightViewModel>> BuildBhwInsightsAsync()
    {
        return [await BuildHealthTrendInsightAsync()];
    }

    public async Task<List<AssignmentRecommendationViewModel>> BuildAssignmentRecommendationsAsync()
    {
        await _predictiveAnalyticsService.RecalculateHouseholdRisksAsync();

        var households = await _context.Households
            .Where(household => (household.RiskScore ?? 0) >= 60)
            .OrderByDescending(household => household.RiskScore)
            .Take(5)
            .ToListAsync();

        var bhws = await _context.Users.Where(user => user.Role == "BHW").ToListAsync();
        var taskLoad = await _context.TaskAssignments
            .Where(task => task.Status != "Done" && task.BHWID != null)
            .Select(task => new { BHWID = task.BHWID!.Value })
            .GroupBy(task => task.BHWID)
            .Select(group => new { BHWID = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.BHWID, item => item.Count);

        var recommendations = new List<AssignmentRecommendationViewModel>();
        foreach (var household in households)
        {
            var bestWorker = bhws
                .Select(worker => new
                {
                    Worker = worker,
                    Score = BuildWorkerScore(worker, household, taskLoad.TryGetValue(worker.UserID, out var openTasks) ? openTasks : 0)
                })
                .OrderByDescending(item => item.Score)
                .FirstOrDefault();

            recommendations.Add(new AssignmentRecommendationViewModel
            {
                HouseholdID = household.HouseholdID,
                HouseholdName = household.HouseholdMember,
                Address = household.Address,
                RiskScore = household.RiskScore ?? 0,
                RecommendedBHWID = bestWorker?.Worker.UserID,
                RecommendedBHWName = bestWorker?.Worker.FullName ?? "Unassigned",
                Reason = BuildRecommendationReason(bestWorker?.Worker, household, taskLoad)
            });
        }

        return recommendations;
    }

    public async Task<TaskAssignment?> AutoAssignHighestRiskHouseholdAsync()
    {
        var recommendation = (await BuildAssignmentRecommendationsAsync()).FirstOrDefault();
        if (recommendation?.RecommendedBHWID == null)
        {
            return null;
        }

        var existingTask = await _context.TaskAssignments.FirstOrDefaultAsync(
            task => task.HouseholdID == recommendation.HouseholdID && task.Status != "Done");

        if (existingTask != null)
        {
            return existingTask;
        }

        var household = await _context.Households.FindAsync(recommendation.HouseholdID);
        if (household == null)
        {
            return null;
        }

        var task = new TaskAssignment
        {
            BHWID = recommendation.RecommendedBHWID,
            HouseholdID = household.HouseholdID,
            TaskDate = DateTime.Today.AddHours(9),
            Priority = household.RiskScore >= 80 ? "High" : "Medium",
            Status = "Pending",
            Title = $"AI follow-up for {household.HouseholdMember}",
            Description = recommendation.Reason
        };

        await _context.TaskAssignments.AddAsync(task);
        await _context.SaveChangesAsync();
        return task;
    }

    private static int BuildWorkerScore(User worker, Household household, int openTasks)
    {
        var availabilityScore = worker.IsAvailable ? 40 : 5;
        var workloadScore = Math.Max(5, 30 - (openTasks * 5));
        var proximityScore = 10;
        if (!string.IsNullOrWhiteSpace(worker.AssignedArea) &&
            household.Address.Contains(worker.AssignedArea, StringComparison.OrdinalIgnoreCase))
        {
            proximityScore = 25;
        }

        var riskBonus = Convert.ToInt32((household.RiskScore ?? 0) / 10);
        return availabilityScore + workloadScore + proximityScore + riskBonus;
    }

    private static string BuildRecommendationReason(User? worker, Household household, IReadOnlyDictionary<int, int> taskLoad)
    {
        if (worker == null)
        {
            return "No available BHW could be recommended for this household.";
        }

        var openTasks = taskLoad.TryGetValue(worker.UserID, out var value) ? value : 0;
        var proximity = !string.IsNullOrWhiteSpace(worker.AssignedArea) &&
                        household.Address.Contains(worker.AssignedArea, StringComparison.OrdinalIgnoreCase)
            ? $"assigned to {worker.AssignedArea}"
            : "best overall available match";

        return $"{worker.FullName} was selected because the worker is {proximity}, marked {(worker.IsAvailable ? "available" : "unavailable")}, and currently has {openTasks} active tasks.";
    }

    private async Task<AssistantInsightViewModel> BuildHealthTrendInsightAsync()
    {
        await _predictiveAnalyticsService.RecalculateHouseholdRisksAsync();
        var highRiskCount = await _context.Households.CountAsync(household => (household.RiskScore ?? 0) >= 70);

        return new AssistantInsightViewModel
        {
            Type = "Trend",
            Title = "Health trend summary",
            Description = $"{highRiskCount} households are currently tagged as high risk based on recent records and household size.",
            Severity = highRiskCount >= 3 ? "High" : "Medium"
        };
    }
}
