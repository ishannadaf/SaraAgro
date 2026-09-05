namespace SaraAgro.Api.Models;

public class ExpenseAccount
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Client? Client { get; set; }

    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}