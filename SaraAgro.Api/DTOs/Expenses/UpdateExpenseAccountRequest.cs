namespace SaraAgro.Api.DTOs.Expenses;

public class UpdateExpenseAccountRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }
}