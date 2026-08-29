using Microsoft.EntityFrameworkCore;
using SaraAgro.Api.Data;
using SaraAgro.Api.DTOs.Billing;
using SaraAgro.Api.Interfaces.Billing;
using SaraAgro.Api.Models;

namespace SaraAgro.Api.Services.Billing;

public class BillingService : IBillingService
{
    private readonly SaraAgroDbContext _dbContext;

    public BillingService(
        SaraAgroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // =========================================================
    // GENERATE BILL
    // =========================================================

    public async Task<BillResponse> GenerateBillAsync(
        int clientId,
        GenerateBillRequest request,
        CancellationToken cancellationToken = default)
    {
        var fromDate = request.FromDate.Date;
        var toDate = request.ToDate.Date;

        if (toDate < fromDate)
        {
            throw new ArgumentException(
                "To date cannot be earlier than From date.");
        }

        var customer =
            await _dbContext.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == request.CustomerId &&
                        x.ClientId == clientId,
                    cancellationToken);

        if (customer == null)
        {
            throw new InvalidOperationException(
                "Customer not found.");
        }

        // -----------------------------------------------------
        // Prevent duplicate billing period
        // -----------------------------------------------------

        var existingBill =
            await _dbContext.Bills
                .FirstOrDefaultAsync(
                    x =>
                        x.ClientId == clientId &&
                        x.CustomerId == request.CustomerId &&
                        x.FromDate == fromDate &&
                        x.ToDate == toDate,
                    cancellationToken);

        if (existingBill != null)
        {
            throw new InvalidOperationException(
                "A bill already exists for this customer and billing period.");
        }

        // -----------------------------------------------------
        // Calculate milk charges
        // -----------------------------------------------------

        var milkAmount =
            await _dbContext.MilkDistributions
                .Where(
                    x =>
                        x.ClientId == clientId &&
                        x.CustomerId == request.CustomerId &&
                        x.DistributionDate >= fromDate &&
                        x.DistributionDate <= toDate)
                .SumAsync(
                    x => x.Amount,
                    cancellationToken);

        milkAmount =
            decimal.Round(
                milkAmount,
                2,
                MidpointRounding.AwayFromZero);

        // -----------------------------------------------------
        // Get previous outstanding
        //
        // Important:
        // We use the latest previous bill's balance,
        // instead of summing all previous bills.
        // This prevents double counting carried-forward
        // balances.
        // -----------------------------------------------------

        var previousBill =
            await _dbContext.Bills
                .AsNoTracking()
                .Where(
                    x =>
                        x.ClientId == clientId &&
                        x.CustomerId == request.CustomerId &&
                        x.ToDate < fromDate)
                .OrderByDescending(x => x.ToDate)
                .ThenByDescending(x => x.Id)
                .FirstOrDefaultAsync(
                    cancellationToken);

        var previousOutstanding =
            previousBill?.BalanceAmount ?? 0m;

        previousOutstanding =
            decimal.Round(
                previousOutstanding,
                2,
                MidpointRounding.AwayFromZero);

        var totalPayable =
            decimal.Round(
                milkAmount +
                previousOutstanding,
                2,
                MidpointRounding.AwayFromZero);

        // -----------------------------------------------------
        // Generate bill number
        // -----------------------------------------------------

        var billNumber =
            await GenerateBillNumberAsync(
                clientId,
                cancellationToken);

        var bill =
            new Bill
            {
                ClientId = clientId,

                CustomerId =
                    request.CustomerId,

                BillNumber =
                    billNumber,

                FromDate =
                    fromDate,

                ToDate =
                    toDate,

                MilkAmount =
                    milkAmount,

                PreviousOutstanding =
                    previousOutstanding,

                TotalPayable =
                    totalPayable,

                PaidAmount = 0m,

                BalanceAmount =
                    totalPayable,

                Status =
                    totalPayable == 0
                        ? "Paid"
                        : "Unpaid",

                CreatedAt =
                    DateTime.UtcNow
            };

        _dbContext.Bills.Add(bill);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return await GetBillByIdAsync(
                   clientId,
                   bill.Id,
                   cancellationToken)
               ?? throw new InvalidOperationException(
                   "Unable to load generated bill.");
    }

    // =========================================================
    // GET BILLS
    // =========================================================

    public async Task<List<BillResponse>> GetBillsAsync(
        int clientId,
        int? customerId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query =
            _dbContext.Bills
                .AsNoTracking()
                .Include(x => x.Customer)
                .Include(x => x.Payments)
                .Where(x => x.ClientId == clientId);

        if (customerId.HasValue)
        {
            query =
                query.Where(
                    x => x.CustomerId == customerId.Value);
        }

        if (fromDate.HasValue)
        {
            var from =
                fromDate.Value.Date;

            query =
                query.Where(
                    x => x.ToDate >= from);
        }

        if (toDate.HasValue)
        {
            var to =
                toDate.Value.Date;

            query =
                query.Where(
                    x => x.FromDate <= to);
        }

        var bills =
            await query
                .OrderByDescending(x => x.ToDate)
                .ThenByDescending(x => x.Id)
                .ToListAsync(cancellationToken);

        return bills
            .Select(MapBill)
            .ToList();
    }

    // =========================================================
    // GET BILL
    // =========================================================

    public async Task<BillResponse?> GetBillByIdAsync(
        int clientId,
        int billId,
        CancellationToken cancellationToken = default)
    {
        var bill =
            await _dbContext.Bills
                .AsNoTracking()
                .Include(x => x.Customer)
                .Include(x => x.Payments)
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == billId &&
                        x.ClientId == clientId,
                    cancellationToken);

        if (bill == null)
            return null;

        return MapBill(bill);
    }

    // =========================================================
    // ADD PAYMENT
    // =========================================================

    public async Task<BillPaymentResponse> AddPaymentAsync(
        int clientId,
        int billId,
        AddBillPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
        {
            throw new ArgumentException(
                "Payment amount must be greater than zero.");
        }

        var amount =
            decimal.Round(
                request.Amount,
                2,
                MidpointRounding.AwayFromZero);

        var bill =
            await _dbContext.Bills
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == billId &&
                        x.ClientId == clientId,
                    cancellationToken);

        if (bill == null)
        {
            throw new InvalidOperationException(
                "Bill not found.");
        }

        if (amount > bill.BalanceAmount)
        {
            throw new InvalidOperationException(
                $"Payment cannot be greater than the outstanding balance of ₹{bill.BalanceAmount:0.00}.");
        }

        var paymentMode =
            string.IsNullOrWhiteSpace(
                request.PaymentMode)
                ? "Cash"
                : request.PaymentMode.Trim();

        var payment =
            new BillPayment
            {
                ClientId =
                    clientId,

                BillId =
                    bill.Id,

                CustomerId =
                    bill.CustomerId,

                PaymentDate =
                    request.PaymentDate.Date,

                Amount =
                    amount,

                PaymentMode =
                    paymentMode,

                ReferenceNumber =
                    request.ReferenceNumber?.Trim()
                    ?? string.Empty,

                Notes =
                    request.Notes?.Trim()
                    ?? string.Empty,

                CreatedAt =
                    DateTime.UtcNow
            };

        _dbContext.BillPayments.Add(payment);

        // -----------------------------------------------------
        // Update bill balance
        // -----------------------------------------------------

        bill.PaidAmount =
            decimal.Round(
                bill.PaidAmount + amount,
                2,
                MidpointRounding.AwayFromZero);

        bill.BalanceAmount =
            decimal.Round(
                bill.TotalPayable - bill.PaidAmount,
                2,
                MidpointRounding.AwayFromZero);

        if (bill.BalanceAmount <= 0)
        {
            bill.BalanceAmount = 0m;
            bill.Status = "Paid";
        }
        else if (bill.PaidAmount > 0)
        {
            bill.Status = "PartiallyPaid";
        }
        else
        {
            bill.Status = "Unpaid";
        }

        bill.UpdatedAt =
            DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return new BillPaymentResponse
        {
            Id =
                payment.Id,

            BillId =
                payment.BillId,

            PaymentDate =
                payment.PaymentDate,

            Amount =
                payment.Amount,

            PaymentMode =
                payment.PaymentMode,

            ReferenceNumber =
                payment.ReferenceNumber,

            Notes =
                payment.Notes,

            CreatedAt =
                payment.CreatedAt
        };
    }

    // =========================================================
    // GET PAYMENTS
    // =========================================================

    public async Task<List<BillPaymentResponse>> GetPaymentsAsync(
        int clientId,
        int billId,
        CancellationToken cancellationToken = default)
    {
        var billExists =
            await _dbContext.Bills
                .AnyAsync(
                    x =>
                        x.Id == billId &&
                        x.ClientId == clientId,
                    cancellationToken);

        if (!billExists)
        {
            throw new InvalidOperationException(
                "Bill not found.");
        }

        return await _dbContext.BillPayments
            .AsNoTracking()
            .Where(
                x =>
                    x.ClientId == clientId &&
                    x.BillId == billId)
            .OrderByDescending(x => x.PaymentDate)
            .ThenByDescending(x => x.Id)
            .Select(
                x => new BillPaymentResponse
                {
                    Id =
                        x.Id,

                    BillId =
                        x.BillId,

                    PaymentDate =
                        x.PaymentDate,

                    Amount =
                        x.Amount,

                    PaymentMode =
                        x.PaymentMode,

                    ReferenceNumber =
                        x.ReferenceNumber,

                    Notes =
                        x.Notes,

                    CreatedAt =
                        x.CreatedAt
                })
            .ToListAsync(cancellationToken);
    }

    // =========================================================
    // BILL NUMBER
    // =========================================================

    private async Task<string> GenerateBillNumberAsync(
        int clientId,
        CancellationToken cancellationToken)
    {
        var year =
            DateTime.Today.Year;

        var prefix =
            $"BILL-{year}-";

        var lastBill =
            await _dbContext.Bills
                .AsNoTracking()
                .Where(
                    x =>
                        x.ClientId == clientId &&
                        x.BillNumber.StartsWith(prefix))
                .OrderByDescending(x => x.Id)
                .Select(x => x.BillNumber)
                .FirstOrDefaultAsync(
                    cancellationToken);

        var nextNumber = 1;

        if (!string.IsNullOrWhiteSpace(lastBill))
        {
            var numberPart =
                lastBill[prefix.Length..];

            if (int.TryParse(
                    numberPart,
                    out var parsed))
            {
                nextNumber =
                    parsed + 1;
            }
        }

        return $"{prefix}{nextNumber:0000}";
    }

    // =========================================================
    // MAPPER
    // =========================================================

    private static BillResponse MapBill(
        Bill bill)
    {
        return new BillResponse
        {
            Id =
                bill.Id,

            BillNumber =
                bill.BillNumber,

            CustomerId =
                bill.CustomerId,

            CustomerCode =
                bill.Customer?.CustomerCode
                ?? string.Empty,

            CustomerName =
                bill.Customer?.FullName
                ?? string.Empty,

            FromDate =
                bill.FromDate,

            ToDate =
                bill.ToDate,

            MilkAmount =
                bill.MilkAmount,

            PreviousOutstanding =
                bill.PreviousOutstanding,

            TotalPayable =
                bill.TotalPayable,

            PaidAmount =
                bill.PaidAmount,

            BalanceAmount =
                bill.BalanceAmount,

            Status =
                bill.Status,

            CreatedAt =
                bill.CreatedAt,

            Payments =
                bill.Payments
                    .OrderByDescending(x => x.PaymentDate)
                    .ThenByDescending(x => x.Id)
                    .Select(
                        x => new BillPaymentResponse
                        {
                            Id =
                                x.Id,

                            BillId =
                                x.BillId,

                            PaymentDate =
                                x.PaymentDate,

                            Amount =
                                x.Amount,

                            PaymentMode =
                                x.PaymentMode,

                            ReferenceNumber =
                                x.ReferenceNumber,

                            Notes =
                                x.Notes,

                            CreatedAt =
                                x.CreatedAt
                        })
                    .ToList()
        };
    }
}