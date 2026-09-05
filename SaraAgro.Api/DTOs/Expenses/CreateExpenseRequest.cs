namespace SaraAgro.Api.DTOs.Expenses;

public class CreateExpenseRequest
{
    public int ExpenseAccountId { get; set; }

    public DateTime ExpenseDate { get; set; }

    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public string PaymentMode { get; set; } = "Cash";

    public string? ReferenceNumber { get; set; }
}