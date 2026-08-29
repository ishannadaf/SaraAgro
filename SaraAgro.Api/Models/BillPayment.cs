namespace SaraAgro.Api.Models;

public class BillPayment
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public int BillId { get; set; }

    public int CustomerId { get; set; }

    public DateTime PaymentDate { get; set; }

    public decimal Amount { get; set; }

    // Cash / UPI / Bank / Cheque / Other
    public string PaymentMode { get; set; } = "Cash";

    public string ReferenceNumber { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Bill? Bill { get; set; }

    public Customer? Customer { get; set; }

    public Client? Client { get; set; }
}