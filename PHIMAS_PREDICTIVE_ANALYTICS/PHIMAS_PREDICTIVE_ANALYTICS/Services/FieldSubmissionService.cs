using Microsoft.EntityFrameworkCore;
using PHIMAS_PREDICTIVE_ANALYTICS.Data;
using PHIMAS_PREDICTIVE_ANALYTICS.Helpers;
using PHIMAS_PREDICTIVE_ANALYTICS.Models;
using PHIMAS_PREDICTIVE_ANALYTICS.Models.ViewModels;

namespace PHIMAS_PREDICTIVE_ANALYTICS.Services;

public class FieldSubmissionService
{
    private readonly AppDbContext _context;
    private readonly PredictiveAnalyticsService _predictiveAnalyticsService;

    public FieldSubmissionService(
        AppDbContext context,
        PredictiveAnalyticsService predictiveAnalyticsService)
    {
        _context = context;
        _predictiveAnalyticsService = predictiveAnalyticsService;
    }

    public async Task<HealthRecord> CreateHealthRecordAsync(
        int bhwId,
        CreateHealthRecordViewModel form,
        CancellationToken cancellationToken = default)
    {
        return await UpsertHealthRecordAsync(
            form.RecordID,
            bhwId,
            form,
            cancellationToken);
    }

    public async Task<HealthRecord> UpsertHealthRecordAsync(
        int? recordId,
        int bhwId,
        CreateHealthRecordViewModel form,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var intake = await HouseholdIntakeHelper.ResolveOrCreateAsync(
            _context,
            new HouseholdIntakeRequest(
                form.PatientName,
                form.ContactNumber,
                form.Address,
                form.EmergencyContactName,
                form.EmergencyContactNumber),
            cancellationToken);

        HealthRecord record;
        if (recordId is int resolvedRecordId && resolvedRecordId > 0)
        {
            record = await _context.HealthRecords.FirstOrDefaultAsync(
                         item => item.RecordID == resolvedRecordId,
                         cancellationToken)
                     ?? throw new ArgumentException("Health record not found.", nameof(form.RecordID));

            record.BHWID = bhwId;
            record.Patient = intake.Patient;
            record.DateRecorded = NormalizeDate(form.DateRecorded, DateTime.UtcNow);
            record.Disease = HouseholdIntakeHelper.NormalizeRequired(form.Disease, nameof(form.Disease));
            record.Symptoms = HouseholdIntakeHelper.NormalizeRequired(form.Symptoms, nameof(form.Symptoms));
            record.Status = HouseholdIntakeHelper.NormalizeRequired(form.Status, nameof(form.Status));
        }
        else
        {
            record = new HealthRecord
            {
                BHWID = bhwId,
                Patient = intake.Patient,
                DateRecorded = NormalizeDate(form.DateRecorded, DateTime.UtcNow),
                Disease = HouseholdIntakeHelper.NormalizeRequired(form.Disease, nameof(form.Disease)),
                Symptoms = HouseholdIntakeHelper.NormalizeRequired(form.Symptoms, nameof(form.Symptoms)),
                Status = HouseholdIntakeHelper.NormalizeRequired(form.Status, nameof(form.Status))
            };

            _context.HealthRecords.Add(record);
        }

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await LoadHealthRecordGraphAsync(record.RecordID, cancellationToken);
        await _predictiveAnalyticsService.RecalculateHouseholdRisksAsync();
        return record;
    }

    public async Task<Report> CreateReportAsync(
        int generatedBy,
        CreateReportViewModel form,
        CancellationToken cancellationToken = default)
    {
        return await UpsertReportAsync(
            form.ReportID,
            generatedBy,
            form,
            cancellationToken);
    }

    public async Task<Report> UpsertReportAsync(
        int? reportId,
        int generatedBy,
        CreateReportViewModel form,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var intake = await HouseholdIntakeHelper.ResolveOrCreateAsync(
            _context,
            new HouseholdIntakeRequest(
                form.PatientName,
                form.ContactNumber,
                form.Address,
                form.EmergencyContactName,
                form.EmergencyContactNumber),
            cancellationToken);

        Report report;
        if (reportId is int resolvedReportId && resolvedReportId > 0)
        {
            report = await _context.Reports.FirstOrDefaultAsync(
                         item => item.ReportID == resolvedReportId,
                         cancellationToken)
                     ?? throw new ArgumentException("Report not found.", nameof(form.ReportID));

            report.GeneratedBy = generatedBy;
            report.Patient = intake.Patient;
            report.DateGenerated = NormalizeDate(form.DateGenerated, DateTime.UtcNow);
            report.ReportType = HouseholdIntakeHelper.NormalizeRequired(form.ReportType, nameof(form.ReportType));
            report.Content = HouseholdIntakeHelper.NormalizeRequired(form.Content, nameof(form.Content));
        }
        else
        {
            report = new Report
            {
                GeneratedBy = generatedBy,
                Patient = intake.Patient,
                DateGenerated = NormalizeDate(form.DateGenerated, DateTime.UtcNow),
                ReportType = HouseholdIntakeHelper.NormalizeRequired(form.ReportType, nameof(form.ReportType)),
                Content = HouseholdIntakeHelper.NormalizeRequired(form.Content, nameof(form.Content))
            };

            _context.Reports.Add(report);
        }

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await LoadReportGraphAsync(report.ReportID, cancellationToken);
        return report;
    }

    private async Task LoadHealthRecordGraphAsync(int recordId, CancellationToken cancellationToken)
    {
        var hydratedRecord = await _context.HealthRecords
            .Include(record => record.BHW)
            .Include(record => record.Patient)
            .ThenInclude(patient => patient!.Household)
            .ThenInclude(household => household!.Members)
            .FirstOrDefaultAsync(record => record.RecordID == recordId, cancellationToken);

        _ = hydratedRecord;
    }

    private async Task LoadReportGraphAsync(int reportId, CancellationToken cancellationToken)
    {
        var hydratedReport = await _context.Reports
            .Include(report => report.GeneratedByUser)
            .Include(report => report.Patient)
            .ThenInclude(patient => patient!.Household)
            .ThenInclude(household => household!.Members)
            .FirstOrDefaultAsync(report => report.ReportID == reportId, cancellationToken);

        _ = hydratedReport;
    }

    private static DateTime NormalizeDate(DateTime value, DateTime fallback)
    {
        return value == default ? fallback : value;
    }
}
