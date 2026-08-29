using SaraAgro.Api.DTOs.Customer;

namespace SaraAgro.Api.Interfaces.Customer;

public interface ICustomerService
{
    Task<int> CreateCustomerAsync(
    int clientId,
    CreateCustomerRequest request,
    CancellationToken cancellationToken = default);

    Task<List<CustomerResponse>> GetCustomersAsync(
        int clientId,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateCustomerStatusAsync(
    int clientId,
    int customerId,
    UpdateCustomerStatusRequest request,
    CancellationToken cancellationToken = default);

    Task<CustomerResponse?> GetCustomerByIdAsync(
    int clientId,
    int customerId,
    CancellationToken cancellationToken = default);

    Task<bool> UpdateCustomerAsync(
    int clientId,
    int customerId,
    UpdateCustomerRequest request,
    CancellationToken cancellationToken = default);

    Task<CustomerAccountSummaryResponse?> GetCustomerAccountSummaryAsync(
    int clientId,
    int customerId,
    DateTime fromDate,
    DateTime toDate,
    CancellationToken cancellationToken = default);
}