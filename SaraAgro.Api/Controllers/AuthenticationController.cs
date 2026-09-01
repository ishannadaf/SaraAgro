using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaraAgro.Api.DTOs.Authentication;
using SaraAgro.Api.Interfaces.Authentication;

namespace SaraAgro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;

    public AuthenticationController(
        IAuthenticationService authenticationService)
    {
        _authenticationService =
            authenticationService;
    }


    // =========================================================
    // LOGIN
    // =========================================================

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await _authenticationService.LoginAsync(
                request,
                cancellationToken);

        return Ok(response);
    }


    // =========================================================
    // REGISTER
    // =========================================================

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await _authenticationService.RegisterAsync(
                request,
                cancellationToken);

        return Ok(response);
    }


    // =========================================================
    // REFRESH TOKEN
    // =========================================================

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponse>> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await _authenticationService.RefreshAsync(
                request.RefreshToken,
                cancellationToken);

        return Ok(response);
    }


    // =========================================================
    // LOGOUT
    // =========================================================

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        await _authenticationService.LogoutAsync(
            request.RefreshToken,
            cancellationToken);

        return Ok(new
        {
            success = true,
            message = "Logged out successfully."
        });
    }
}


// =============================================================
// REFRESH / LOGOUT REQUEST
// =============================================================

public sealed class RefreshTokenRequest
{
    public string RefreshToken { get; set; } =
        string.Empty;
}