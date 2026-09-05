using SaraAgro.Api.DTOs.Reports;

namespace SaraAgro.Api.Interfaces.Reports;

public interface IReportsService
{
    Task<CustomerLedgerResponse?>
        GetCustomerLedgerAsync(
            int clientId,
            int customerId,
            DateTime fromDate,
            DateTime toDate,
            CancellationToken cancellationToken = default);


    Task<DailyReportResponse>
        GetDailyReportAsync(
            int clientId,
            DateTime date,
            CancellationToken cancellationToken = default);


    Task<MonthlyReportResponse>
        GetMonthlyReportAsync(
            int clientId,
            DateTime month,
            CancellationToken cancellationToken = default);


    Task<PendingAmountReportResponse>
        GetPendingAmountReportAsync(
            int clientId,
            DateTime asOfDate,
            CancellationToken cancellationToken = default);


    Task<AllPaymentReportResponse>
        GetAllPaymentReportAsync(
            int clientId,
            DateTime fromDate,
            DateTime toDate,
            CancellationToken cancellationToken = default);

    Task<FinancialSummaryResponse>
    GetFinancialSummaryAsync(
        int clientId,
        DateTime date,
        CancellationToken cancellationToken = default);
}