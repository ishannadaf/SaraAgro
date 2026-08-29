namespace SaraAgro.Api.DTOs.Billing;

public class BillPaymentResponse
{
    public int Id { get; set; }

    public int BillId { get; set; }

    public DateTime PaymentDate { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMode { get; set; } = string.Empty;

    public string ReferenceNumber { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}