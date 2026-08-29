namespace SaraAgro.Api.Models;

public class RateMaster
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    // New relationship:
    // RateMaster belongs to a Route / Rate Group
    public int RateGroupId { get; set; }

    // Cow / Buffalo
    public string MilkType { get; set; } = string.Empty;

    // Rate per litre
    public decimal Rate { get; set; }

    public DateTime EffectiveDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }


    // =========================================================
    // NAVIGATION PROPERTIES
    // =========================================================

    public Client? Client { get; set; }

    public RateGroup? RateGroup { get; set; }
}