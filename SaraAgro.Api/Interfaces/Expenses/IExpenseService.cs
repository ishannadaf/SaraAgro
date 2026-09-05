using SaraAgro.Api.DTOs.Expenses;

namespace SaraAgro.Api.Interfaces.Expenses;

public interface IExpenseService
{
    // Expense Accounts
    Task<List<ExpenseAccountResponse>> GetExpenseAccountsAsync(
        int clientId,
        bool includeInactive = false);

    Task<ExpenseAccountResponse> CreateExpenseAccountAsync(
        int clientId,
        CreateExpenseAccountRequest request);

    Task<ExpenseAccountResponse> UpdateExpenseAccountAsync(
        int clientId,
        int id,
        UpdateExpenseAccountRequest request);

    Task DeleteExpenseAccountAsync(
        int clientId,
        int id);

    // Expenses
    Task<List<ExpenseResponse>> GetExpensesAsync(
        int clientId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int? expenseAccountId = null);

    Task<ExpenseResponse> GetExpenseByIdAsync(
        int clientId,
        int id);

    Task<ExpenseResponse> CreateExpenseAsync(
        int clientId,
        CreateExpenseRequest request);

    Task<ExpenseResponse> UpdateExpenseAsync(
        int clientId,
        int id,
        CreateExpenseRequest request);

    Task DeleteExpenseAsync(
        int clientId,
        int id);

    // Summary
    Task<ExpenseSummaryResponse> GetExpenseSummaryAsync(
        int clientId,
        DateTime fromDate,
        DateTime toDate);
}