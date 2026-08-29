using System.ComponentModel.DataAnnotations;

namespace SaraAgro.Api.DTOs.Client;

public class CreateClientRequest
{
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [EmailAddress]
    [StringLength(150)]
    public string? Email { get; set; }
}