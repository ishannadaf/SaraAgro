namespace SaraAgro.Api.Models;

public class Expense
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public int ExpenseAccountId { get; set; }

    public DateTime ExpenseDate { get; set; }

    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public string PaymentMode { get; set; } = "Cash";

    public string? ReferenceNumber { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Client? Client { get; set; }

    public ExpenseAccount? ExpenseAccount { get; set; }
}