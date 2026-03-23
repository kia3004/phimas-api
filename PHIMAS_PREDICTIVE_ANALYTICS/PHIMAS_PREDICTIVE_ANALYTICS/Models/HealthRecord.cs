using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PHIMAS_PREDICTIVE_ANALYTICS.Models;

public class HealthRecord
{
    [Key]
    public int RecordID { get; set; }

    public int? BHWID { get; set; }

    public int? PatientID { get; set; }

    public DateTime DateRecorded { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(100)]
    public string Disease { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Symptoms { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Submitted";

    [ForeignKey(nameof(BHWID))]
    public User? BHW { get; set; }

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
    public string DisplayEmergencyContactName
    {
        get
        {
            return Household?.GetEmergencyContact(PatientID)?.FullName ?? string.Empty;
        }
    }

    [NotMapped]
    public string DisplayEmergencyContactNumber
    {
        get
        {
            return Household?.GetEmergencyContact(PatientID)?.ContactNumber ?? string.Empty;
        }
    }
}
