using System.ComponentModel.DataAnnotations;

namespace SaraAgro.Api.DTOs.Customer;

public class UpdateCustomerStatusRequest
{
    [Required]
    public bool IsActive { get; set; }
}