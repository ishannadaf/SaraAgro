namespace SaraAgro.Api.DTOs.Authentication;

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public DateTime AccessTokenExpiresAt { get; set; }

    public int UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string MobileNumber { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public int ClientId { get; set; }

    public string ClientName { get; set; } = string.Empty;
}