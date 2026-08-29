namespace SaraAgro.Api.Models;

public class MilkDistribution
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public int CustomerId { get; set; }

    public int RateMasterId { get; set; }

    public DateTime DistributionDate { get; set; }

    // M = Morning, E = Evening
    public string Session { get; set; } = string.Empty;

    // Cow / Buffalo
    public string MilkType { get; set; } = string.Empty;

    // Quantity in litres
    public decimal Quantity { get; set; }

    // Rate applicable at the time of distribution
    public decimal Rate { get; set; }

    // Quantity × Rate
    public decimal Amount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Customer? Customer { get; set; }

    public RateMaster? RateMaster { get; set; }

    public Client? Client { get; set; }
}