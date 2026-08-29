using System.ComponentModel.DataAnnotations;

namespace SaraAgro.Api.DTOs.Authentication;

public class RegisterRequest
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string BusinessName { get; set; } = string.Empty;


    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string OwnerName { get; set; } = string.Empty;


    [Required]
    [StringLength(20)]
    public string MobileNumber { get; set; } = string.Empty;


    [EmailAddress]
    [StringLength(150)]
    public string? Email { get; set; }


    [StringLength(150)]
    public string? City { get; set; }


    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;


    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string Otp { get; set; } = string.Empty;
}