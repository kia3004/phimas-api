using System.ComponentModel.DataAnnotations;

namespace PHIMAS_PREDICTIVE_ANALYTICS.Models;

public class Inventory
{
    [Key]
    public int ItemID { get; set; }

    [Required]
    [StringLength(150)]
    public string ItemName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Unit { get; set; } = string.Empty;

    public int? MinimumThreshold { get; set; }

    public int? CurrentStock { get; set; }
}
