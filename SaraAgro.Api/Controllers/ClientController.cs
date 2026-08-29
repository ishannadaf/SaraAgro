using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaraAgro.Api.DTOs.Client;
using SaraAgro.Api.Interfaces.Client;

namespace SaraAgro.Api.Controllers;

[ApiController]
[Authorize(Roles = "SuperAdmin")]
[Route("api/[controller]")]
public class ClientController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateClient(
        [FromBody] CreateClientRequest request,
        CancellationToken cancellationToken)
    {
        var clientId = await _clientService.CreateClientAsync(
            request,
            cancellationToken);

        return Ok(new
        {
            success = true,
            clientId
        });
    }
}