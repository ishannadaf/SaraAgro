using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaraAgro.Api.DTOs.MilkDistribution;
using SaraAgro.Api.Extensions;
using SaraAgro.Api.Interfaces.MilkDistribution;

namespace SaraAgro.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class MilkDistributionController : ControllerBase
{
    private readonly IMilkDistributionService _milkDistributionService;

    public MilkDistributionController(
        IMilkDistributionService milkDistributionService)
    {
        _milkDistributionService = milkDistributionService;
    }


    // =========================================================
    // CREATE DISTRIBUTION
    // =========================================================

    [HttpPost]
    public async Task<ActionResult<MilkDistributionResponse>>
        CreateDistribution(
            [FromBody] CreateMilkDistributionRequest request,
            CancellationToken cancellationToken)
    {
        var clientId = User.GetClientId();

        var response =
            await _milkDistributionService.CreateDistributionAsync(
                clientId,
                request,
                cancellationToken);

        return Ok(response);
    }


    // =========================================================
    // GET DISTRIBUTIONS
    // =========================================================

    [HttpGet]
    public async Task<ActionResult<List<MilkDistributionResponse>>>
        GetDistributions(
            [FromQuery] DateTime? date,
            [FromQuery] string? session,
            [FromQuery] string? search,
            CancellationToken cancellationToken)
    {
        var clientId = User.GetClientId();

        var distributions =
            await _milkDistributionService.GetDistributionsAsync(
                clientId,
                date,
                session,
                search,
                cancellationToken);

        return Ok(distributions);
    }


    // =========================================================
    // GET CUSTOMER DISTRIBUTIONS
    // =========================================================

    [HttpGet("customer/{customerId:int}")]
    public async Task<ActionResult<List<MilkDistributionResponse>>>
        GetCustomerDistributions(
            int customerId,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            CancellationToken cancellationToken)
    {
        var clientId = User.GetClientId();

        var distributions =
            await _milkDistributionService
                .GetCustomerDistributionsAsync(
                    clientId,
                    customerId,
                    fromDate,
                    toDate,
                    cancellationToken);

        return Ok(distributions);
    }


    // =========================================================
    // DAILY SUMMARY
    // =========================================================

    [HttpGet("summary")]
    public async Task<ActionResult<MilkDistributionSummaryResponse>>
        GetDailySummary(
            [FromQuery] DateTime date,
            CancellationToken cancellationToken)
    {
        var clientId = User.GetClientId();

        var summary =
            await _milkDistributionService.GetDailySummaryAsync(
                clientId,
                date,
                cancellationToken);

        return Ok(summary);
    }


    // =========================================================
    // UPDATE DISTRIBUTION
    // =========================================================

    [HttpPut("{distributionId:int}")]
    public async Task<ActionResult<MilkDistributionResponse>>
        UpdateDistribution(
            int distributionId,
            [FromBody] UpdateMilkDistributionRequest request,
            CancellationToken cancellationToken)
    {
        var clientId = User.GetClientId();

        var response =
            await _milkDistributionService.UpdateDistributionAsync(
                clientId,
                distributionId,
                request,
                cancellationToken);

        if (response == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Milk distribution not found."
            });
        }

        return Ok(response);
    }

    // =========================================================
    // MONTHLY SUMMARY
    // =========================================================

    [HttpGet("summary/month")]
    public async Task<ActionResult<MilkDistributionMonthlySummaryResponse>>
        GetMonthlySummary(
            [FromQuery] DateTime month,
            CancellationToken cancellationToken)
    {
        var clientId =
            User.GetClientId();

        var summary =
            await _milkDistributionService
                .GetMonthlySummaryAsync(
                    clientId,
                    month,
                    cancellationToken);

        return Ok(summary);
    }
}