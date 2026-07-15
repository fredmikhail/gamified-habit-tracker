using System.ComponentModel.DataAnnotations;

namespace HabitTracker.Api.DTOs;

public sealed class RegisterRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(30, MinimumLength = 3)]
    [RegularExpression(@"^[A-Za-z0-9_]+$")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 15)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string TimeZone { get; set; } = string.Empty;
}
