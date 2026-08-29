namespace SaraAgro.Api.Models;

public class User
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string MobileNumber { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = "User";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Client? Client { get; set; }
}