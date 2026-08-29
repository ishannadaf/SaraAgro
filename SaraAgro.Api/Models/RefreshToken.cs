namespace SaraAgro.Api.Models;

public class RefreshToken
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? RevokedAt { get; set; }

    public User? User { get; set; }

    public bool IsActive =>
        RevokedAt == null && ExpiresAt > DateTime.UtcNow;
}