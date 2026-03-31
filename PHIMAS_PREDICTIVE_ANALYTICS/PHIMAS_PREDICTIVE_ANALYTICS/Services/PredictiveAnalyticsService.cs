using Microsoft.EntityFrameworkCore;
using PHIMAS_PREDICTIVE_ANALYTICS.Data;
using PHIMAS_PREDICTIVE_ANALYTICS.Models;
using PHIMAS_PREDICTIVE_ANALYTICS.Models.ViewModels;

namespace PHIMAS_PREDICTIVE_ANALYTICS.Services;

public class PredictiveAnalyticsService
{
    private readonly AppDbContext _context;

    public PredictiveAnalyticsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task RecalculateHouseholdRisksAsync()
    {
        var households = await _context.Households.ToListAsync();
        if (households.Count == 0)
        {
            return;
        }

        var since = DateTime.UtcNow.AddDays(-45);
        var memberCounts = await _context.HouseholdMembers
            .AsNoTracking()
            .GroupBy(member => member.HouseholdID)
            .Select(group => new
            {
                HouseholdID = group.Key,
                Count = group.Count()
            })
            .ToDictionaryAsync(item => item.HouseholdID, item => item.Count);

        var recentRecordStats = await _context.HealthRecords
            .AsNoTracking()
            .Where(record => record.DateRecorded >= since && record.PatientID != null)
            .Join(
                _context.HouseholdMembers.AsNoTracking(),
                record => record.PatientID!.Value,
                patient => patient.MemberID,
                (record, patient) => new
                {
                    patient.HouseholdID,
                    record.Status,
                    record.Disease
                })
            .GroupBy(item => item.HouseholdID)
            .Select(group => new
            {
                HouseholdID = group.Key,
                ActiveCount = group.Sum(item => item.Status != "Done" ? 1 : 0),
                WeightedDiseaseRisk = group.Sum(item =>
                    EF.Functions.Like(item.Disease, "%dengue%") ? 25 :
                    EF.Functions.Like(item.Disease, "%lept%") ? 20 :
                    EF.Functions.Like(item.Disease, "%influenza%") ? 12 :
                    8)
            })
            .ToDictionaryAsync(item => item.HouseholdID);

        foreach (var household in households)
        {
            var sizeFactor = (memberCounts.TryGetValue(household.HouseholdID, out var memberCount) ? memberCount : 0) * 2;
            if (!recentRecordStats.TryGetValue(household.HouseholdID, out var stats))
            {
                household.RiskScore = Math.Min(100, sizeFactor);
                continue;
            }

            household.RiskScore = Math.Min(100, (stats.ActiveCount * 12) + stats.WeightedDiseaseRisk + sizeFactor);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<ChartPointViewModel>> GetDiseaseTrendAsync(int days = 6)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-(days - 1));
        var records = await _context.HealthRecords
            .Where(record => record.DateRecorded >= startDate)
            .ToListAsync();

        return Enumerable.Range(0, days)
            .Select(index =>
            {
                var date = startDate.AddDays(index);
                return new ChartPointViewModel
                {
                    Label = date.ToString("MMM dd"),
                    Value = records.Count(record => record.DateRecorded.Date == date)
                };
            })
            .ToList();
    }

    public async Task<List<ChartPointViewModel>> GetRiskProfileAsync()
    {
        await RecalculateHouseholdRisksAsync();
        var households = await _context.Households
            .Include(household => household.Members)
            .OrderByDescending(household => household.RiskScore)
            .Take(6)
            .ToListAsync();

        return households
            .Select(household => new ChartPointViewModel
            {
                Label = household.HouseholdMember,
                Value = Convert.ToInt32(household.RiskScore ?? 0)
            })
            .ToList();
    }

    public async Task<List<DiseaseForecastViewModel>> GetForecastsAsync(int limit = 5)
    {
        var storedForecasts = await GetStoredForecastsAsync(limit);
        return storedForecasts.Count > 0
            ? storedForecasts
            : await GenerateFallbackForecastsAsync(limit);
    }

    public async Task<PredictiveAnalysis?> GetLatestStoredAnalysisAsync()
    {
        var analyses = await _context.PredictiveAnalysis
            .AsNoTracking()
            .OrderByDescending(item => item.DateGenerated)
            .ThenByDescending(item => item.ConfidenceScore)
            .ToListAsync();

        return analyses
            .Where(item => !string.IsNullOrWhiteSpace(item.Disease))
            .GroupBy(item => item.Disease.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(SelectWinningAnalysis)
            .OrderByDescending(item => item.DateGenerated.Date)
            .ThenByDescending(item => item.PredictedCases ?? 0)
            .ThenByDescending(item => NormalizeConfidence(item.ConfidenceScore))
            .ThenByDescending(item => item.DateGenerated)
            .FirstOrDefault();
    }

    public async Task<List<DiseaseForecastViewModel>> GenerateFallbackForecastsAsync(int limit = 5)
    {
        var since = DateTime.UtcNow.AddDays(-30);
        var recentRecords = await _context.HealthRecords
            .Where(record => record.DateRecorded >= since)
            .Include(record => record.Patient)
            .ThenInclude(patient => patient!.Household)
            .ToListAsync();

        var grouped = recentRecords
            .GroupBy(record => record.Disease)
            .OrderByDescending(group => group.Count())
            .Take(limit)
            .ToList();

        var forecasts = new List<DiseaseForecastViewModel>();
        foreach (var group in grouped)
        {
            var currentCases = group.Count();
            var casesLastWeek = group.Count(record => record.DateRecorded >= DateTime.UtcNow.AddDays(-7));
            var casesPreviousWeek = group.Count(record => record.DateRecorded >= DateTime.UtcNow.AddDays(-14) && record.DateRecorded < DateTime.UtcNow.AddDays(-7));
            var delta = casesLastWeek - casesPreviousWeek;
            var predicted = Math.Max(0, currentCases + delta);
            var highRiskBarangay = group
                .Select(record => ExtractArea(record.Household?.Address))
                .GroupBy(area => area)
                .OrderByDescending(areaGroup => areaGroup.Count())
                .Select(areaGroup => areaGroup.Key)
                .FirstOrDefault() ?? "Unspecified";

            forecasts.Add(new DiseaseForecastViewModel
            {
                Disease = group.Key,
                CurrentCases = currentCases,
                PredictedCases = predicted,
                ConfidenceScore = Math.Clamp(0.55f + (currentCases / 40f), 0.55f, 0.95f),
                HighRiskBarangay = highRiskBarangay
            });
        }

        return forecasts;
    }

    public async Task<PredictiveAnalyticsPageViewModel> BuildPredictivePageAsync()
    {
        return new PredictiveAnalyticsPageViewModel
        {
            LatestAnalysis = await GetLatestStoredAnalysisAsync(),
            Forecasts = await GetForecastsAsync(),
            DiseaseTrend = await GetDiseaseTrendAsync()
        };
    }

    private async Task<List<DiseaseForecastViewModel>> GetStoredForecastsAsync(int limit)
    {
        var analyses = await _context.PredictiveAnalysis
            .AsNoTracking()
            .OrderByDescending(item => item.DateGenerated)
            .ThenByDescending(item => item.ConfidenceScore)
            .ToListAsync();

        var latestAnalyses = analyses
            .Where(item => !string.IsNullOrWhiteSpace(item.Disease))
            .GroupBy(item => item.Disease.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(SelectWinningAnalysis)
            .OrderByDescending(item => item.DateGenerated.Date)
            .ThenByDescending(item => item.PredictedCases ?? 0)
            .ThenByDescending(item => NormalizeConfidence(item.ConfidenceScore))
            .ThenByDescending(item => item.DateGenerated)
            .Take(limit)
            .ToList();

        if (latestAnalyses.Count == 0)
        {
            return [];
        }

        var since = DateTime.UtcNow.AddDays(-30);
        var currentCaseCounts = await _context.HealthRecords
            .Where(record => record.DateRecorded >= since)
            .GroupBy(record => record.Disease)
            .Select(group => new
            {
                Disease = group.Key,
                Count = group.Count()
            })
            .ToListAsync();

        var currentCaseLookup = currentCaseCounts
            .Where(item => !string.IsNullOrWhiteSpace(item.Disease))
            .GroupBy(item => item.Disease.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Sum(item => item.Count), StringComparer.OrdinalIgnoreCase);

        return latestAnalyses
            .Select(analysis => new DiseaseForecastViewModel
            {
                Disease = analysis.Disease,
                CurrentCases = currentCaseLookup.TryGetValue(analysis.Disease, out var currentCases) ? currentCases : 0,
                PredictedCases = analysis.PredictedCases ?? 0,
                ConfidenceScore = NormalizeConfidence(analysis.ConfidenceScore),
                HighRiskBarangay = analysis.HighRiskBarangay
            })
            .ToList();
    }

    private static PredictiveAnalysis SelectWinningAnalysis(IGrouping<string, PredictiveAnalysis> group)
    {
        var latestDate = group.Max(item => item.DateGenerated.Date);

        return group
            .Where(item => item.DateGenerated.Date == latestDate)
            .OrderByDescending(item => item.PredictedCases ?? 0)
            .ThenByDescending(item => NormalizeConfidence(item.ConfidenceScore))
            .ThenByDescending(item => item.DateGenerated)
            .First();
    }

    private static string ExtractArea(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return "Unspecified";
        }

        return address.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? address;
    }

    private static float NormalizeConfidence(float score)
    {
        return score > 1f
            ? Math.Clamp(score / 100f, 0f, 1f)
            : Math.Clamp(score, 0f, 1f);
    }
}
