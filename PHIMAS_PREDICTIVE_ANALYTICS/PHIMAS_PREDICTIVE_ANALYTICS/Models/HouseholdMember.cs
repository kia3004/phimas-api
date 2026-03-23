using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PHIMAS_PREDICTIVE_ANALYTICS.Models;

public class HouseholdMember
{
    [Key]
    [Column("PatientID")]
    public int MemberID { get; set; }

    [NotMapped]
    public int PatientID
    {
        get => MemberID;
        set => MemberID = value;
    }

    [Required]
    public int HouseholdID { get; set; }

    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(15)]
    public string ContactNumber { get; set; } = string.Empty;

    public bool IsEmergencyContact { get; set; }

    [ForeignKey(nameof(HouseholdID))]
    public Household? Household { get; set; }
}
