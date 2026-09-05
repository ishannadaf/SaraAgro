namespace SaraAgro.Api.DTOs.Expenses;

public class CreateExpenseAccountRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}