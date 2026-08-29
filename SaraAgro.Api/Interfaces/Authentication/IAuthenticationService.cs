using SaraAgro.Api.DTOs.Authentication;

namespace SaraAgro.Api.Interfaces.Authentication;

public interface IAuthenticationService
{
    Task<RegisterResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);

    Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);
}