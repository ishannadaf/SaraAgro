using SaraAgro.Api.DTOs.RateGroup;

namespace SaraAgro.Api.Interfaces.RateGroup;

public interface IRateGroupService
{
    Task<List<RateGroupResponse>> GetRateGroupsAsync(
        int clientId,
        CancellationToken cancellationToken = default);

    Task<int> CreateRateGroupAsync(
        int clientId,
        CreateRateGroupRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateRateGroupStatusAsync(
        int clientId,
        int rateGroupId,
        bool isActive,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateRateGroupAsync(
    int clientId,
    int rateGroupId,
    UpdateRateGroupRequest request,
    CancellationToken cancellationToken = default);
}