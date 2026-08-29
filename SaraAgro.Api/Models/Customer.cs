namespace SaraAgro.Api.Models;

public class Customer
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    // Customer belongs to a Route / Rate Group.
    // It is NOT tied to Cow or Buffalo.
    public int RateGroupId { get; set; }

    public string CustomerCode { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string MobileNumber { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }


    // =========================================================
    // NAVIGATION PROPERTIES
    // =========================================================

    public Client? Client { get; set; }

    public RateGroup? RateGroup { get; set; }
}