namespace SaraAgro.Api.DTOs.Reports;


// =============================================================
// CUSTOMER LEDGER
// =============================================================

public class CustomerLedgerResponse
{
    public int CustomerId { get; set; }

    public string CustomerCode { get; set; } =
        string.Empty;

    public string CustomerName { get; set; } =
        string.Empty;

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public decimal OpeningBalance { get; set; }

    public decimal TotalMilkQuantity { get; set; }

    public decimal TotalMilkAmount { get; set; }

    public decimal TotalPayments { get; set; }

    public decimal ClosingBalance { get; set; }

    public List<CustomerLedgerEntry> Entries { get; set; } =
        new();
}


public class CustomerLedgerEntry
{
    public DateTime Date { get; set; }

    public string Type { get; set; } =
        string.Empty;

    public string Description { get; set; } =
        string.Empty;

    public string Session { get; set; } =
        string.Empty;

    public string MilkType { get; set; } =
        string.Empty;

    public decimal Quantity { get; set; }

    public decimal Rate { get; set; }

    public decimal Debit { get; set; }

    public decimal Credit { get; set; }

    public decimal RunningBalance { get; set; }

    public string PaymentMode { get; set; } =
        string.Empty;

    public string ReferenceNumber { get; set; } =
        string.Empty;

    // ---------------------------------------------------------
    // BILL INFORMATION
    // ---------------------------------------------------------

    public string BillNumber { get; set; } =
        string.Empty;

    public decimal BillMilkAmount { get; set; }

    public decimal PreviousOutstanding { get; set; }

    public decimal TotalPayable { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal BillBalance { get; set; }
}


// =============================================================
// DAILY REPORT
// =============================================================

public class DailyReportResponse
{
    public DateTime Date { get; set; }

    public decimal MorningCowQuantity { get; set; }

    public decimal MorningBuffaloQuantity { get; set; }

    public decimal MorningTotalQuantity { get; set; }

    public decimal MorningAmount { get; set; }

    public decimal EveningCowQuantity { get; set; }

    public decimal EveningBuffaloQuantity { get; set; }

    public decimal EveningTotalQuantity { get; set; }

    public decimal EveningAmount { get; set; }

    public decimal TotalCowQuantity { get; set; }

    public decimal TotalBuffaloQuantity { get; set; }

    public decimal TotalQuantity { get; set; }

    public decimal TotalAmount { get; set; }

    public int CustomerCount { get; set; }

    public List<DailyCustomerReportRow> Customers { get; set; } =
        new();
}


public class DailyCustomerReportRow
{
    public int CustomerId { get; set; }

    public string CustomerCode { get; set; } =
        string.Empty;

    public string CustomerName { get; set; } =
        string.Empty;

    public decimal CowQuantity { get; set; }

    public decimal BuffaloQuantity { get; set; }

    public decimal TotalQuantity { get; set; }

    public decimal TotalAmount { get; set; }
}


// =============================================================
// MONTHLY REPORT
// =============================================================

public class MonthlyReportResponse
{
    public DateTime Month { get; set; }

    public decimal CowQuantity { get; set; }

    public decimal BuffaloQuantity { get; set; }

    public decimal TotalQuantity { get; set; }

    public decimal TotalAmount { get; set; }

    public int CustomerCount { get; set; }

    public int DaysRecorded { get; set; }

    public List<MonthlyCustomerReportRow> Customers { get; set; } =
        new();
}


public class MonthlyCustomerReportRow
{
    public int CustomerId { get; set; }

    public string CustomerCode { get; set; } =
        string.Empty;

    public string CustomerName { get; set; } =
        string.Empty;

    public decimal CowQuantity { get; set; }

    public decimal BuffaloQuantity { get; set; }

    public decimal TotalQuantity { get; set; }

    public decimal TotalAmount { get; set; }
}


// =============================================================
// PENDING AMOUNT REPORT
// =============================================================

public class PendingAmountReportResponse
{
    public DateTime AsOfDate { get; set; }

    public int CustomerCount { get; set; }

    public decimal TotalPendingAmount { get; set; }

    public List<PendingCustomerReportRow> Customers { get; set; } =
        new();
}


public class PendingCustomerReportRow
{
    public int CustomerId { get; set; }

    public string CustomerCode { get; set; } =
        string.Empty;

    public string CustomerName { get; set; } =
        string.Empty;

    public string BillNumber { get; set; } =
        string.Empty;

    public DateTime BillDate { get; set; }

    public decimal TotalPayable { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal PendingAmount { get; set; }

    public string Status { get; set; } =
        string.Empty;
}


// =============================================================
// ALL PAYMENT REPORT
// =============================================================

public class AllPaymentReportResponse
{
    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public decimal TotalPaymentAmount { get; set; }

    public int PaymentCount { get; set; }

    public List<PaymentReportRow> Payments { get; set; } =
        new();
}


public class PaymentReportRow
{
    public int PaymentId { get; set; }

    public int BillId { get; set; }

    public string BillNumber { get; set; } =
        string.Empty;

    public int CustomerId { get; set; }

    public string CustomerCode { get; set; } =
        string.Empty;

    public string CustomerName { get; set; } =
        string.Empty;

    public DateTime PaymentDate { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMode { get; set; } =
        string.Empty;

    public string ReferenceNumber { get; set; } =
        string.Empty;

    public string Notes { get; set; } =
        string.Empty;
}