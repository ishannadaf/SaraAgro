using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaraAgro.Api.DTOs.RateGroup;
using SaraAgro.Api.Extensions;
using SaraAgro.Api.Interfaces.RateGroup;

namespace SaraAgro.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class RateGroupController : ControllerBase
{
    private readonly IRateGroupService _rateGroupService;

    public RateGroupController(
        IRateGroupService rateGroupService)
    {
        _rateGroupService = rateGroupService;
    }

    // =========================================================
    // UPDATE RATE GROUP
    // =========================================================

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRateGroup(
        int id,
        [FromBody] UpdateRateGroupRequest request,
        CancellationToken cancellationToken)
    {
        var clientId =
            User.GetClientId();

        var updated =
            await _rateGroupService.UpdateRateGroupAsync(
                clientId,
                id,
                request,
                cancellationToken);

        if (!updated)
        {
            return NotFound(new
            {
                message = "Rate group not found."
            });
        }

        return Ok(new
        {
            success = true
        });
    }

    // =========================================================
    // GET ACTIVE RATE GROUPS
    // =========================================================

    [HttpGet]
    public async Task<ActionResult<List<RateGroupResponse>>> GetRateGroups(
        CancellationToken cancellationToken)
    {
        var clientId = User.GetClientId();

        var rateGroups =
            await _rateGroupService.GetRateGroupsAsync(
                clientId,
                cancellationToken);

        return Ok(rateGroups);
    }


    // =========================================================
    // CREATE RATE GROUP
    // =========================================================

    [HttpPost]
    public async Task<IActionResult> CreateRateGroup(
        [FromBody] CreateRateGroupRequest request,
        CancellationToken cancellationToken)
    {
        var clientId = User.GetClientId();

        var rateGroupId =
            await _rateGroupService.CreateRateGroupAsync(
                clientId,
                request,
                cancellationToken);

        return Ok(new
        {
            success = true,
            rateGroupId
        });
    }


    // =========================================================
    // UPDATE RATE GROUP STATUS
    // =========================================================

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateRateGroupStatus(
        int id,
        [FromBody] bool isActive,
        CancellationToken cancellationToken)
    {
        var clientId = User.GetClientId();

        var updated =
            await _rateGroupService.UpdateRateGroupStatusAsync(
                clientId,
                id,
                isActive,
                cancellationToken);

        if (!updated)
        {
            return NotFound(new
            {
                message = "Rate group not found."
            });
        }

        return Ok(new
        {
            success = true
        });
    }
}