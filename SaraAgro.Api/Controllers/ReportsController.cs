using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaraAgro.Api.DTOs.Reports;
using SaraAgro.Api.Extensions;
using SaraAgro.Api.Interfaces.Reports;
using SaraAgro.Api.Services.Reports;

namespace SaraAgro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportsService _reportsService;
    private readonly ReportPdfService _reportPdfService;


    public ReportsController(
        IReportsService reportsService,
        ReportPdfService reportPdfService)
    {
        _reportsService =
            reportsService;

        _reportPdfService =
            reportPdfService;
    }


    // =========================================================
    // CUSTOMER LEDGER
    // =========================================================

    [HttpGet("customer-ledger")]
    public async Task<ActionResult<CustomerLedgerResponse>>
        GetCustomerLedger(
            [FromQuery] int customerId,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            CancellationToken cancellationToken)
    {
        var clientId =
            User.GetClientId();

        var result =
            await _reportsService
                .GetCustomerLedgerAsync(
                    clientId,
                    customerId,
                    fromDate,
                    toDate,
                    cancellationToken);

        if (result == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Customer not found."
            });
        }

        return Ok(result);
    }


    // =========================================================
    // CUSTOMER LEDGER PDF
    // =========================================================

    [HttpGet("customer-ledger/pdf")]
    public async Task<IActionResult>
        GetCustomerLedgerPdf(
            [FromQuery] int customerId,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            CancellationToken cancellationToken)
    {
        var clientId =
            User.GetClientId();

        var report =
            await _reportsService
                .GetCustomerLedgerAsync(
                    clientId,
                    customerId,
                    fromDate,
                    toDate,
                    cancellationToken);

        if (report == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Customer not found."
            });
        }

        var pdf =
            _reportPdfService
                .GenerateCustomerLedgerPdf(
                    report);

        var fileName =
            $"CustomerLedger_{SafeFileName(report.CustomerName)}_" +
            $"{report.FromDate:yyyyMMdd}_{report.ToDate:yyyyMMdd}.pdf";

        return File(
            pdf,
            "application/pdf",
            fileName);
    }


    // =========================================================
    // DAILY REPORT
    // =========================================================

    [HttpGet("daily")]
    public async Task<ActionResult<DailyReportResponse>>
        GetDailyReport(
            [FromQuery] DateTime date,
            CancellationToken cancellationToken)
    {
        var clientId =
            User.GetClientId();

        var result =
            await _reportsService
                .GetDailyReportAsync(
                    clientId,
                    date,
                    cancellationToken);

        return Ok(result);
    }


    // =========================================================
    // DAILY REPORT PDF
    // =========================================================

    [HttpGet("daily/pdf")]
    public async Task<IActionResult>
        GetDailyReportPdf(
            [FromQuery] DateTime date,
            CancellationToken cancellationToken)
    {
        var clientId =
            User.GetClientId();

        var report =
            await _reportsService
                .GetDailyReportAsync(
                    clientId,
                    date,
                    cancellationToken);

        var pdf =
            _reportPdfService
                .GenerateDailyReportPdf(
                    report);

        var fileName =
            $"DailyReport_{report.Date:yyyyMMdd}.pdf";

        return File(
            pdf,
            "application/pdf",
            fileName);
    }


    // =========================================================
    // MONTHLY REPORT
    // =========================================================

    [HttpGet("monthly")]
    public async Task<ActionResult<MonthlyReportResponse>>
        GetMonthlyReport(
            [FromQuery] DateTime month,
            CancellationToken cancellationToken)
    {
        var clientId =
            User.GetClientId();

        var result =
            await _reportsService
                .GetMonthlyReportAsync(
                    clientId,
                    month,
                    cancellationToken);

        return Ok(result);
    }


    // =========================================================
    // MONTHLY REPORT PDF
    // =========================================================

    [HttpGet("monthly/pdf")]
    public async Task<IActionResult>
        GetMonthlyReportPdf(
            [FromQuery] DateTime month,
            CancellationToken cancellationToken)
    {
        var clientId =
            User.GetClientId();

        var report =
            await _reportsService
                .GetMonthlyReportAsync(
                    clientId,
                    month,
                    cancellationToken);

        var pdf =
            _reportPdfService
                .GenerateMonthlyReportPdf(
                    report);

        var fileName =
            $"MonthlyReport_{report.Month:yyyyMM}.pdf";

        return File(
            pdf,
            "application/pdf",
            fileName);
    }


    // =========================================================
    // PENDING AMOUNT REPORT
    // =========================================================

    [HttpGet("pending")]
    public async Task<ActionResult<PendingAmountReportResponse>>
        GetPendingAmountReport(
            [FromQuery] DateTime asOfDate,
            CancellationToken cancellationToken)
    {
        var clientId =
            User.GetClientId();

        var result =
            await _reportsService
                .GetPendingAmountReportAsync(
                    clientId,
                    asOfDate,
                    cancellationToken);

        return Ok(result);
    }


    // =========================================================
    // PENDING AMOUNT REPORT PDF
    // =========================================================

    [HttpGet("pending/pdf")]
    public async Task<IActionResult>
        GetPendingAmountReportPdf(
            [FromQuery] DateTime asOfDate,
            CancellationToken cancellationToken)
    {
        var clientId =
            User.GetClientId();

        var report =
            await _reportsService
                .GetPendingAmountReportAsync(
                    clientId,
                    asOfDate,
                    cancellationToken);

        var pdf =
            _reportPdfService
                .GeneratePendingAmountReportPdf(
                    report);

        var fileName =
            $"PendingAmount_{report.AsOfDate:yyyyMMdd}.pdf";

        return File(
            pdf,
            "application/pdf",
            fileName);
    }


    // =========================================================
    // ALL PAYMENTS
    // =========================================================

    [HttpGet("payments")]
    public async Task<ActionResult<AllPaymentReportResponse>>
        GetAllPaymentReport(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            CancellationToken cancellationToken)
    {
        var clientId =
            User.GetClientId();

        var result =
            await _reportsService
                .GetAllPaymentReportAsync(
                    clientId,
                    fromDate,
                    toDate,
                    cancellationToken);

        return Ok(result);
    }


    // =========================================================
    // ALL PAYMENTS PDF
    // =========================================================

    [HttpGet("payments/pdf")]
    public async Task<IActionResult>
        GetAllPaymentReportPdf(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            CancellationToken cancellationToken)
    {
        var clientId =
            User.GetClientId();

        var report =
            await _reportsService
                .GetAllPaymentReportAsync(
                    clientId,
                    fromDate,
                    toDate,
                    cancellationToken);

        var pdf =
            _reportPdfService
                .GenerateAllPaymentReportPdf(
                    report);

        var fileName =
            $"AllPayments_{report.FromDate:yyyyMMdd}_{report.ToDate:yyyyMMdd}.pdf";

        return File(
            pdf,
            "application/pdf",
            fileName);
    }


    // =========================================================
    // SAFE FILE NAME
    // =========================================================

    private static string SafeFileName(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Customer";

        var invalidCharacters =
            Path.GetInvalidFileNameChars();

        var result =
            string.Join(
                "_",
                value.Split(
                    invalidCharacters,
                    StringSplitOptions.RemoveEmptyEntries));

        return string.IsNullOrWhiteSpace(result)
            ? "Customer"
            : result;
    }
}