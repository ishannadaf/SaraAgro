namespace SaraAgro.Api.DTOs.MilkDistribution;

public class MilkDistributionResponse
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string CustomerCode { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public DateTime DistributionDate { get; set; }

    // M = Morning, E = Evening
    public string Session { get; set; } = string.Empty;

    // Cow / Buffalo
    public string MilkType { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public decimal Rate { get; set; }

    public decimal Amount { get; set; }

    // The exact Rate Master used for this distribution.
    public int RateMasterId { get; set; }

    // Customer's Route / Rate Group.
    public int RateGroupId { get; set; }

    public string RateGroupName { get; set; } = string.Empty;
}