namespace SaraAgro.Api.DTOs.Billing;

public class AddBillPaymentRequest
{
    public DateTime PaymentDate { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMode { get; set; } = "Cash";

    public string ReferenceNumber { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;
}