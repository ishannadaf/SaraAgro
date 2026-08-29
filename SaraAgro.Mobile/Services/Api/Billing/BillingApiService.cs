using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace SaraAgro.Mobile.Services.Api.Billing;

public class BillingApiService
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

    public BillingApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // =========================================================
    // GET BILLS
    // =========================================================

    public async Task<List<BillResponse>> GetBillsAsync(
        int? customerId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new List<string>();

        if (customerId.HasValue)
        {
            parameters.Add(
                $"customerId={customerId.Value}");
        }

        if (fromDate.HasValue)
        {
            parameters.Add(
                $"fromDate={Uri.EscapeDataString(
                    fromDate.Value.Date.ToString("O"))}");
        }

        if (toDate.HasValue)
        {
            parameters.Add(
                $"toDate={Uri.EscapeDataString(
                    toDate.Value.Date.ToString("O"))}");
        }

        var url =
            "api/Billing";

        if (parameters.Count > 0)
        {
            url += "?" +
                   string.Join("&", parameters);
        }

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                url);

        await AddAuthorizationHeaderAsync(
            request,
            cancellationToken);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content
                .ReadFromJsonAsync<List<BillResponse>>(
                    JsonOptions,
                    cancellationToken)
                ?? new List<BillResponse>();
        }

        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }

    // =========================================================
    // GET BILL BY ID
    // =========================================================

    public async Task<BillResponse?> GetBillAsync(
        int billId,
        CancellationToken cancellationToken = default)
    {
        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                $"api/Billing/{billId}");

        await AddAuthorizationHeaderAsync(
            request,
            cancellationToken);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content
                .ReadFromJsonAsync<BillResponse>(
                    JsonOptions,
                    cancellationToken);
        }

        if (response.StatusCode ==
            HttpStatusCode.NotFound)
        {
            return null;
        }

        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }

    // =========================================================
    // GENERATE BILL
    // =========================================================

    public async Task<BillResponse> GenerateBillAsync(
        int customerId,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        var requestBody =
            new GenerateBillRequest
            {
                CustomerId = customerId,

                FromDate = fromDate.Date,

                ToDate = toDate.Date
            };

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "api/Billing/generate");

        request.Content =
            JsonContent.Create(
                requestBody,
                options: JsonOptions);

        await AddAuthorizationHeaderAsync(
            request,
            cancellationToken);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result =
                await response.Content
                    .ReadFromJsonAsync<BillResponse>(
                        JsonOptions,
                        cancellationToken);

            if (result == null)
            {
                throw new InvalidOperationException(
                    "The API returned an empty bill response.");
            }

            return result;
        }

        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }

    // =========================================================
    // ADD PAYMENT
    // =========================================================

    public async Task<BillPaymentResponse> AddPaymentAsync(
        int billId,
        DateTime paymentDate,
        decimal amount,
        string paymentMode,
        string referenceNumber,
        string notes,
        CancellationToken cancellationToken = default)
    {
        var requestBody =
            new AddBillPaymentRequest
            {
                PaymentDate =
                    paymentDate.Date,

                Amount =
                    amount,

                PaymentMode =
                    paymentMode?.Trim()
                    ?? "Cash",

                ReferenceNumber =
                    referenceNumber?.Trim()
                    ?? string.Empty,

                Notes =
                    notes?.Trim()
                    ?? string.Empty
            };

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                $"api/Billing/{billId}/payments");

        request.Content =
            JsonContent.Create(
                requestBody,
                options: JsonOptions);

        await AddAuthorizationHeaderAsync(
            request,
            cancellationToken);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result =
                await response.Content
                    .ReadFromJsonAsync<BillPaymentResponse>(
                        JsonOptions,
                        cancellationToken);

            if (result == null)
            {
                throw new InvalidOperationException(
                    "The API returned an empty payment response.");
            }

            return result;
        }

        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }

    // =========================================================
    // GET PAYMENT HISTORY
    // =========================================================

    public async Task<List<BillPaymentResponse>> GetPaymentsAsync(
        int billId,
        CancellationToken cancellationToken = default)
    {
        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                $"api/Billing/{billId}/payments");

        await AddAuthorizationHeaderAsync(
            request,
            cancellationToken);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content
                .ReadFromJsonAsync<List<BillPaymentResponse>>(
                    JsonOptions,
                    cancellationToken)
                ?? new List<BillPaymentResponse>();
        }

        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }

    // =========================================================
    // AUTHORIZATION
    // =========================================================

    private async Task AddAuthorizationHeaderAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token =
            await SecureStorage.Default.GetAsync(
                "access_token");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException(
                "Your session has expired. Please login again.");
        }

        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
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
                "Your session has expired. Please login again.");
        }

        var message =
            $"API request failed ({(int)response.StatusCode} {response.ReasonPhrase}).";

        try
        {
            var body =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!string.IsNullOrWhiteSpace(body))
            {
                try
                {
                    var error =
                        JsonSerializer.Deserialize<ApiErrorResponse>(
                            body,
                            JsonOptions);

                    if (!string.IsNullOrWhiteSpace(
                            error?.Message))
                    {
                        message =
                            error.Message;
                    }
                }
                catch
                {
                    // Keep the HTTP error message.
                }
            }
        }
        catch
        {
            // Keep the HTTP error message.
        }

        return new HttpRequestException(
            message,
            null,
            response.StatusCode);
    }
}


// =============================================================
// REQUEST MODELS
// =============================================================

public sealed class GenerateBillRequest
{
    public int CustomerId { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }
}


public sealed class AddBillPaymentRequest
{
    public DateTime PaymentDate { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMode { get; set; } =
        "Cash";

    public string ReferenceNumber { get; set; } =
        string.Empty;

    public string Notes { get; set; } =
        string.Empty;
}


// =============================================================
// RESPONSE MODELS
// =============================================================

public sealed class BillResponse
{
    public int Id { get; set; }

    public string BillNumber { get; set; } =
        string.Empty;

    public int CustomerId { get; set; }

    public string CustomerCode { get; set; } =
        string.Empty;

    public string CustomerName { get; set; } =
        string.Empty;

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public decimal MilkAmount { get; set; }

    public decimal PreviousOutstanding { get; set; }

    public decimal TotalPayable { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal BalanceAmount { get; set; }

    public string Status { get; set; } =
        string.Empty;

    public DateTime CreatedAt { get; set; }

    public List<BillPaymentResponse> Payments { get; set; } =
        new();
}


public sealed class BillPaymentResponse
{
    public int Id { get; set; }

    public int BillId { get; set; }

    public DateTime PaymentDate { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMode { get; set; } =
        string.Empty;

    public string ReferenceNumber { get; set; } =
        string.Empty;

    public string Notes { get; set; } =
        string.Empty;

    public DateTime CreatedAt { get; set; }
}


public sealed class ApiErrorResponse
{
    public string? Message { get; set; }
}