using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PHIMAS_PREDICTIVE_ANALYTICS.Models;

public class TaskAssignment
{
    [Key]
    [Column("TaskAssignmentID")]
    public int TaskID { get; set; }

    public int? BHWID { get; set; }

    public int? HouseholdID { get; set; }

    public DateTime TaskDate { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Pending";

    [Required]
    [StringLength(50)]
    public string Priority { get; set; } = "Medium";

    [StringLength(150)]
    public string? Title { get; set; }

    [Required]
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;

    [ForeignKey(nameof(BHWID))]
    public User? BHW { get; set; }

    [ForeignKey(nameof(HouseholdID))]
    public Household? Household { get; set; }
}
