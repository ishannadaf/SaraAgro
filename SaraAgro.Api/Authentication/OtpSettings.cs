namespace SaraAgro.Api.Authentication;

public class OtpSettings
{
    public int ExpirationMinutes { get; set; } = 5;

    public int MaxAttempts { get; set; } = 5;

    public int ResendCooldownSeconds { get; set; } = 60;
}