using System.ComponentModel.DataAnnotations;

namespace SaraAgro.Api.DTOs.Customer;

public class CreateCustomerRequest
{
    [Required]
    public int RateGroupId { get; set; }

    [Required]
    [MaxLength(50)]
    public string CustomerCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string MobileNumber { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;
}