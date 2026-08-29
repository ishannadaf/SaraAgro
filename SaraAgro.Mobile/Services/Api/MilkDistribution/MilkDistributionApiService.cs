using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SaraAgro.Mobile.Services.Api.MilkDistribution;

public sealed class MilkDistributionApiService
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public MilkDistributionApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<MilkDistributionResponse>> GetDistributionsAsync(
    DateTime? date = null,
    string? session = null,
    string? search = null,
    CancellationToken cancellationToken = default)
    {
        var query = new List<string>();

        if (date.HasValue)
        {
            var dateText =
                date.Value.Date.ToString(
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture);

            query.Add(
                $"date={Uri.EscapeDataString(dateText)}");
        }

        if (!string.IsNullOrWhiteSpace(session))
        {
            query.Add(
                $"session={Uri.EscapeDataString(
                    session.Trim().ToUpperInvariant())}");
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query.Add(
                $"search={Uri.EscapeDataString(
                    search.Trim())}");
        }

        var url = "api/MilkDistribution";

        if (query.Count > 0)
        {
            url += "?" +
                   string.Join("&", query);
        }

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                url);

        await AddAuthorizationHeaderAsync(
            request);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content
                .ReadFromJsonAsync<
                    List<MilkDistributionResponse>>(
                    JsonOptions,
                    cancellationToken)
                ?? new List<MilkDistributionResponse>();
        }

        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }

    public async Task<MilkDistributionResponse> CreateDistributionAsync(
        int customerId,
        DateTime distributionDate,
        string session,
        string milkType,
        decimal quantity,
        CancellationToken cancellationToken = default)
    {
        var body = new CreateMilkDistributionRequest
        {
            CustomerId = customerId,
            DistributionDate = distributionDate.Date,
            Session = session.Trim().ToUpperInvariant(),
            MilkType = milkType.Trim(),
            Quantity = decimal.Round(quantity, 2)
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "api/MilkDistribution")
        {
            Content = JsonContent.Create(body, options: JsonOptions)
        };

        await AddAuthorizationHeaderAsync(request);

        using var response =
            await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result =
                await response.Content.ReadFromJsonAsync<
                    MilkDistributionResponse>(
                    JsonOptions,
                    cancellationToken);

            return result
                ?? throw new InvalidOperationException(
                    "Invalid response received from the server.");
        }

        throw await CreateApiExceptionAsync(response, cancellationToken);
    }

    public async Task<MilkDistributionResponse?> UpdateDistributionAsync(
        int distributionId,
        decimal quantity,
        CancellationToken cancellationToken = default)
    {
        var body = new UpdateMilkDistributionRequest
        {
            Quantity = decimal.Round(quantity, 2)
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"api/MilkDistribution/{distributionId}")
        {
            Content = JsonContent.Create(body, options: JsonOptions)
        };

        await AddAuthorizationHeaderAsync(request);

        using var response =
            await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<
                MilkDistributionResponse>(
                JsonOptions,
                cancellationToken);
        }

        throw await CreateApiExceptionAsync(response, cancellationToken);
    }

    public async Task<List<MilkDistributionResponse>>
        GetCustomerDistributionsAsync(
            int customerId,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default)
    {
        var query = new List<string>();

        if (fromDate.HasValue)
            query.Add($"fromDate={Uri.EscapeDataString(fromDate.Value.Date.ToString("O"))}");

        if (toDate.HasValue)
            query.Add($"toDate={Uri.EscapeDataString(toDate.Value.Date.ToString("O"))}");

        var url = $"api/MilkDistribution/customer/{customerId}";

        if (query.Count > 0)
            url += "?" + string.Join("&", query);

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        await AddAuthorizationHeaderAsync(request);

        using var response =
            await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<
                List<MilkDistributionResponse>>(
                JsonOptions,
                cancellationToken) ?? new();
        }

        throw await CreateApiExceptionAsync(response, cancellationToken);
    }

    public async Task<MilkDistributionSummaryResponse>
    GetDailySummaryAsync(
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var dateText =
            date.Date.ToString(
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture);

        var url =
            $"api/MilkDistribution/summary" +
            $"?date={Uri.EscapeDataString(dateText)}";

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                url);

        await AddAuthorizationHeaderAsync(
            request);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content
                .ReadFromJsonAsync<
                    MilkDistributionSummaryResponse>(
                    JsonOptions,
                    cancellationToken)
                ?? throw new InvalidOperationException(
                    "Invalid summary response received from the server.");
        }

        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }

    public async Task<MilkDistributionMonthlySummaryResponse>
    GetMonthlySummaryAsync(
        DateTime month,
        CancellationToken cancellationToken = default)
    {
        var monthStart =
            new DateTime(
                month.Year,
                month.Month,
                1);

        var url =
            $"api/MilkDistribution/summary/month" +
            $"?month={Uri.EscapeDataString(monthStart.ToString("O"))}";

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                url);

        await AddAuthorizationHeaderAsync(request);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<
                MilkDistributionMonthlySummaryResponse>(
                    JsonOptions,
                    cancellationToken)
                ?? throw new InvalidOperationException(
                    "Invalid monthly summary response received from the server.");
        }

        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }

    private static async Task AddAuthorizationHeaderAsync(
        HttpRequestMessage request)
    {
        var token =
            await SecureStorage.Default.GetAsync("access_token");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException(
                "Access token is missing. Please login again.");
        }

        request.Headers.Remove("Authorization");
        request.Headers.TryAddWithoutValidation(
            "Authorization",
            $"Bearer {token}");
    }

    private static async Task<Exception> CreateApiExceptionAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var responseText =
            await response.Content.ReadAsStringAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(responseText))
        {
            try
            {
                using var document = JsonDocument.Parse(responseText);
                var root = document.RootElement;

                if (root.TryGetProperty("errors", out var errors))
                {
                    var messages = new List<string>();

                    foreach (var property in errors.EnumerateObject())
                    {
                        foreach (var error in property.Value.EnumerateArray())
                        {
                            if (error.ValueKind == JsonValueKind.String)
                            {
                                messages.Add(
                                    $"{property.Name}: {error.GetString()}");
                            }
                        }
                    }

                    if (messages.Count > 0)
                    {
                        return new InvalidOperationException(
                            string.Join(Environment.NewLine, messages));
                    }
                }

                if (root.TryGetProperty("message", out var message) &&
                    message.ValueKind == JsonValueKind.String)
                {
                    var text = message.GetString();

                    if (!string.IsNullOrWhiteSpace(text))
                        return new InvalidOperationException(text);
                }

                if (root.TryGetProperty("title", out var title) &&
                    title.ValueKind == JsonValueKind.String)
                {
                    var text = title.GetString();

                    if (!string.IsNullOrWhiteSpace(text))
                        return new InvalidOperationException(text);
                }
            }
            catch (JsonException)
            {
                // Non-JSON response.
            }
        }

        return response.StatusCode switch
        {
            HttpStatusCode.Unauthorized =>
                new UnauthorizedAccessException(
                    "API rejected the access token (401 Unauthorized). Please login again."),

            HttpStatusCode.BadRequest =>
                new InvalidOperationException(
                    "Invalid milk distribution request."),

            HttpStatusCode.NotFound =>
                new InvalidOperationException(
                    "Milk distribution was not found."),

            HttpStatusCode.Conflict =>
                new InvalidOperationException(
                    "Milk distribution already exists for this customer, date and session."),

            _ =>
                new HttpRequestException(
                    $"Milk distribution request failed ({(int)response.StatusCode}).")
        };
    }
}

public sealed class CreateMilkDistributionRequest
{
    [JsonPropertyName("customerId")]
    public int CustomerId { get; set; }

    [JsonPropertyName("distributionDate")]
    public DateTime DistributionDate { get; set; }

    [JsonPropertyName("session")]
    public string Session { get; set; } = string.Empty;

    [JsonPropertyName("milkType")]
    public string MilkType { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; }
}

public sealed class UpdateMilkDistributionRequest
{
    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; }
}

public sealed class MilkDistributionResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("customerId")]
    public int CustomerId { get; set; }

    [JsonPropertyName("customerCode")]
    public string CustomerCode { get; set; } = string.Empty;

    [JsonPropertyName("customerName")]
    public string CustomerName { get; set; } = string.Empty;

    [JsonPropertyName("distributionDate")]
    public DateTime DistributionDate { get; set; }

    [JsonPropertyName("session")]
    public string Session { get; set; } = string.Empty;

    [JsonPropertyName("milkType")]
    public string MilkType { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; }

    [JsonPropertyName("rate")]
    public decimal Rate { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("rateMasterId")]
    public int RateMasterId { get; set; }

    [JsonPropertyName("rateGroupId")]
    public int RateGroupId { get; set; }

    [JsonPropertyName("rateGroupName")]
    public string RateGroupName { get; set; } = string.Empty;
}

public sealed class MilkDistributionSummaryResponse
{
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("morningCowQuantity")]
    public decimal MorningCowQuantity { get; set; }

    [JsonPropertyName("morningBuffaloQuantity")]
    public decimal MorningBuffaloQuantity { get; set; }

    [JsonPropertyName("eveningCowQuantity")]
    public decimal EveningCowQuantity { get; set; }

    [JsonPropertyName("eveningBuffaloQuantity")]
    public decimal EveningBuffaloQuantity { get; set; }

    [JsonPropertyName("morningTotalQuantity")]
    public decimal MorningTotalQuantity { get; set; }

    [JsonPropertyName("eveningTotalQuantity")]
    public decimal EveningTotalQuantity { get; set; }

    [JsonPropertyName("totalQuantity")]
    public decimal TotalQuantity { get; set; }

    [JsonPropertyName("morningAmount")]
    public decimal MorningAmount { get; set; }

    [JsonPropertyName("eveningAmount")]
    public decimal EveningAmount { get; set; }

    [JsonPropertyName("totalAmount")]
    public decimal TotalAmount { get; set; }
}

public sealed class MilkDistributionMonthlySummaryResponse
{
    [JsonPropertyName("month")]
    public DateTime Month { get; set; }

    [JsonPropertyName("activeCustomerCount")]
    public int ActiveCustomerCount { get; set; }

    [JsonPropertyName("cowQuantity")]
    public decimal CowQuantity { get; set; }

    [JsonPropertyName("buffaloQuantity")]
    public decimal BuffaloQuantity { get; set; }

    [JsonPropertyName("totalQuantity")]
    public decimal TotalQuantity { get; set; }

    [JsonPropertyName("totalAmount")]
    public decimal TotalAmount { get; set; }

    [JsonPropertyName("daysRecorded")]
    public int DaysRecorded { get; set; }
}