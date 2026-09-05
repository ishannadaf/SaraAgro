using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace SaraAgro.Mobile.Services.Api.Expenses;

public sealed class ExpenseApiService
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

    public ExpenseApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // =========================================================
    // EXPENSE ACCOUNTS
    // =========================================================

    public async Task<List<ExpenseAccountResponse>> GetExpenseAccountsAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var url =
            $"api/Expenses/accounts?includeInactive={includeInactive.ToString().ToLowerInvariant()}";

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            url);

        await AddAuthorizationHeaderAsync(request);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content
                .ReadFromJsonAsync<List<ExpenseAccountResponse>>(
                    JsonOptions,
                    cancellationToken)
                ?? new List<ExpenseAccountResponse>();
        }

        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }


    public async Task<ExpenseAccountResponse> CreateExpenseAccountAsync(
        string name,
        string? description = null,
        CancellationToken cancellationToken = default)
    {
        var body = new CreateExpenseAccountRequest
        {
            Name = name?.Trim() ?? string.Empty,
            Description = string.IsNullOrWhiteSpace(description)
                ? null
                : description.Trim()
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "api/Expenses/accounts")
        {
            Content = JsonContent.Create(
                body,
                options: JsonOptions)
        };

        await AddAuthorizationHeaderAsync(request);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result =
                await response.Content
                    .ReadFromJsonAsync<ExpenseAccountResponse>(
                        JsonOptions,
                        cancellationToken);

            return result
                ?? throw new InvalidOperationException(
                    "The API returned an empty expense account response.");
        }

        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }


    public async Task<ExpenseAccountResponse> UpdateExpenseAccountAsync(
        int id,
        string name,
        string? description,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var body = new UpdateExpenseAccountRequest
        {
            Name = name?.Trim() ?? string.Empty,
            Description = string.IsNullOrWhiteSpace(description)
                ? null
                : description.Trim(),
            IsActive = isActive
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"api/Expenses/accounts/{id}")
        {
            Content = JsonContent.Create(
                body,
                options: JsonOptions)
        };

        await AddAuthorizationHeaderAsync(request);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result =
                await response.Content
                    .ReadFromJsonAsync<ExpenseAccountResponse>(
                        JsonOptions,
                        cancellationToken);

            return result
                ?? throw new InvalidOperationException(
                    "The API returned an empty expense account response.");
        }

        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }


    public async Task DeleteExpenseAccountAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"api/Expenses/accounts/{id}");

        await AddAuthorizationHeaderAsync(request);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        if (response.IsSuccessStatusCode)
            return;

        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }


    // =========================================================
    // EXPENSES
    // =========================================================

    public async Task<List<ExpenseResponse>> GetExpensesAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int? expenseAccountId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();

        if (fromDate.HasValue)
        {
            var dateText =
                fromDate.Value.Date.ToString(
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture);

            query.Add(
                $"fromDate={Uri.EscapeDataString(dateText)}");
        }

        if (toDate.HasValue)
        {
            var dateText =
                toDate.Value.Date.ToString(
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture);

            query.Add(
                $"toDate={Uri.EscapeDataString(dateText)}");
        }

        if (expenseAccountId.HasValue)
        {
            query.Add(
                $"expenseAccountId={expenseAccountId.Value}");
        }

        var url = "api/Expenses";

        if (query.Count > 0)
        {
            url += "?" + string.Join("&", query);
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            url);

        await AddAuthorizationHeaderAsync(request);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content
                .ReadFromJsonAsync<List<ExpenseResponse>>(
                    JsonOptions,
                    cancellationToken)
                ?? new List<ExpenseResponse>();
        }

        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }


    public async Task<ExpenseResponse?> GetExpenseAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/Expenses/{id}");

        await AddAuthorizationHeaderAsync(request);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content
                .ReadFromJsonAsync<ExpenseResponse>(
                    JsonOptions,
                    cancellationToken);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }


    public async Task<ExpenseResponse> CreateExpenseAsync(
        int expenseAccountId,
        DateTime expenseDate,
        decimal amount,
        string paymentMode,
        string? description = null,
        string? referenceNumber = null,
        CancellationToken cancellationToken = default)
    {
        var businessDate =
            DateTime.SpecifyKind(
                expenseDate.Date,
                DateTimeKind.Unspecified);

        var body = new CreateExpenseRequest
        {
            ExpenseAccountId = expenseAccountId,

            ExpenseDate = businessDate,

            Amount = decimal.Round(
                amount,
                2,
                MidpointRounding.AwayFromZero),

            Description = string.IsNullOrWhiteSpace(description)
                ? null
                : description.Trim(),

            PaymentMode = string.IsNullOrWhiteSpace(paymentMode)
                ? "Cash"
                : paymentMode.Trim(),

            ReferenceNumber = string.IsNullOrWhiteSpace(
                referenceNumber)
                ? null
                : referenceNumber.Trim()
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "api/Expenses")
        {
            Content = JsonContent.Create(
                body,
                options: JsonOptions)
        };

        await AddAuthorizationHeaderAsync(request);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result =
                await response.Content
                    .ReadFromJsonAsync<ExpenseResponse>(
                        JsonOptions,
                        cancellationToken);

            return result
                ?? throw new InvalidOperationException(
                    "The API returned an empty expense response.");
        }

        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }


    public async Task<ExpenseResponse> UpdateExpenseAsync(
        int id,
        int expenseAccountId,
        DateTime expenseDate,
        decimal amount,
        string paymentMode,
        string? description = null,
        string? referenceNumber = null,
        CancellationToken cancellationToken = default)
    {
        var businessDate =
            DateTime.SpecifyKind(
                expenseDate.Date,
                DateTimeKind.Unspecified);

        var body = new CreateExpenseRequest
        {
            ExpenseAccountId = expenseAccountId,

            ExpenseDate = businessDate,

            Amount = decimal.Round(
                amount,
                2,
                MidpointRounding.AwayFromZero),

            Description = string.IsNullOrWhiteSpace(description)
                ? null
                : description.Trim(),

            PaymentMode = string.IsNullOrWhiteSpace(paymentMode)
                ? "Cash"
                : paymentMode.Trim(),

            ReferenceNumber = string.IsNullOrWhiteSpace(
                referenceNumber)
                ? null
                : referenceNumber.Trim()
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"api/Expenses/{id}")
        {
            Content = JsonContent.Create(
                body,
                options: JsonOptions)
        };

        await AddAuthorizationHeaderAsync(request);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result =
                await response.Content
                    .ReadFromJsonAsync<ExpenseResponse>(
                        JsonOptions,
                        cancellationToken);

            return result
                ?? throw new InvalidOperationException(
                    "The API returned an empty expense response.");
        }

        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }


    public async Task DeleteExpenseAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"api/Expenses/{id}");

        await AddAuthorizationHeaderAsync(request);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        if (response.IsSuccessStatusCode)
            return;

        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }


    // =========================================================
    // SUMMARY
    // =========================================================

    public async Task<ExpenseSummaryResponse> GetExpenseSummaryAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        var fromText =
            fromDate.Date.ToString(
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture);

        var toText =
            toDate.Date.ToString(
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture);

        var url =
            $"api/Expenses/summary" +
            $"?fromDate={Uri.EscapeDataString(fromText)}" +
            $"&toDate={Uri.EscapeDataString(toText)}";

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            url);

        await AddAuthorizationHeaderAsync(request);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result =
                await response.Content
                    .ReadFromJsonAsync<ExpenseSummaryResponse>(
                        JsonOptions,
                        cancellationToken);

            return result
                ?? throw new InvalidOperationException(
                    "The API returned an empty expense summary.");
        }

        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }


    // =========================================================
    // AUTHORIZATION
    // =========================================================

    private static async Task AddAuthorizationHeaderAsync(
        HttpRequestMessage request)
    {
        var token =
            await SecureStorage.Default.GetAsync(
                "access_token");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException(
                "Access token is missing. Please login again.");
        }

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);
    }


    // =========================================================
    // API ERROR
    // =========================================================

    private static async Task<Exception> CreateApiExceptionAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.StatusCode ==
            HttpStatusCode.Unauthorized)
        {
            return new UnauthorizedAccessException(
                "The server rejected your access token (401 Unauthorized).");
        }

        var message =
            $"Expense API request failed " +
            $"({(int)response.StatusCode} {response.ReasonPhrase}).";

        try
        {
            var body =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!string.IsNullOrWhiteSpace(body))
            {
                try
                {
                    using var document =
                        JsonDocument.Parse(body);

                    var root =
                        document.RootElement;

                    if (root.TryGetProperty(
                            "message",
                            out var messageElement) &&
                        messageElement.ValueKind ==
                            JsonValueKind.String)
                    {
                        var apiMessage =
                            messageElement.GetString();

                        if (!string.IsNullOrWhiteSpace(
                                apiMessage))
                        {
                            message = apiMessage;
                        }
                    }
                    else if (root.TryGetProperty(
                                 "title",
                                 out var titleElement) &&
                             titleElement.ValueKind ==
                                 JsonValueKind.String)
                    {
                        var apiTitle =
                            titleElement.GetString();

                        if (!string.IsNullOrWhiteSpace(
                                apiTitle))
                        {
                            message = apiTitle;
                        }
                    }
                }
                catch (JsonException)
                {
                    // Keep default HTTP error.
                }
            }
        }
        catch
        {
            // Keep default HTTP error.
        }

        return response.StatusCode switch
        {
            HttpStatusCode.BadRequest =>
                new InvalidOperationException(message),

            HttpStatusCode.NotFound =>
                new InvalidOperationException(message),

            HttpStatusCode.Conflict =>
                new InvalidOperationException(message),

            _ =>
                new HttpRequestException(message)
        };
    }
}


// =============================================================
// REQUEST MODELS
// =============================================================

public sealed class CreateExpenseAccountRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}


public sealed class UpdateExpenseAccountRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }
}


public sealed class CreateExpenseRequest
{
    public int ExpenseAccountId { get; set; }

    public DateTime ExpenseDate { get; set; }

    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public string PaymentMode { get; set; } = "Cash";

    public string? ReferenceNumber { get; set; }
}


// =============================================================
// RESPONSE MODELS
// =============================================================

public sealed class ExpenseAccountResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public decimal TotalAmount { get; set; }
}


public sealed class ExpenseResponse
{
    public int Id { get; set; }

    public int ExpenseAccountId { get; set; }

    public string ExpenseAccountName { get; set; } = string.Empty;

    public DateTime ExpenseDate { get; set; }

    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public string PaymentMode { get; set; } = string.Empty;

    public string? ReferenceNumber { get; set; }
}


public sealed class ExpenseSummaryResponse
{
    public decimal TotalExpenses { get; set; }

    public List<ExpenseAccountSummaryResponse> Accounts { get; set; }
        = new();
}


public sealed class ExpenseAccountSummaryResponse
{
    public int ExpenseAccountId { get; set; }

    public string ExpenseAccountName { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }
}