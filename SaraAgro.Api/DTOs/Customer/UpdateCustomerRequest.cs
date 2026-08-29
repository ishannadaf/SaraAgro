using System.ComponentModel.DataAnnotations;

namespace SaraAgro.Api.DTOs.Customer;

public class UpdateCustomerRequest
{
    [Required]
    public int RateGroupId { get; set; }

    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string MobileNumber { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}