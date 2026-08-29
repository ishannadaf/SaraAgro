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
        _authenticationService = authenticationService;
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
}