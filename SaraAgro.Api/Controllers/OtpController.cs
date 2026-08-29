using Microsoft.AspNetCore.Mvc;
using SaraAgro.Api.DTOs.Otp;
using SaraAgro.Api.Interfaces.Otp;

namespace SaraAgro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OtpController : ControllerBase
{
    private readonly IOtpService _otpService;

    public OtpController(IOtpService otpService)
    {
        _otpService = otpService;
    }

    [HttpPost("send")]
    public async Task<ActionResult<SendOtpResponse>> SendOtp(
        [FromBody] SendOtpRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _otpService.SendOtpAsync(
            request,
            cancellationToken);

        return Ok(response);
    }

    [HttpPost("verify")]
    public async Task<ActionResult<bool>> VerifyOtp(
    [FromBody] VerifyOtpRequest request,
    CancellationToken cancellationToken)
    {
        var result = await _otpService.VerifyOtpAsync(
            request,
            cancellationToken);

        return Ok(result);
    }
}