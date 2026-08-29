using System.ComponentModel.DataAnnotations;

namespace SaraAgro.Api.DTOs.RateMaster;

public class CreateRateMasterRequest
{
    [Required]
    public int ClientId { get; set; }

    [Required]
    public int RateGroupId { get; set; }

    [Required]
    [MaxLength(20)]
    public string MilkType { get; set; } = string.Empty;

    [Range(0.01, 999999)]
    public decimal Rate { get; set; }

    [Required]
    public DateTime EffectiveDate { get; set; }
}