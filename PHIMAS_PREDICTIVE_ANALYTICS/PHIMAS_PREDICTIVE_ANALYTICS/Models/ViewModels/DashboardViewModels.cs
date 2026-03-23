using System.ComponentModel.DataAnnotations;

namespace PHIMAS_PREDICTIVE_ANALYTICS.Models.ViewModels;

public class DashboardCardViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public int Value { get; set; }
    public string AccentClass { get; set; } = "accent-green";
}

public class ChartPointViewModel
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class DiseaseForecastViewModel
{
    public string Disease { get; set; } = string.Empty;
    public int CurrentCases { get; set; }
    public int PredictedCases { get; set; }
    public float ConfidenceScore { get; set; }
    public string HighRiskBarangay { get; set; } = string.Empty;
}

public class AssistantInsightViewModel
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "Info";
}

public class TaskListItemViewModel
{
    public int TaskID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime TaskDate { get; set; }
    public int? BHWID { get; set; }
    public string BHWName { get; set; } = string.Empty;
    public int? HouseholdID { get; set; }
    public string HouseholdName { get; set; } = string.Empty;
    public string HouseholdAddress { get; set; } = string.Empty;
}

public class AssignmentRecommendationViewModel
{
    public int HouseholdID { get; set; }
    public string HouseholdName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public float RiskScore { get; set; }
    public int? RecommendedBHWID { get; set; }
    public string RecommendedBHWName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class AdminDashboardViewModel
{
    public List<DashboardCardViewModel> Cards { get; set; } = [];
    public List<TaskListItemViewModel> RecentTasks { get; set; } = [];
    public List<Report> RecentReports { get; set; } = [];
    public List<Inventory> LowStockItems { get; set; } = [];
    public List<AssistantInsightViewModel> ActivitySummary { get; set; } = [];
}

public class ChoDashboardViewModel
{
    public List<DashboardCardViewModel> Cards { get; set; } = [];
    public List<ChartPointViewModel> DiseaseTrend { get; set; } = [];
    public List<ChartPointViewModel> HouseholdRiskTrend { get; set; } = [];
    public List<DiseaseForecastViewModel> Forecasts { get; set; } = [];
    public List<Household> HighRiskHouseholds { get; set; } = [];
    public List<AssistantInsightViewModel> Insights { get; set; } = [];
}

public class BhwDashboardViewModel
{
    public User Worker { get; set; } = new();
    public int AssignedTasks { get; set; }
    public int HighPriorityTasks { get; set; }
    public int DueToday { get; set; }
    public int CompletedTasks { get; set; }
    public int ProgressPercent { get; set; }
    public List<TaskListItemViewModel> TodayTasks { get; set; } = [];
    public List<Report> RecentConsultations { get; set; } = [];
    public List<AssistantInsightViewModel> Insights { get; set; } = [];
}

public class UsersPageViewModel
{
    public User Form { get; set; } = new();
    public List<User> Users { get; set; } = [];
    public string? Search { get; set; }
}

public class HouseholdsPageViewModel
{
    public Household Form { get; set; } = new();
    public List<Household> Households { get; set; } = [];
    public string? Search { get; set; }
}

public class HealthRecordsPageViewModel
{
    public CreateHealthRecordViewModel Form { get; set; } = new() { DateRecorded = DateTime.Today };
    public List<HealthRecord> HealthRecords { get; set; } = [];
    public List<PatientLookupViewModel> Patients { get; set; } = [];
    public List<User> BHWs { get; set; } = [];
    public string AssignedArea { get; set; } = string.Empty;
    public string? Search { get; set; }
}

public class InventoryPageViewModel
{
    public Inventory Form { get; set; } = new();
    public List<Inventory> Items { get; set; } = [];
    public string? Search { get; set; }
}

public class ReportsPageViewModel
{
    public CreateReportViewModel Form { get; set; } = new() { DateGenerated = DateTime.Today };
    public List<Report> Reports { get; set; } = [];
    public List<User> Users { get; set; } = [];
    public List<PatientLookupViewModel> Patients { get; set; } = [];
    public List<AssistantInsightViewModel> Insights { get; set; } = [];
    public string AssignedArea { get; set; } = string.Empty;
    public string? Search { get; set; }
    public int TotalScope { get; set; }
    public int DataPoints { get; set; }
    public string RiskIndex { get; set; } = string.Empty;
}

public class PatientLookupViewModel
{
    public int PatientID { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int HouseholdID { get; set; }
    public string HouseholdName { get; set; } = string.Empty;
    public string HouseholdAddress { get; set; } = string.Empty;
    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactNumber { get; set; } = string.Empty;
    public string SearchText { get; set; } = string.Empty;
}

public class PatientLookupFieldViewModel
{
    public List<PatientLookupViewModel> Patients { get; set; } = [];
    public string PatientIdFieldName { get; set; } = "Form.PatientID";
    public string HouseholdIdFieldName { get; set; } = "Form.HouseholdID";
    public int? SelectedPatientId { get; set; }
    public int? SelectedHouseholdId { get; set; }
    public bool AllowEmpty { get; set; }
    public string EmptyLabel { get; set; } = "Select Patient";
    public string LookupLabel { get; set; } = "Patient Name";
    public string Placeholder { get; set; } = "Type a patient name";
    public string HelperText { get; set; } = "Select a patient to auto-fill the household and emergency contact.";
    public bool EnableVisitAddressEditor { get; set; }
    public string VisitAddressFieldName { get; set; } = "Form.Address";
    public string VisitAddressLabel { get; set; } = "Household Address";
    public string VisitAddressValue { get; set; } = string.Empty;
    public string AssignedArea { get; set; } = string.Empty;
    public string VisitAddressHelperText { get; set; } = "Address starts from the registered household or your assigned area. Add only real household location details.";
    public bool EnableEmergencyContactEditor { get; set; }
    public string EmergencyContactNameFieldName { get; set; } = "Form.EmergencyContactName";
    public string EmergencyContactNumberFieldName { get; set; } = "Form.EmergencyContactNumber";
    public string EmergencyContactNameValue { get; set; } = string.Empty;
    public string EmergencyContactNumberValue { get; set; } = string.Empty;
    public string EmergencyContactNameLabel { get; set; } = "Emergency Contact Name";
    public string EmergencyContactNumberLabel { get; set; } = "Contact Number";
}

public class TaskMonitoringPageViewModel
{
    public TaskAssignment Form { get; set; } = new() { TaskDate = DateTime.Today };
    public List<TaskListItemViewModel> Tasks { get; set; } = [];
    public List<User> BHWs { get; set; } = [];
    public List<Household> Households { get; set; } = [];
    public List<AssignmentRecommendationViewModel> Recommendations { get; set; } = [];
}

public class PredictiveAnalyticsPageViewModel
{
    public PredictiveAnalysis? LatestAnalysis { get; set; }
    public List<DiseaseForecastViewModel> Forecasts { get; set; } = [];
    public List<ChartPointViewModel> DiseaseTrend { get; set; } = [];
    public List<AssistantInsightViewModel> Insights { get; set; } = [];
}

public abstract class InlineHouseholdIntakeViewModel : IValidatableObject
{
    [Required]
    [StringLength(100)]
    public string PatientName { get; set; } = string.Empty;

    [Required]
    [StringLength(15)]
    [Phone]
    public string ContactNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Address { get; set; } = string.Empty;

    [StringLength(100)]
    public string? EmergencyContactName { get; set; }

    [StringLength(15)]
    [Phone]
    public string? EmergencyContactNumber { get; set; }

    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(EmergencyContactName) && !string.IsNullOrWhiteSpace(EmergencyContactNumber))
        {
            yield return new ValidationResult(
                "Emergency contact name is required when a contact number is provided.",
                [nameof(EmergencyContactName), nameof(EmergencyContactNumber)]);
        }
    }
}

public class CreateHealthRecordViewModel : InlineHouseholdIntakeViewModel
{
    [Required]
    public DateTime DateRecorded { get; set; } = DateTime.Today;

    [Required]
    [StringLength(100)]
    public string Disease { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Symptoms { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Submitted";
}

public class CreateReportViewModel : InlineHouseholdIntakeViewModel
{
    [Required]
    public DateTime DateGenerated { get; set; } = DateTime.Today;

    [Required]
    [StringLength(50)]
    public string ReportType { get; set; } = "Consultation Log";

    [Required]
    [StringLength(4000)]
    public string Content { get; set; } = string.Empty;
}
