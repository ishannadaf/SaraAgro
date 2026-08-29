using Microsoft.EntityFrameworkCore;
using SaraAgro.Api.Data;
using SaraAgro.Api.DTOs.Client;
using SaraAgro.Api.Interfaces.Client;
namespace SaraAgro.Api.Services.Client;

using ClientModel = SaraAgro.Api.Models.Client;

public class ClientService : IClientService
{
    private readonly SaraAgroDbContext _dbContext;

    public ClientService(SaraAgroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> CreateClientAsync(
        CreateClientRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        var code = request.Code.Trim().ToUpperInvariant();

        var codeExists = await _dbContext.Clients
            .AnyAsync(
                x => x.Code == code,
                cancellationToken);

        if (codeExists)
        {
            throw new InvalidOperationException(
                "A client with this code already exists.");
        }

        var client = new ClientModel
        {
            Name = name,
            Code = code,
            PhoneNumber = request.PhoneNumber?.Trim(),
            Email = request.Email?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Clients.Add(client);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return client.Id;
    }
}