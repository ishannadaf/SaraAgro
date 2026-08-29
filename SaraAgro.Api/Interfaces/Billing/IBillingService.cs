using SaraAgro.Api.DTOs.Billing;

namespace SaraAgro.Api.Interfaces.Billing;

public interface IBillingService
{
    Task<BillResponse> GenerateBillAsync(
        int clientId,
        GenerateBillRequest request,
        CancellationToken cancellationToken = default);

    Task<List<BillResponse>> GetBillsAsync(
        int clientId,
        int? customerId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    Task<BillResponse?> GetBillByIdAsync(
        int clientId,
        int billId,
        CancellationToken cancellationToken = default);

    Task<BillPaymentResponse> AddPaymentAsync(
        int clientId,
        int billId,
        AddBillPaymentRequest request,
        CancellationToken cancellationToken = default);

    Task<List<BillPaymentResponse>> GetPaymentsAsync(
        int clientId,
        int billId,
        CancellationToken cancellationToken = default);
}