namespace SaraAgro.Api.Models;

public class OtpVerification
{
    public int Id { get; set; }

    public string MobileNumber { get; set; } = string.Empty;

    public string OtpHash { get; set; } = string.Empty;

    public string Purpose { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public int AttemptCount { get; set; }

    public bool IsVerified { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? VerifiedAt { get; set; }
}