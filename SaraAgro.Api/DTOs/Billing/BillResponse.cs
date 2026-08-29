namespace SaraAgro.Api.DTOs.Billing;

public class BillResponse
{
    public int Id { get; set; }

    public string BillNumber { get; set; } = string.Empty;

    public int CustomerId { get; set; }

    public string CustomerCode { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public decimal MilkAmount { get; set; }

    public decimal PreviousOutstanding { get; set; }

    public decimal TotalPayable { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal BalanceAmount { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public List<BillPaymentResponse> Payments { get; set; }
        = new();
}