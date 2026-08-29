using SaraAgro.Api.DTOs.Client;

namespace SaraAgro.Api.Interfaces.Client;

public interface IClientService
{
    Task<int> CreateClientAsync(
        CreateClientRequest request,
        CancellationToken cancellationToken = default);
}