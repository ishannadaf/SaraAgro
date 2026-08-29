using System.ComponentModel.DataAnnotations;

namespace SaraAgro.Api.DTOs.Otp;

public class SendOtpRequest
{
    [Required]
    [StringLength(20)]
    public string MobileNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Purpose { get; set; } = string.Empty;
}