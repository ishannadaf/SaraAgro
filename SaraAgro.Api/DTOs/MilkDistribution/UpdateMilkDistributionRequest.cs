using System.ComponentModel.DataAnnotations;

namespace SaraAgro.Api.DTOs.MilkDistribution;

public class UpdateMilkDistributionRequest
{
    [Range(
        0.01,
        9999.99,
        ErrorMessage = "Quantity must be greater than 0 and have maximum 2 decimal places.")]
    public decimal Quantity { get; set; }
}