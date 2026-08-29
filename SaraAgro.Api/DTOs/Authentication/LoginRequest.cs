using System.ComponentModel.DataAnnotations;

namespace SaraAgro.Api.DTOs.Authentication;

public class LoginRequest
{
    [Required]
    [StringLength(20)]
    public string MobileNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}