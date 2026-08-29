using System.ComponentModel.DataAnnotations;

namespace SaraAgro.Api.DTOs.RateGroup;

public class CreateRateGroupRequest
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;
}