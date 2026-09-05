namespace SaraAgro.Api.DTOs.Expenses;

public class ExpenseSummaryResponse
{
    public decimal TotalExpenses { get; set; }

    public List<ExpenseAccountSummaryResponse> Accounts { get; set; }
        = new();
}

public class ExpenseAccountSummaryResponse
{
    public int ExpenseAccountId { get; set; }

    public string ExpenseAccountName { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }
}