namespace SaraAgro.Api.Models;

public class RateGroup
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    // Example: Kothrud, Sangli, Baner
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Client? Client { get; set; }

    public ICollection<RateMaster> RateMasters { get; set; }
        = new List<RateMaster>();
}