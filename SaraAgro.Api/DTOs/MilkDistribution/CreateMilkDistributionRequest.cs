using System.ComponentModel.DataAnnotations;

namespace SaraAgro.Api.DTOs.MilkDistribution;

public class CreateMilkDistributionRequest
{
    [Required]
    public int CustomerId { get; set; }

    [Required]
    public DateTime DistributionDate { get; set; }

    // M = Morning, E = Evening
    [Required]
    [RegularExpression(
        "^(M|E)$",
        ErrorMessage = "Session must be M or E.")]
    public string Session { get; set; } = string.Empty;

    // Cow / Buffalo
    [Required]
    [RegularExpression(
        "^(Cow|Buffalo)$",
        ErrorMessage = "Milk type must be Cow or Buffalo.")]
    public string MilkType { get; set; } = string.Empty;

    // Litres - maximum 2 decimal places
    [Range(
        0.01,
        9999.99,
        ErrorMessage = "Quantity must be greater than 0 and have maximum 2 decimal places.")]
    public decimal Quantity { get; set; }
}