using SaraAgro.Api.DTOs.MilkDistribution;

namespace SaraAgro.Api.Interfaces.MilkDistribution;

public interface IMilkDistributionService
{
    Task<MilkDistributionResponse> CreateDistributionAsync(
        int clientId,
        CreateMilkDistributionRequest request,
        CancellationToken cancellationToken = default);

    Task<List<MilkDistributionResponse>> GetDistributionsAsync(
    int clientId,
    DateTime? date,
    string? session,
    string? search,
    CancellationToken cancellationToken = default);

    Task<List<MilkDistributionResponse>> GetCustomerDistributionsAsync(
    int clientId,
    int customerId,
    DateTime? fromDate,
    DateTime? toDate,
    CancellationToken cancellationToken = default);

    Task<MilkDistributionSummaryResponse> GetDailySummaryAsync(
    int clientId,
    DateTime date,
    CancellationToken cancellationToken = default);

    Task<MilkDistributionResponse?> UpdateDistributionAsync(
    int clientId,
    int distributionId,
    UpdateMilkDistributionRequest request,
    CancellationToken cancellationToken = default);

    Task<MilkDistributionMonthlySummaryResponse>
    GetMonthlySummaryAsync(
        int clientId,
        DateTime month,
        CancellationToken cancellationToken = default);
}