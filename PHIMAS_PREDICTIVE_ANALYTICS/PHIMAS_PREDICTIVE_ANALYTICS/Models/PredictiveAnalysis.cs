using System.ComponentModel.DataAnnotations;

namespace PHIMAS_PREDICTIVE_ANALYTICS.Models;

public class PredictiveAnalysis
{
    [Key]
    public int AnalyticsID { get; set; }

    public DateTime DateGenerated { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(150)]
    public string Disease { get; set; } = string.Empty;

    public int? PredictedCases { get; set; }

    [Required]
    [StringLength(150)]
    public string HighRiskBarangay { get; set; } = string.Empty;

    public float ConfidenceScore { get; set; }
}
