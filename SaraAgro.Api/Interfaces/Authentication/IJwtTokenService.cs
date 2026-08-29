using SaraAgro.Api.Models;

namespace SaraAgro.Api.Interfaces.Authentication;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user, string clientName);

    DateTime GetAccessTokenExpiration();

    string GenerateRefreshToken();

    string HashRefreshToken(string refreshToken);
}