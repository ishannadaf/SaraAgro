using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaraAgro.Api.DTOs.Billing;
using SaraAgro.Api.Extensions;
using SaraAgro.Api.Interfaces.Billing;

namespace SaraAgro.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class BillingController : ControllerBase
{
    private readonly IBillingService _billingService;

    public BillingController(
        IBillingService billingService)
    {
        _billingService = billingService;
    }

    // =========================================================
    // GENERATE BILL
    // =========================================================

    [HttpPost("generate")]
    public async Task<ActionResult<BillResponse>> GenerateBill(
        [FromBody] GenerateBillRequest request,
        CancellationToken cancellationToken)
    {
        var clientId =
            User.GetClientId();

        var bill =
            await _billingService.GenerateBillAsync(
                clientId,
                request,
                cancellationToken);

        return Ok(bill);
    }

    // =========================================================
    // GET BILLS
    // =========================================================

    [HttpGet]
    public async Task<ActionResult<List<BillResponse>>> GetBills(
        [FromQuery] int? customerId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var clientId =
            User.GetClientId();

        var bills =
            await _billingService.GetBillsAsync(
                clientId,
                customerId,
                fromDate,
                toDate,
                cancellationToken);

        return Ok(bills);
    }

    // =========================================================
    // GET BILL BY ID
    // =========================================================

    [HttpGet("{billId:int}")]
    public async Task<ActionResult<BillResponse>> GetBill(
        int billId,
        CancellationToken cancellationToken)
    {
        var clientId =
            User.GetClientId();

        var bill =
            await _billingService.GetBillByIdAsync(
                clientId,
                billId,
                cancellationToken);

        if (bill == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Bill not found."
            });
        }

        return Ok(bill);
    }

    // =========================================================
    // ADD PAYMENT
    // =========================================================

    [HttpPost("{billId:int}/payments")]
    public async Task<ActionResult<BillPaymentResponse>> AddPayment(
        int billId,
        [FromBody] AddBillPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var clientId =
            User.GetClientId();

        var payment =
            await _billingService.AddPaymentAsync(
                clientId,
                billId,
                request,
                cancellationToken);

        return Ok(payment);
    }

    // =========================================================
    // PAYMENT HISTORY
    // =========================================================

    [HttpGet("{billId:int}/payments")]
    public async Task<ActionResult<List<BillPaymentResponse>>> GetPayments(
        int billId,
        CancellationToken cancellationToken)
    {
        var clientId =
            User.GetClientId();

        var payments =
            await _billingService.GetPaymentsAsync(
                clientId,
                billId,
                cancellationToken);

        return Ok(payments);
    }
}