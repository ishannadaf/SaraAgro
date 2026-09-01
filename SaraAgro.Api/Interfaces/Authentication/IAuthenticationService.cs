using SaraAgro.Api.DTOs.Authentication;

namespace SaraAgro.Api.Interfaces.Authentication;

public interface IAuthenticationService
{
    // =========================================================
    // REGISTER
    // =========================================================

    Task<RegisterResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);


    // =========================================================
    // LOGIN
    // =========================================================

    Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);


    // =========================================================
    // REFRESH TOKEN
    // =========================================================

    Task<LoginResponse> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);


    // =========================================================
    // LOGOUT
    // =========================================================

    Task LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);
}