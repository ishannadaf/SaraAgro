using Microsoft.EntityFrameworkCore;
using SaraAgro.Api.Data;
using SaraAgro.Api.DTOs.Reports;
using SaraAgro.Api.Interfaces.Reports;

namespace SaraAgro.Api.Services.Reports;

public class ReportsService : IReportsService
{
    private readonly SaraAgroDbContext _dbContext;

    public ReportsService(
        SaraAgroDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    // =========================================================
    // CUSTOMER LEDGER
    // =========================================================

    public async Task<CustomerLedgerResponse?>
        GetCustomerLedgerAsync(
            int clientId,
            int customerId,
            DateTime fromDate,
            DateTime toDate,
            CancellationToken cancellationToken = default)
    {
        var from =
            fromDate.Date;

        var to =
            toDate.Date;


        if (to < from)
        {
            throw new ArgumentException(
                "To date cannot be earlier than From date.");
        }


        var toExclusive =
            to.AddDays(1);


        // -----------------------------------------------------
        // CUSTOMER
        // -----------------------------------------------------

        var customer =
            await _dbContext.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == customerId &&
                        x.ClientId == clientId,
                    cancellationToken);


        if (customer == null)
            return null;


        // -----------------------------------------------------
        // HISTORICAL MILK
        // -----------------------------------------------------

        var previousMilkAmount =
            await _dbContext.MilkDistributions
                .AsNoTracking()
                .Where(
                    x =>
                        x.ClientId == clientId &&
                        x.CustomerId == customerId &&
                        x.DistributionDate < from)
                .SumAsync(
                    x => x.Amount,
                    cancellationToken);


        // -----------------------------------------------------
        // HISTORICAL PAYMENTS
        // -----------------------------------------------------

        var previousPayments =
            await _dbContext.BillPayments
                .AsNoTracking()
                .Where(
                    x =>
                        x.ClientId == clientId &&
                        x.CustomerId == customerId &&
                        x.PaymentDate < from)
                .SumAsync(
                    x => x.Amount,
                    cancellationToken);


        var openingBalance =
            decimal.Round(
                previousMilkAmount -
                previousPayments,
                2,
                MidpointRounding.AwayFromZero);


        // -----------------------------------------------------
        // CURRENT DISTRIBUTIONS
        //
        // IMPORTANT:
        // [from, toExclusive)
        // handles DateTime values containing time.
        // -----------------------------------------------------

        var distributions =
            await _dbContext.MilkDistributions
                .AsNoTracking()
                .Where(
                    x =>
                        x.ClientId == clientId &&
                        x.CustomerId == customerId &&
                        x.DistributionDate >= from &&
                        x.DistributionDate < toExclusive)
                .OrderBy(x => x.DistributionDate)
                .ThenBy(x => x.Session)
                .ThenBy(x => x.Id)
                .ToListAsync(
                    cancellationToken);


        // -----------------------------------------------------
        // CURRENT PAYMENTS
        // -----------------------------------------------------

        var payments =
            await _dbContext.BillPayments
                .AsNoTracking()
                .Include(x => x.Bill)
                .Where(
                    x =>
                        x.ClientId == clientId &&
                        x.CustomerId == customerId &&
                        x.PaymentDate >= from &&
                        x.PaymentDate < toExclusive)
                .OrderBy(x => x.PaymentDate)
                .ThenBy(x => x.Id)
                .ToListAsync(
                    cancellationToken);


        // -----------------------------------------------------
        // CURRENT BILLS
        //
        // Bills are INFORMATIONAL ledger rows.
        //
        // We do NOT add TotalPayable to Debit because the milk
        // distributions have already been counted as debits.
        // Otherwise the customer's balance would be doubled.
        // -----------------------------------------------------

        var bills =
            await _dbContext.Bills
                .AsNoTracking()
                .Where(
                    x =>
                        x.ClientId == clientId &&
                        x.CustomerId == customerId &&
                        x.ToDate >= from &&
                        x.ToDate < toExclusive)
                .OrderBy(x => x.ToDate)
                .ThenBy(x => x.Id)
                .ToListAsync(
                    cancellationToken);


        // -----------------------------------------------------
        // LEDGER ENTRIES
        // -----------------------------------------------------

        var entries =
            new List<CustomerLedgerEntry>();


        // -----------------------------------------------------
        // MILK
        // -----------------------------------------------------

        foreach (var distribution in distributions)
        {
            entries.Add(
                new CustomerLedgerEntry
                {
                    Date =
                        distribution.DistributionDate,

                    Type =
                        "Milk",

                    Description =
                        distribution.Session == "M"
                            ? $"Morning {distribution.MilkType} Milk"
                            : $"Evening {distribution.MilkType} Milk",

                    Session =
                        distribution.Session,

                    MilkType =
                        distribution.MilkType,

                    Quantity =
                        distribution.Quantity,

                    Rate =
                        distribution.Rate,

                    Debit =
                        distribution.Amount,

                    Credit =
                        0m
                });
        }


        // -----------------------------------------------------
        // BILLS
        // -----------------------------------------------------

        foreach (var bill in bills)
        {
            entries.Add(
                new CustomerLedgerEntry
                {
                    Date =
                        bill.ToDate,

                    Type =
                        "Bill",

                    Description =
                        $"Bill {bill.BillNumber}",

                    Quantity =
                        0m,

                    Rate =
                        0m,

                    Debit =
                        0m,

                    Credit =
                        0m,

                    BillNumber =
                        bill.BillNumber,

                    BillMilkAmount =
                        bill.MilkAmount,

                    PreviousOutstanding =
                        bill.PreviousOutstanding,

                    TotalPayable =
                        bill.TotalPayable,

                    PaidAmount =
                        bill.PaidAmount,

                    BillBalance =
                        bill.BalanceAmount
                });
        }


        // -----------------------------------------------------
        // PAYMENTS
        // -----------------------------------------------------

        foreach (var payment in payments)
        {
            entries.Add(
                new CustomerLedgerEntry
                {
                    Date =
                        payment.PaymentDate,

                    Type =
                        "Payment",

                    Description =
                        $"Payment - {payment.Bill?.BillNumber ?? "Bill"}",

                    Quantity =
                        0m,

                    Rate =
                        0m,

                    Debit =
                        0m,

                    Credit =
                        payment.Amount,

                    PaymentMode =
                        payment.PaymentMode,

                    ReferenceNumber =
                        payment.ReferenceNumber,

                    BillNumber =
                        payment.Bill?.BillNumber
                        ?? string.Empty
                });
        }


        // -----------------------------------------------------
        // SORT
        // -----------------------------------------------------

        entries =
            entries
                .OrderBy(x => x.Date)
                .ThenBy(
                    x =>
                        x.Type switch
                        {
                            "Milk" => 0,
                            "Bill" => 1,
                            "Payment" => 2,
                            _ => 3
                        })
                .ToList();


        // -----------------------------------------------------
        // RUNNING BALANCE
        // -----------------------------------------------------

        var runningBalance =
            openingBalance;


        foreach (var entry in entries)
        {
            runningBalance =
                decimal.Round(
                    runningBalance +
                    entry.Debit -
                    entry.Credit,
                    2,
                    MidpointRounding.AwayFromZero);

            entry.RunningBalance =
                runningBalance;
        }


        // -----------------------------------------------------
        // TOTALS
        // -----------------------------------------------------

        var totalMilkQuantity =
            distributions.Sum(
                x => x.Quantity);


        var totalMilkAmount =
            distributions.Sum(
                x => x.Amount);


        var totalPayments =
            payments.Sum(
                x => x.Amount);


        var closingBalance =
            decimal.Round(
                openingBalance +
                totalMilkAmount -
                totalPayments,
                2,
                MidpointRounding.AwayFromZero);


        return new CustomerLedgerResponse
        {
            CustomerId =
                customer.Id,

            CustomerCode =
                customer.CustomerCode,

            CustomerName =
                customer.FullName,

            FromDate =
                from,

            ToDate =
                to,

            OpeningBalance =
                openingBalance,

            TotalMilkQuantity =
                totalMilkQuantity,

            TotalMilkAmount =
                totalMilkAmount,

            TotalPayments =
                totalPayments,

            ClosingBalance =
                closingBalance,

            Entries =
                entries
        };
    }


    // =========================================================
    // DAILY REPORT
    // =========================================================

    public async Task<DailyReportResponse>
        GetDailyReportAsync(
            int clientId,
            DateTime date,
            CancellationToken cancellationToken = default)
    {
        var reportDate =
            date.Date;

        var nextDate =
            reportDate.AddDays(1);


        var distributions =
            await _dbContext.MilkDistributions
                .AsNoTracking()
                .Include(x => x.Customer)
                .Where(
                    x =>
                        x.ClientId == clientId &&
                        x.DistributionDate >= reportDate &&
                        x.DistributionDate < nextDate)
                .ToListAsync(
                    cancellationToken);


        var morning =
            distributions
                .Where(x => x.Session == "M")
                .ToList();


        var evening =
            distributions
                .Where(x => x.Session == "E")
                .ToList();


        var morningCow =
            morning
                .Where(x => x.MilkType == "Cow")
                .Sum(x => x.Quantity);


        var morningBuffalo =
            morning
                .Where(x => x.MilkType == "Buffalo")
                .Sum(x => x.Quantity);


        var eveningCow =
            evening
                .Where(x => x.MilkType == "Cow")
                .Sum(x => x.Quantity);


        var eveningBuffalo =
            evening
                .Where(x => x.MilkType == "Buffalo")
                .Sum(x => x.Quantity);


        var customerRows =
            distributions
                .GroupBy(
                    x => new
                    {
                        x.CustomerId,
                        CustomerCode =
                            x.Customer!.CustomerCode,
                        CustomerName =
                            x.Customer.FullName
                    })
                .Select(
                    group =>
                    {
                        var cow =
                            group
                                .Where(
                                    x => x.MilkType == "Cow")
                                .Sum(
                                    x => x.Quantity);

                        var buffalo =
                            group
                                .Where(
                                    x => x.MilkType == "Buffalo")
                                .Sum(
                                    x => x.Quantity);

                        return new DailyCustomerReportRow
                        {
                            CustomerId =
                                group.Key.CustomerId,

                            CustomerCode =
                                group.Key.CustomerCode,

                            CustomerName =
                                group.Key.CustomerName,

                            CowQuantity =
                                cow,

                            BuffaloQuantity =
                                buffalo,

                            TotalQuantity =
                                cow + buffalo,

                            TotalAmount =
                                group.Sum(x => x.Amount)
                        };
                    })
                .OrderBy(x => x.CustomerCode)
                .ToList();


        return new DailyReportResponse
        {
            Date =
                reportDate,

            MorningCowQuantity =
                morningCow,

            MorningBuffaloQuantity =
                morningBuffalo,

            MorningTotalQuantity =
                morningCow +
                morningBuffalo,

            MorningAmount =
                morning.Sum(x => x.Amount),

            EveningCowQuantity =
                eveningCow,

            EveningBuffaloQuantity =
                eveningBuffalo,

            EveningTotalQuantity =
                eveningCow +
                eveningBuffalo,

            EveningAmount =
                evening.Sum(x => x.Amount),

            TotalCowQuantity =
                morningCow +
                eveningCow,

            TotalBuffaloQuantity =
                morningBuffalo +
                eveningBuffalo,

            TotalQuantity =
                distributions.Sum(x => x.Quantity),

            TotalAmount =
                distributions.Sum(x => x.Amount),

            CustomerCount =
                customerRows.Count,

            Customers =
                customerRows
        };
    }


    // =========================================================
    // MONTHLY REPORT
    // =========================================================

    public async Task<MonthlyReportResponse>
        GetMonthlyReportAsync(
            int clientId,
            DateTime month,
            CancellationToken cancellationToken = default)
    {
        var monthStart =
            new DateTime(
                month.Year,
                month.Month,
                1);

        var monthEnd =
            monthStart.AddMonths(1);


        var distributions =
            await _dbContext.MilkDistributions
                .AsNoTracking()
                .Include(x => x.Customer)
                .Where(
                    x =>
                        x.ClientId == clientId &&
                        x.DistributionDate >= monthStart &&
                        x.DistributionDate < monthEnd)
                .ToListAsync(
                    cancellationToken);


        var customerRows =
            distributions
                .GroupBy(
                    x => new
                    {
                        x.CustomerId,
                        CustomerCode =
                            x.Customer!.CustomerCode,
                        CustomerName =
                            x.Customer.FullName
                    })
                .Select(
                    group =>
                    {
                        var cow =
                            group
                                .Where(
                                    x => x.MilkType == "Cow")
                                .Sum(
                                    x => x.Quantity);

                        var buffalo =
                            group
                                .Where(
                                    x => x.MilkType == "Buffalo")
                                .Sum(
                                    x => x.Quantity);

                        return new MonthlyCustomerReportRow
                        {
                            CustomerId =
                                group.Key.CustomerId,

                            CustomerCode =
                                group.Key.CustomerCode,

                            CustomerName =
                                group.Key.CustomerName,

                            CowQuantity =
                                cow,

                            BuffaloQuantity =
                                buffalo,

                            TotalQuantity =
                                cow + buffalo,

                            TotalAmount =
                                group.Sum(x => x.Amount)
                        };
                    })
                .OrderBy(x => x.CustomerCode)
                .ToList();


        var daysRecorded =
            distributions
                .Select(
                    x => x.DistributionDate.Date)
                .Distinct()
                .Count();


        return new MonthlyReportResponse
        {
            Month =
                monthStart,

            CowQuantity =
                distributions
                    .Where(x => x.MilkType == "Cow")
                    .Sum(x => x.Quantity),

            BuffaloQuantity =
                distributions
                    .Where(x => x.MilkType == "Buffalo")
                    .Sum(x => x.Quantity),

            TotalQuantity =
                distributions.Sum(x => x.Quantity),

            TotalAmount =
                distributions.Sum(x => x.Amount),

            CustomerCount =
                customerRows.Count,

            DaysRecorded =
                daysRecorded,

            Customers =
                customerRows
        };
    }


    // =========================================================
    // PENDING AMOUNT
    // =========================================================

    // =========================================================
    // PENDING AMOUNT
    // =========================================================

    public async Task<PendingAmountReportResponse>
        GetPendingAmountReportAsync(
            int clientId,
            DateTime asOfDate,
            CancellationToken cancellationToken = default)
    {
        var date =
            asOfDate.Date;


        // =====================================================
        // GET ALL BILLS UP TO SELECTED DATE
        // =====================================================

        var bills =
            await _dbContext.Bills
                .AsNoTracking()
                .Include(x => x.Customer)
                .Where(
                    x =>
                        x.ClientId == clientId &&
                        x.ToDate <= date)
                .OrderBy(x => x.CustomerId)
                .ThenBy(x => x.ToDate)
                .ThenBy(x => x.Id)
                .ToListAsync(
                    cancellationToken);


        if (bills.Count == 0)
        {
            return new PendingAmountReportResponse
            {
                AsOfDate =
                    date,

                CustomerCount =
                    0,

                TotalPendingAmount =
                    0m,

                Customers =
                    new List<PendingCustomerReportRow>()
            };
        }


        // =====================================================
        // GET ALL PAYMENTS UP TO SELECTED DATE
        //
        // IMPORTANT:
        // We calculate payments from BillPayments instead of
        // trusting only the latest Bill.PaidAmount.
        // =====================================================

        var payments =
            await _dbContext.BillPayments
                .AsNoTracking()
                .Where(
                    x =>
                        x.ClientId == clientId &&
                        x.PaymentDate <= date)
                .GroupBy(x => x.BillId)
                .Select(
                    group => new
                    {
                        BillId =
                            group.Key,

                        PaidAmount =
                            group.Sum(x => x.Amount)
                    })
                .ToListAsync(
                    cancellationToken);


        var paymentLookup =
            payments.ToDictionary(
                x => x.BillId,
                x => x.PaidAmount);


        // =====================================================
        // GROUP BILLS BY CUSTOMER
        // =====================================================

        var pendingCustomers =
            bills
                .GroupBy(
                    x => new
                    {
                        x.CustomerId,

                        CustomerCode =
                            x.Customer?.CustomerCode
                            ?? string.Empty,

                        CustomerName =
                            x.Customer?.FullName
                            ?? string.Empty
                    })
                .Select(
                    group =>
                    {
                        var customerBills =
                            group
                                .OrderBy(x => x.ToDate)
                                .ThenBy(x => x.Id)
                                .ToList();


                        // -------------------------------------------------
                        // TOTAL PAYABLE ACROSS ALL BILLS
                        // -------------------------------------------------

                        var totalPayable =
                            customerBills.Sum(
                                x => x.TotalPayable);


                        // -------------------------------------------------
                        // TOTAL PAYMENTS ACROSS ALL BILLS
                        // -------------------------------------------------

                        var paidAmount =
                            customerBills.Sum(
                                x =>
                                    paymentLookup.TryGetValue(
                                        x.Id,
                                        out var payment)
                                        ? payment
                                        : 0m);


                        // -------------------------------------------------
                        // PENDING
                        // -------------------------------------------------

                        var pendingAmount =
                            Math.Max(
                                0m,
                                decimal.Round(
                                    totalPayable -
                                    paidAmount,
                                    2,
                                    MidpointRounding.AwayFromZero));


                        // -------------------------------------------------
                        // LATEST BILL
                        //
                        // Used only for display information.
                        // The pending amount itself is calculated from
                        // ALL bills + ALL payments.
                        // -------------------------------------------------

                        var latestBill =
                            customerBills
                                .OrderByDescending(
                                    x => x.ToDate)
                                .ThenByDescending(
                                    x => x.Id)
                                .First();


                        var latestBillPaid =
                            paymentLookup.TryGetValue(
                                latestBill.Id,
                                out var latestPayment)
                                ? latestPayment
                                : 0m;


                        var latestBillBalance =
                            Math.Max(
                                0m,
                                decimal.Round(
                                    latestBill.TotalPayable -
                                    latestBillPaid,
                                    2,
                                    MidpointRounding.AwayFromZero));


                        return new
                        {
                            group.Key.CustomerId,
                            group.Key.CustomerCode,
                            group.Key.CustomerName,

                            TotalPayable =
                                totalPayable,

                            PaidAmount =
                                paidAmount,

                            PendingAmount =
                                pendingAmount,

                            LatestBill =
                                latestBill,

                            LatestBillPaid =
                                latestBillPaid,

                            LatestBillBalance =
                                latestBillBalance
                        };
                    })
                .Where(
                    x =>
                        x.PendingAmount > 0)
                .Select(
                    x =>
                    {
                        // =================================================
                        // DISPLAY STRATEGY
                        //
                        // Show the customer's CURRENT/LATEST bill details,
                        // but the PendingAmount is the COMPLETE outstanding
                        // amount across all bills.
                        // =================================================

                        var status =
                            x.PendingAmount <= 0
                                ? "Paid"
                                : x.PaidAmount > 0
                                    ? "PartiallyPaid"
                                    : "Unpaid";


                        return new PendingCustomerReportRow
                        {
                            CustomerId =
                                x.CustomerId,

                            CustomerCode =
                                x.CustomerCode,

                            CustomerName =
                                x.CustomerName,

                            BillNumber =
                                x.LatestBill.BillNumber,

                            BillDate =
                                x.LatestBill.ToDate,

                            TotalPayable =
                                x.TotalPayable,

                            PaidAmount =
                                x.PaidAmount,

                            PendingAmount =
                                x.PendingAmount,

                            Status =
                                status
                        };
                    })
                .OrderByDescending(
                    x => x.PendingAmount)
                .ThenBy(
                    x => x.CustomerCode)
                .ToList();


        // =====================================================
        // FINAL RESPONSE
        // =====================================================

        return new PendingAmountReportResponse
        {
            AsOfDate =
                date,

            CustomerCount =
                pendingCustomers.Count,

            TotalPendingAmount =
                pendingCustomers.Sum(
                    x => x.PendingAmount),
            Customers =
                pendingCustomers
        };
    }

    // =========================================================
    // ALL PAYMENTS
    // =========================================================

    public async Task<AllPaymentReportResponse>
        GetAllPaymentReportAsync(
            int clientId,
            DateTime fromDate,
            DateTime toDate,
            CancellationToken cancellationToken = default)
    {
        var from =
            fromDate.Date;

        var to =
            toDate.Date;


        if (to < from)
        {
            throw new ArgumentException(
                "To date cannot be earlier than from date.");
        }


        var toExclusive =
            to.AddDays(1);


        var payments =
            await _dbContext.BillPayments
                .AsNoTracking()
                .Include(x => x.Customer)
                .Include(x => x.Bill)
                .Where(
                    x =>
                        x.ClientId == clientId &&
                        x.PaymentDate >= from &&
                        x.PaymentDate < toExclusive)
                .OrderByDescending(x => x.PaymentDate)
                .ThenByDescending(x => x.Id)
                .ToListAsync(
                    cancellationToken);


        var rows =
            payments
                .Select(
                    x => new PaymentReportRow
                    {
                        PaymentId =
                            x.Id,

                        BillId =
                            x.BillId,

                        BillNumber =
                            x.Bill?.BillNumber
                            ?? string.Empty,

                        CustomerId =
                            x.CustomerId,

                        CustomerCode =
                            x.Customer?.CustomerCode
                            ?? string.Empty,

                        CustomerName =
                            x.Customer?.FullName
                            ?? string.Empty,

                        PaymentDate =
                            x.PaymentDate,

                        Amount =
                            x.Amount,

                        PaymentMode =
                            x.PaymentMode,

                        ReferenceNumber =
                            x.ReferenceNumber,

                        Notes =
                            x.Notes
                    })
                .ToList();


        return new AllPaymentReportResponse
        {
            FromDate =
                from,

            ToDate =
                to,

            TotalPaymentAmount =
                payments.Sum(x => x.Amount),

            PaymentCount =
                payments.Count,

            Payments =
                rows
        };
    }
}