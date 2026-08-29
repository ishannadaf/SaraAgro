using System.Security.Claims;

namespace SaraAgro.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetClientId(
    this ClaimsPrincipal user)
    {
        var clientIdClaim =
            user.FindFirst("client_id")
            ?? user.FindFirst("clientId");

        if (clientIdClaim == null ||
            !int.TryParse(
                clientIdClaim.Value,
                out var clientId))
        {
            throw new UnauthorizedAccessException(
                "Client information is missing from the access token.");
        }

        return clientId;
    }

    public static int GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(
            ClaimTypes.NameIdentifier);

        if (userIdClaim == null ||
            !int.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException(
                "User information is missing from the access token.");
        }

        return userId;
    }
}