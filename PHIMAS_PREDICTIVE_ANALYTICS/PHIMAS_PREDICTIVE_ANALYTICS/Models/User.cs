using System.ComponentModel.DataAnnotations;

namespace PHIMAS_PREDICTIVE_ANALYTICS.Models;

public class User
{
    [Key]
    public int UserID { get; set; }

    [Required]
    [StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string Role { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Password { get; set; } = string.Empty;

    [StringLength(50)]
    public string ContactNumber { get; set; } = string.Empty;

    public bool IsAvailable { get; set; } = true;

    [StringLength(255)]
    public string? ProfilePicture { get; set; }

    [StringLength(120)]
    public string? AssignedArea { get; set; }
}
