using Microsoft.EntityFrameworkCore;
using SaraAgro.Api.Data;
using SaraAgro.Api.DTOs.Expenses;
using SaraAgro.Api.Interfaces.Expenses;
using SaraAgro.Api.Models;

namespace SaraAgro.Api.Services.Expenses;

public class ExpenseService : IExpenseService
{
    private readonly SaraAgroDbContext _dbContext;

    public ExpenseService(SaraAgroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // =========================================================
    // EXPENSE ACCOUNTS
    // =========================================================

    public async Task<List<ExpenseAccountResponse>> GetExpenseAccountsAsync(
        int clientId,
        bool includeInactive = false)
    {
        var query = _dbContext.ExpenseAccounts
            .AsNoTracking()
            .Where(x => x.ClientId == clientId);

        if (!includeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        return await query
            .OrderBy(x => x.Name)
            .Select(x => new ExpenseAccountResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive,

                TotalAmount = _dbContext.Expenses
                    .Where(e =>
                        e.ClientId == clientId &&
                        e.ExpenseAccountId == x.Id)
                    .Sum(e => (decimal?)e.Amount) ?? 0
            })
            .ToListAsync();
    }


    public async Task<ExpenseAccountResponse> CreateExpenseAccountAsync(
        int clientId,
        CreateExpenseAccountRequest request)
    {
        var name = request.Name?.Trim();

        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException(
                "Expense account name is required.");

        if (name.Length > 100)
            throw new InvalidOperationException(
                "Expense account name cannot exceed 100 characters.");

        var exists = await _dbContext.ExpenseAccounts
            .AnyAsync(x =>
                x.ClientId == clientId &&
                x.Name.ToLower() == name.ToLower());

        if (exists)
            throw new InvalidOperationException(
                "An expense account with this name already exists.");

        var account = new ExpenseAccount
        {
            ClientId = clientId,
            Name = name,
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.ExpenseAccounts.Add(account);

        await _dbContext.SaveChangesAsync();

        return new ExpenseAccountResponse
        {
            Id = account.Id,
            Name = account.Name,
            Description = account.Description,
            IsActive = account.IsActive,
            TotalAmount = 0
        };
    }


    public async Task<ExpenseAccountResponse> UpdateExpenseAccountAsync(
        int clientId,
        int id,
        UpdateExpenseAccountRequest request)
    {
        var account = await _dbContext.ExpenseAccounts
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.ClientId == clientId);

        if (account == null)
            throw new KeyNotFoundException(
                "Expense account not found.");

        var name = request.Name?.Trim();

        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException(
                "Expense account name is required.");

        if (name.Length > 100)
            throw new InvalidOperationException(
                "Expense account name cannot exceed 100 characters.");

        var duplicate = await _dbContext.ExpenseAccounts
            .AnyAsync(x =>
                x.Id != id &&
                x.ClientId == clientId &&
                x.Name.ToLower() == name.ToLower());

        if (duplicate)
            throw new InvalidOperationException(
                "Another expense account with this name already exists.");

        account.Name = name;

        account.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();

        account.IsActive = request.IsActive;
        account.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        var totalAmount = await _dbContext.Expenses
            .Where(x =>
                x.ClientId == clientId &&
                x.ExpenseAccountId == account.Id)
            .SumAsync(x => (decimal?)x.Amount) ?? 0;

        return new ExpenseAccountResponse
        {
            Id = account.Id,
            Name = account.Name,
            Description = account.Description,
            IsActive = account.IsActive,
            TotalAmount = totalAmount
        };
    }


    public async Task DeleteExpenseAccountAsync(
        int clientId,
        int id)
    {
        var account = await _dbContext.ExpenseAccounts
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.ClientId == clientId);

        if (account == null)
            throw new KeyNotFoundException(
                "Expense account not found.");

        var hasExpenses = await _dbContext.Expenses
            .AnyAsync(x =>
                x.ClientId == clientId &&
                x.ExpenseAccountId == id);

        if (hasExpenses)
        {
            // Preserve historical expenses.
            account.IsActive = false;
            account.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return;
        }

        _dbContext.ExpenseAccounts.Remove(account);

        await _dbContext.SaveChangesAsync();
    }


    // =========================================================
    // EXPENSES
    // =========================================================

    public async Task<List<ExpenseResponse>> GetExpensesAsync(
        int clientId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int? expenseAccountId = null)
    {
        var query = _dbContext.Expenses
            .AsNoTracking()
            .Include(x => x.ExpenseAccount)
            .Where(x => x.ClientId == clientId);

        if (fromDate.HasValue)
        {
            var from = fromDate.Value.Date;

            query = query.Where(x =>
                x.ExpenseDate >= from);
        }

        if (toDate.HasValue)
        {
            var to = toDate.Value.Date;

            query = query.Where(x =>
                x.ExpenseDate < to.AddDays(1));
        }

        if (expenseAccountId.HasValue)
        {
            query = query.Where(x =>
                x.ExpenseAccountId == expenseAccountId.Value);
        }

        return await query
            .OrderByDescending(x => x.ExpenseDate)
            .ThenByDescending(x => x.Id)
            .Select(x => new ExpenseResponse
            {
                Id = x.Id,
                ExpenseAccountId = x.ExpenseAccountId,
                ExpenseAccountName = x.ExpenseAccount != null
                    ? x.ExpenseAccount.Name
                    : string.Empty,
                ExpenseDate = x.ExpenseDate,
                Amount = x.Amount,
                Description = x.Description,
                PaymentMode = x.PaymentMode,
                ReferenceNumber = x.ReferenceNumber
            })
            .ToListAsync();
    }


    public async Task<ExpenseResponse> GetExpenseByIdAsync(
        int clientId,
        int id)
    {
        var expense = await _dbContext.Expenses
            .AsNoTracking()
            .Include(x => x.ExpenseAccount)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.ClientId == clientId);

        if (expense == null)
            throw new KeyNotFoundException(
                "Expense not found.");

        return MapExpense(expense);
    }


    public async Task<ExpenseResponse> CreateExpenseAsync(
        int clientId,
        CreateExpenseRequest request)
    {
        ValidateExpenseRequest(request);

        var account = await _dbContext.ExpenseAccounts
            .FirstOrDefaultAsync(x =>
                x.Id == request.ExpenseAccountId &&
                x.ClientId == clientId);

        if (account == null)
            throw new KeyNotFoundException(
                "Expense account not found.");

        if (!account.IsActive)
            throw new InvalidOperationException(
                "This expense account is inactive.");

        var expenseDate = DateTime.SpecifyKind(
            request.ExpenseDate.Date,
            DateTimeKind.Unspecified);

        var expense = new Expense
        {
            ClientId = clientId,
            ExpenseAccountId = account.Id,
            ExpenseDate = expenseDate,
            Amount = decimal.Round(
                request.Amount,
                2,
                MidpointRounding.AwayFromZero),
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim(),
            PaymentMode = request.PaymentMode.Trim(),
            ReferenceNumber = string.IsNullOrWhiteSpace(
                request.ReferenceNumber)
                ? null
                : request.ReferenceNumber.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Expenses.Add(expense);

        await _dbContext.SaveChangesAsync();

        expense.ExpenseAccount = account;

        return MapExpense(expense);
    }


    public async Task<ExpenseResponse> UpdateExpenseAsync(
        int clientId,
        int id,
        CreateExpenseRequest request)
    {
        ValidateExpenseRequest(request);

        var expense = await _dbContext.Expenses
            .Include(x => x.ExpenseAccount)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.ClientId == clientId);

        if (expense == null)
            throw new KeyNotFoundException(
                "Expense not found.");

        var account = await _dbContext.ExpenseAccounts
            .FirstOrDefaultAsync(x =>
                x.Id == request.ExpenseAccountId &&
                x.ClientId == clientId);

        if (account == null)
            throw new KeyNotFoundException(
                "Expense account not found.");

        if (!account.IsActive)
            throw new InvalidOperationException(
                "This expense account is inactive.");

        expense.ExpenseAccountId = account.Id;

        expense.ExpenseDate = DateTime.SpecifyKind(
            request.ExpenseDate.Date,
            DateTimeKind.Unspecified);

        expense.Amount = decimal.Round(
            request.Amount,
            2,
            MidpointRounding.AwayFromZero);

        expense.Description = string.IsNullOrWhiteSpace(
            request.Description)
            ? null
            : request.Description.Trim();

        expense.PaymentMode = request.PaymentMode.Trim();

        expense.ReferenceNumber = string.IsNullOrWhiteSpace(
            request.ReferenceNumber)
            ? null
            : request.ReferenceNumber.Trim();

        expense.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        expense.ExpenseAccount = account;

        return MapExpense(expense);
    }


    public async Task DeleteExpenseAsync(
        int clientId,
        int id)
    {
        var expense = await _dbContext.Expenses
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.ClientId == clientId);

        if (expense == null)
            throw new KeyNotFoundException(
                "Expense not found.");

        _dbContext.Expenses.Remove(expense);

        await _dbContext.SaveChangesAsync();
    }


    // =========================================================
    // SUMMARY
    // =========================================================

    public async Task<ExpenseSummaryResponse> GetExpenseSummaryAsync(
        int clientId,
        DateTime fromDate,
        DateTime toDate)
    {
        var from = DateTime.SpecifyKind(
            fromDate.Date,
            DateTimeKind.Unspecified);

        var to = DateTime.SpecifyKind(
            toDate.Date.AddDays(1),
            DateTimeKind.Unspecified);

        if (from >= to)
            throw new InvalidOperationException(
                "Invalid expense date range.");

        var accountSummary = await _dbContext.Expenses
            .AsNoTracking()
            .Include(x => x.ExpenseAccount)
            .Where(x =>
                x.ClientId == clientId &&
                x.ExpenseDate >= from &&
                x.ExpenseDate < to)
            .GroupBy(x => new
            {
                x.ExpenseAccountId,
                AccountName = x.ExpenseAccount!.Name
            })
            .Select(g => new ExpenseAccountSummaryResponse
            {
                ExpenseAccountId = g.Key.ExpenseAccountId,
                ExpenseAccountName = g.Key.AccountName,
                TotalAmount = g.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.TotalAmount)
            .ToListAsync();

        return new ExpenseSummaryResponse
        {
            TotalExpenses = accountSummary.Sum(x => x.TotalAmount),
            Accounts = accountSummary
        };
    }


    // =========================================================
    // VALIDATION
    // =========================================================

    private static void ValidateExpenseRequest(
        CreateExpenseRequest request)
    {
        if (request.ExpenseAccountId <= 0)
            throw new InvalidOperationException(
                "Please select an expense account.");

        if (request.ExpenseDate == default)
            throw new InvalidOperationException(
                "Expense date is required.");

        if (request.ExpenseDate.Date > DateTime.Today)
            throw new InvalidOperationException(
                "Expense date cannot be in the future.");

        if (request.Amount <= 0)
            throw new InvalidOperationException(
                "Expense amount must be greater than zero.");

        if (request.Amount > 9999999999.99m)
            throw new InvalidOperationException(
                "Expense amount is too large.");

        if (string.IsNullOrWhiteSpace(request.PaymentMode))
            throw new InvalidOperationException(
                "Payment mode is required.");

        if (request.PaymentMode.Trim().Length > 30)
            throw new InvalidOperationException(
                "Payment mode cannot exceed 30 characters.");
    }


    // =========================================================
    // MAPPING
    // =========================================================

    private static ExpenseResponse MapExpense(
        Expense expense)
    {
        return new ExpenseResponse
        {
            Id = expense.Id,
            ExpenseAccountId = expense.ExpenseAccountId,
            ExpenseAccountName = expense.ExpenseAccount?.Name
                ?? string.Empty,
            ExpenseDate = expense.ExpenseDate,
            Amount = expense.Amount,
            Description = expense.Description,
            PaymentMode = expense.PaymentMode,
            ReferenceNumber = expense.ReferenceNumber
        };
    }
}