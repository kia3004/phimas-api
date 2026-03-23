using Microsoft.EntityFrameworkCore;
using PHIMAS_PREDICTIVE_ANALYTICS.Data;
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
        var households = await _context.Households
            .Include(household => household.Members)
            .ToListAsync();
        var recentRecords = await _context.HealthRecords
            .Where(record => record.DateRecorded >= DateTime.UtcNow.AddDays(-45))
            .Include(record => record.Patient)
            .ToListAsync();

        foreach (var household in households)
        {
            var householdRecords = recentRecords
                .Where(record => record.Patient != null && record.Patient.HouseholdID == household.HouseholdID)
                .ToList();
            var activeCount = householdRecords.Count(record => !string.Equals(record.Status, "Done", StringComparison.OrdinalIgnoreCase));
            var weightedDiseaseRisk = householdRecords.Sum(record => GetDiseaseWeight(record.Disease));
            var sizeFactor = household.NumberOfMembers * 2;
            household.RiskScore = Math.Min(100, activeCount * 12 + weightedDiseaseRisk + sizeFactor);
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

    public async Task<List<DiseaseForecastViewModel>> GenerateForecastsAsync()
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
            .Take(5)
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

        await PersistForecastsAsync(forecasts);
        return forecasts;
    }

    public async Task<PredictiveAnalyticsPageViewModel> BuildPredictivePageAsync()
    {
        var forecasts = await GenerateForecastsAsync();
        return new PredictiveAnalyticsPageViewModel
        {
            LatestAnalysis = await _context.PredictiveAnalysis.OrderByDescending(item => item.DateGenerated).FirstOrDefaultAsync(),
            Forecasts = forecasts,
            DiseaseTrend = await GetDiseaseTrendAsync()
        };
    }

    private async Task PersistForecastsAsync(IEnumerable<DiseaseForecastViewModel> forecasts)
    {
        foreach (var forecast in forecasts)
        {
            var existing = await _context.PredictiveAnalysis.FirstOrDefaultAsync(
                analysis => analysis.Disease == forecast.Disease && analysis.DateGenerated.Date == DateTime.UtcNow.Date);

            if (existing == null)
            {
                await _context.PredictiveAnalysis.AddAsync(new()
                {
                    DateGenerated = DateTime.UtcNow,
                    Disease = forecast.Disease,
                    PredictedCases = forecast.PredictedCases,
                    HighRiskBarangay = forecast.HighRiskBarangay,
                    ConfidenceScore = forecast.ConfidenceScore
                });
            }
            else
            {
                existing.PredictedCases = forecast.PredictedCases;
                existing.HighRiskBarangay = forecast.HighRiskBarangay;
                existing.ConfidenceScore = forecast.ConfidenceScore;
            }
        }

        await _context.SaveChangesAsync();
    }

    private static int GetDiseaseWeight(string disease)
    {
        if (disease.Contains("dengue", StringComparison.OrdinalIgnoreCase))
        {
            return 25;
        }

        if (disease.Contains("lept", StringComparison.OrdinalIgnoreCase))
        {
            return 20;
        }

        if (disease.Contains("influenza", StringComparison.OrdinalIgnoreCase))
        {
            return 12;
        }

        return 8;
    }

    private static string ExtractArea(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return "Unspecified";
        }

        return address.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? address;
    }
}
