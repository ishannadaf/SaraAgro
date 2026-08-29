using SaraAgro.Api.DTOs.RateMaster;

namespace SaraAgro.Api.Interfaces.RateMaster;

public interface IRateMasterService
{
    Task<List<RateLookupResponse>> GetRatesAsync(
        int clientId,
        CancellationToken cancellationToken = default);

    Task<RateLookupResponse?> GetApplicableRateAsync(
        int clientId,
        int rateGroupId,
        string milkType,
        DateTime distributionDate,
        CancellationToken cancellationToken = default);

    Task<int> CreateRateAsync(
        CreateRateMasterRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateRateAsync(
        int clientId,
        int rateMasterId,
        UpdateRateMasterRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteRateAsync(
        int clientId,
        int rateMasterId,
        CancellationToken cancellationToken = default);
}