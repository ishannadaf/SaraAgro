namespace SaraAgro.Api.DTOs.Expenses;

public class ExpenseResponse
{
    public int Id { get; set; }

    public int ExpenseAccountId { get; set; }

    public string ExpenseAccountName { get; set; } = string.Empty;

    public DateTime ExpenseDate { get; set; }

    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public string PaymentMode { get; set; } = string.Empty;

    public string? ReferenceNumber { get; set; }
}