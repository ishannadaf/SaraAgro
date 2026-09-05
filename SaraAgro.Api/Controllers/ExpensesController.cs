using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaraAgro.Api.DTOs.Expenses;
using SaraAgro.Api.Interfaces.Expenses;
using System.Security.Claims;

namespace SaraAgro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _expenseService;

    public ExpensesController(IExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    // =========================================================
    // CLIENT
    // =========================================================

    private int GetClientId()
    {
        var clientIdClaim = User.FindFirst("client_id")?.Value;

        if (string.IsNullOrWhiteSpace(clientIdClaim))
            throw new UnauthorizedAccessException(
                "Client information is missing from the token.");

        if (!int.TryParse(clientIdClaim, out var clientId) ||
            clientId <= 0)
            throw new UnauthorizedAccessException(
                "Invalid client information in token.");

        return clientId;
    }


    // =========================================================
    // EXPENSE ACCOUNTS
    // =========================================================

    /// <summary>
    /// Get expense accounts.
    /// </summary>
    [HttpGet("accounts")]
    public async Task<ActionResult<List<ExpenseAccountResponse>>> GetAccounts(
        [FromQuery] bool includeInactive = false)
    {
        try
        {
            var clientId = GetClientId();

            var result = await _expenseService
                .GetExpenseAccountsAsync(
                    clientId,
                    includeInactive);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new
            {
                message = ex.Message
            });
        }
    }


    /// <summary>
    /// Create a new expense account.
    /// </summary>
    [HttpPost("accounts")]
    public async Task<ActionResult<ExpenseAccountResponse>> CreateAccount(
        [FromBody] CreateExpenseAccountRequest request)
    {
        try
        {
            var clientId = GetClientId();

            var result = await _expenseService
                .CreateExpenseAccountAsync(
                    clientId,
                    request);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }


    /// <summary>
    /// Update an expense account.
    /// </summary>
    [HttpPut("accounts/{id:int}")]
    public async Task<ActionResult<ExpenseAccountResponse>> UpdateAccount(
        int id,
        [FromBody] UpdateExpenseAccountRequest request)
    {
        try
        {
            var clientId = GetClientId();

            var result = await _expenseService
                .UpdateExpenseAccountAsync(
                    clientId,
                    id,
                    request);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new
            {
                message = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }


    /// <summary>
    /// Delete or deactivate an expense account.
    /// </summary>
    [HttpDelete("accounts/{id:int}")]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        try
        {
            var clientId = GetClientId();

            await _expenseService
                .DeleteExpenseAccountAsync(
                    clientId,
                    id);

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new
            {
                message = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }


    // =========================================================
    // EXPENSES
    // =========================================================

    /// <summary>
    /// Get expenses with optional date/account filters.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ExpenseResponse>>> GetExpenses(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int? expenseAccountId = null)
    {
        try
        {
            var clientId = GetClientId();

            var result = await _expenseService
                .GetExpensesAsync(
                    clientId,
                    fromDate,
                    toDate,
                    expenseAccountId);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new
            {
                message = ex.Message
            });
        }
    }


    /// <summary>
    /// Get one expense.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExpenseResponse>> GetExpense(
        int id)
    {
        try
        {
            var clientId = GetClientId();

            var result = await _expenseService
                .GetExpenseByIdAsync(
                    clientId,
                    id);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new
            {
                message = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }


    /// <summary>
    /// Create a new expense.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ExpenseResponse>> CreateExpense(
        [FromBody] CreateExpenseRequest request)
    {
        try
        {
            var clientId = GetClientId();

            var result = await _expenseService
                .CreateExpenseAsync(
                    clientId,
                    request);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new
            {
                message = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }


    /// <summary>
    /// Update an existing expense.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ExpenseResponse>> UpdateExpense(
        int id,
        [FromBody] CreateExpenseRequest request)
    {
        try
        {
            var clientId = GetClientId();

            var result = await _expenseService
                .UpdateExpenseAsync(
                    clientId,
                    id,
                    request);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new
            {
                message = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }


    /// <summary>
    /// Delete an expense.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteExpense(
        int id)
    {
        try
        {
            var clientId = GetClientId();

            await _expenseService
                .DeleteExpenseAsync(
                    clientId,
                    id);

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new
            {
                message = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }


    // =========================================================
    // SUMMARY
    // =========================================================

    /// <summary>
    /// Get expense summary for a date range.
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<ExpenseSummaryResponse>> GetSummary(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        try
        {
            var clientId = GetClientId();

            var result = await _expenseService
                .GetExpenseSummaryAsync(
                    clientId,
                    fromDate,
                    toDate);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
}