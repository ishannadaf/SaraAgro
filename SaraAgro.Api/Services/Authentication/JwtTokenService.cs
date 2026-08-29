using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SaraAgro.Api.Authentication;
using SaraAgro.Api.Interfaces.Authentication;
using SaraAgro.Api.Models;

namespace SaraAgro.Api.Services.Authentication;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(IOptions<JwtSettings> options)
    {
        _settings = options.Value;

        if (string.IsNullOrWhiteSpace(_settings.SecretKey))
        {
            throw new InvalidOperationException(
                "JWT SecretKey is not configured.");
        }

        if (Encoding.UTF8.GetByteCount(_settings.SecretKey) < 32)
        {
            throw new InvalidOperationException(
                "JWT SecretKey must be at least 32 bytes long.");
        }
    }

    public string GenerateAccessToken(User user, string clientName)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_settings.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.MobilePhone, user.MobileNumber),
            new(ClaimTypes.Role, user.Role),
            new("client_id", user.ClientId.ToString()),
            new("client_name", clientName)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_settings.SecretKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public DateTime GetAccessTokenExpiration()
    {
        return DateTime.UtcNow.AddMinutes(
            _settings.AccessTokenMinutes);
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(64));
    }

    public string HashRefreshToken(string refreshToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        var hash = SHA256.HashData(
            Encoding.UTF8.GetBytes(refreshToken));

        return Convert.ToHexString(hash);
    }
}