namespace SaraAgro.Api.DTOs.RateGroup;

public class RateGroupResponse
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}