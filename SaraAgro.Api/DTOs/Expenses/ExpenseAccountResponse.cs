namespace SaraAgro.Api.DTOs.Expenses;

public class ExpenseAccountResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public decimal TotalAmount { get; set; }
}