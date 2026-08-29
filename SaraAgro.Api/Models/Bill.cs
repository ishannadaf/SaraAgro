namespace SaraAgro.Api.Models;

public class Bill
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public int CustomerId { get; set; }

    // Example: BILL-2026-0001
    public string BillNumber { get; set; } = string.Empty;

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    // Milk amount generated for this billing period.
    public decimal MilkAmount { get; set; }

    // Outstanding amount carried from previous bill(s).
    public decimal PreviousOutstanding { get; set; }

    // MilkAmount + PreviousOutstanding.
    public decimal TotalPayable { get; set; }

    // Total payments allocated to this bill.
    public decimal PaidAmount { get; set; }

    // TotalPayable - PaidAmount.
    public decimal BalanceAmount { get; set; }

    // Draft / PartiallyPaid / Paid
    public string Status { get; set; } = "Draft";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Customer? Customer { get; set; }

    public Client? Client { get; set; }

    public ICollection<BillPayment> Payments { get; set; }
        = new List<BillPayment>();
}