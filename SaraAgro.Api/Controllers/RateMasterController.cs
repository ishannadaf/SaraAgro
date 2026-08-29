using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaraAgro.Api.DTOs.RateMaster;
using SaraAgro.Api.Extensions;
using SaraAgro.Api.Interfaces.RateMaster;

namespace SaraAgro.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class RateMasterController : ControllerBase
{
    private readonly IRateMasterService _rateMasterService;

    public RateMasterController(
        IRateMasterService rateMasterService)
    {
        _rateMasterService =
            rateMasterService;
    }


    // =========================================================
    // GET ALL RATES
    // =========================================================

    [HttpGet]
    public async Task<ActionResult<List<RateLookupResponse>>> GetRates(
        CancellationToken cancellationToken)
    {
        var clientId =
            User.GetClientId();

        var rates =
            await _rateMasterService.GetRatesAsync(
                clientId,
                cancellationToken);

        return Ok(rates);
    }


    // =========================================================
    // CREATE RATE
    // =========================================================

    [HttpPost]
    public async Task<IActionResult> CreateRate(
        [FromBody] CreateRateMasterRequest request,
        CancellationToken cancellationToken)
    {
        request.ClientId =
            User.GetClientId();

        var rateId =
            await _rateMasterService.CreateRateAsync(
                request,
                cancellationToken);

        return Ok(new
        {
            success = true,
            rateId
        });
    }

    [HttpGet("applicable")]
    public async Task<ActionResult<RateLookupResponse?>> GetApplicableRate(
    [FromQuery] int rateGroupId,
    [FromQuery] string milkType,
    [FromQuery] DateTime distributionDate,
    CancellationToken cancellationToken)
    {
        var clientId = User.GetClientId();

        var rate =
            await _rateMasterService.GetApplicableRateAsync(
                clientId,
                rateGroupId,
                milkType,
                distributionDate,
                cancellationToken);

        if (rate == null)
        {
            return NotFound(
                new
                {
                    message =
                        "No applicable rate found for the selected rate group and milk type."
                });
        }

        return Ok(rate);
    }


    // =========================================================
    // UPDATE RATE
    // =========================================================

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRate(
        int id,
        [FromBody] UpdateRateMasterRequest request,
        CancellationToken cancellationToken)
    {
        var clientId =
            User.GetClientId();

        var updated =
            await _rateMasterService.UpdateRateAsync(
                clientId,
                id,
                request,
                cancellationToken);

        if (!updated)
        {
            return NotFound(new
            {
                message = "Rate not found."
            });
        }

        return Ok(new
        {
            success = true
        });
    }


    // =========================================================
    // DELETE RATE
    // =========================================================

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRate(
        int id,
        CancellationToken cancellationToken)
    {
        var clientId =
            User.GetClientId();

        var deleted =
            await _rateMasterService.DeleteRateAsync(
                clientId,
                id,
                cancellationToken);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Rate not found."
            });
        }

        return Ok(new
        {
            success = true
        });
    }
}