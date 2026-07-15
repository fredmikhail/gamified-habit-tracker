using System.ComponentModel.DataAnnotations;

namespace HabitTracker.Api.DTOs;

public sealed class LoginRequest
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}
