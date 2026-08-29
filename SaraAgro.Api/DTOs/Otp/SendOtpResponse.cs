namespace SaraAgro.Api.DTOs.Otp;

public class SendOtpResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
}