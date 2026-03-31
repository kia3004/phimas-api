using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PHIMAS_PREDICTIVE_ANALYTICS.Models;

public class Report
{
    [Key]
    public int ReportID { get; set; }

    public DateTime DateGenerated { get; set; } = DateTime.UtcNow;

    public int? GeneratedBy { get; set; }

    public int? PatientID { get; set; }

    [Required]
    [StringLength(50)]
    public string ReportType { get; set; } = string.Empty;

    [Required]
    [StringLength(4000)]
    public string Content { get; set; } = string.Empty;

    [NotMapped]
    public string DisplayContent => HealthRecord.NormalizeSymptomsForDisplay(Content, ReportType);

    [ForeignKey(nameof(GeneratedBy))]
    public User? GeneratedByUser { get; set; }

    [ForeignKey(nameof(PatientID))]
    public HouseholdMember? Patient { get; set; }

    [NotMapped]
    public Household? Household => Patient?.Household;

    [NotMapped]
    public HouseholdMember? Member
    {
        get => Patient;
        set => Patient = value;
    }

    [NotMapped]
    public string DisplayAddress => Household?.Address ?? string.Empty;

    [NotMapped]
    public string DisplayPatientName => Patient?.FullName ?? string.Empty;

    [NotMapped]
    public string DisplayHouseholdName => Household?.HouseholdMember ?? string.Empty;

    [NotMapped]
    public string DisplayEmergencyContactName => Household?.GetEmergencyContact(PatientID)?.FullName ?? string.Empty;

    [NotMapped]
    public string DisplayEmergencyContactNumber => Household?.GetEmergencyContact(PatientID)?.ContactNumber ?? string.Empty;
}
