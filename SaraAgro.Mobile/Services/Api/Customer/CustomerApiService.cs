using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SaraAgro.Mobile.Services.Api.Customer;

public sealed class CustomerApiService
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CustomerApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // =========================================================
    // GET ALL CUSTOMERS
    // =========================================================

    public async Task<List<CustomerResponse>> GetCustomersAsync(
        CancellationToken cancellationToken = default)
    {
        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                "api/Customer");

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
                .ReadFromJsonAsync<List<CustomerResponse>>(
                    JsonOptions,
                    cancellationToken)
                ?? new List<CustomerResponse>();
        }

        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }


    // =========================================================
    // GET CUSTOMER BY ID
    // =========================================================

    public async Task<CustomerResponse?> GetCustomerAsync(
        int customerId,
        CancellationToken cancellationToken = default)
    {
        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                $"api/Customer/{customerId}");

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
                .ReadFromJsonAsync<CustomerResponse>(
                    JsonOptions,
                    cancellationToken);
        }

        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }


    // =========================================================
    // CREATE CUSTOMER
    // =========================================================

    public async Task<int> CreateCustomerAsync(
        int rateGroupId,
        string customerCode,
        string fullName,
        string mobileNumber,
        string address,
        CancellationToken cancellationToken = default)
    {
        var requestBody =
            new CreateCustomerRequest
            {
                RateGroupId = rateGroupId,
                CustomerCode = customerCode.Trim(),
                FullName = fullName.Trim(),
                MobileNumber = mobileNumber.Trim(),
                Address = address.Trim()
            };

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "api/Customer")
            {
                Content =
                    JsonContent.Create(
                        requestBody,
                        options: JsonOptions)
            };

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
                    .ReadFromJsonAsync<CreateCustomerResponse>(
                        JsonOptions,
                        cancellationToken);

            if (result == null)
            {
                throw new InvalidOperationException(
                    "Invalid response received from the server.");
            }

            return result.CustomerId;
        }

        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }


    // =========================================================
    // UPDATE CUSTOMER
    // =========================================================

    public async Task<bool> UpdateCustomerAsync(
        int customerId,
        int rateGroupId,
        string fullName,
        string mobileNumber,
        string address,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var requestBody =
            new UpdateCustomerRequest
            {
                RateGroupId = rateGroupId,
                FullName = fullName.Trim(),
                MobileNumber = mobileNumber.Trim(),
                Address = address.Trim(),
                IsActive = isActive
            };

        using var request =
            new HttpRequestMessage(
                HttpMethod.Put,
                $"api/Customer/{customerId}")
            {
                Content =
                    JsonContent.Create(
                        requestBody,
                        options: JsonOptions)
            };

        await AddAuthorizationHeaderAsync(
            request,
            cancellationToken);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return true;
        }

        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }


    // =========================================================
    // UPDATE STATUS
    // =========================================================

    public async Task<bool> UpdateCustomerStatusAsync(
        int customerId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var requestBody =
            new UpdateCustomerStatusRequest
            {
                IsActive = isActive
            };

        using var request =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"api/Customer/{customerId}/status")
            {
                Content =
                    JsonContent.Create(
                        requestBody,
                        options: JsonOptions)
            };

        await AddAuthorizationHeaderAsync(
            request,
            cancellationToken);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return true;
        }

        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }


    // =========================================================
    // CUSTOMER ACCOUNT
    // =========================================================

    public async Task<CustomerAccountSummaryResponse?>
        GetCustomerAccountAsync(
            int customerId,
            DateTime fromDate,
            DateTime toDate,
            CancellationToken cancellationToken = default)
    {
        var url =
            $"api/Customer/{customerId}/account" +
            $"?fromDate={Uri.EscapeDataString(fromDate.Date.ToString("O"))}" +
            $"&toDate={Uri.EscapeDataString(toDate.Date.ToString("O"))}";

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
                .ReadFromJsonAsync<CustomerAccountSummaryResponse>(
                    JsonOptions,
                    cancellationToken);
        }

        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }


    // =========================================================
    // AUTHORIZATION
    // =========================================================

    private static async Task AddAuthorizationHeaderAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var accessToken =
            await SecureStorage.Default.GetAsync(
                "access_token");

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new UnauthorizedAccessException(
                "Access token is missing from SecureStorage.");
        }

        request.Headers.Remove("Authorization");

        request.Headers.TryAddWithoutValidation(
            "Authorization",
            $"Bearer {accessToken}");
    }


    // =========================================================
    // API ERROR
    // =========================================================

    private static async Task<Exception>
        CreateApiExceptionAsync(
            HttpResponseMessage response,
            CancellationToken cancellationToken)
    {
        var responseText =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        if (!string.IsNullOrWhiteSpace(responseText))
        {
            try
            {
                using var document =
                    JsonDocument.Parse(responseText);

                var root =
                    document.RootElement;

                if (root.TryGetProperty(
                        "errors",
                        out var errors))
                {
                    var messages =
                        new List<string>();

                    foreach (var property
                             in errors.EnumerateObject())
                    {
                        foreach (var error
                                 in property.Value.EnumerateArray())
                        {
                            if (error.ValueKind ==
                                JsonValueKind.String)
                            {
                                messages.Add(
                                    $"{property.Name}: {error.GetString()}");
                            }
                        }
                    }

                    if (messages.Count > 0)
                    {
                        return new InvalidOperationException(
                            string.Join(
                                Environment.NewLine,
                                messages));
                    }
                }

                if (root.TryGetProperty(
                        "message",
                        out var message))
                {
                    var messageText =
                        message.GetString();

                    if (!string.IsNullOrWhiteSpace(
                            messageText))
                    {
                        return new InvalidOperationException(
                            messageText);
                    }
                }

                if (root.TryGetProperty(
                        "title",
                        out var title))
                {
                    var titleText =
                        title.GetString();

                    if (!string.IsNullOrWhiteSpace(
                            titleText))
                    {
                        return new InvalidOperationException(
                            titleText);
                    }
                }
            }
            catch (JsonException)
            {
                // Response wasn't JSON.
            }
        }

        return response.StatusCode switch
        {
            HttpStatusCode.Unauthorized =>
                new UnauthorizedAccessException(
                    "API rejected the access token (401 Unauthorized)."),

            HttpStatusCode.BadRequest =>
                new InvalidOperationException(
                    "Invalid customer request."),

            HttpStatusCode.NotFound =>
                new InvalidOperationException(
                    "Customer not found."),

            HttpStatusCode.Conflict =>
                new InvalidOperationException(
                    "A customer with the same code or mobile number already exists."),

            _ =>
                new HttpRequestException(
                    $"Customer request failed ({(int)response.StatusCode}).")
        };
    }
}


// =============================================================
// CUSTOMER RESPONSE
// =============================================================

public sealed class CustomerResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("clientId")]
    public int ClientId { get; set; }

    [JsonPropertyName("rateGroupId")]
    public int RateGroupId { get; set; }

    [JsonPropertyName("rateGroupName")]
    public string RateGroupName { get; set; } =
        string.Empty;

    [JsonPropertyName("customerCode")]
    public string CustomerCode { get; set; } =
        string.Empty;

    [JsonPropertyName("fullName")]
    public string FullName { get; set; } =
        string.Empty;

    [JsonPropertyName("mobileNumber")]
    public string MobileNumber { get; set; } =
        string.Empty;

    [JsonPropertyName("address")]
    public string Address { get; set; } =
        string.Empty;

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
}


// =============================================================
// CREATE REQUEST
// =============================================================

public sealed class CreateCustomerRequest
{
    [JsonPropertyName("rateGroupId")]
    public int RateGroupId { get; set; }

    [JsonPropertyName("customerCode")]
    public string CustomerCode { get; set; } =
        string.Empty;

    [JsonPropertyName("fullName")]
    public string FullName { get; set; } =
        string.Empty;

    [JsonPropertyName("mobileNumber")]
    public string MobileNumber { get; set; } =
        string.Empty;

    [JsonPropertyName("address")]
    public string Address { get; set; } =
        string.Empty;
}


// =============================================================
// UPDATE REQUEST
// =============================================================

public sealed class UpdateCustomerRequest
{
    [JsonPropertyName("rateGroupId")]
    public int RateGroupId { get; set; }

    [JsonPropertyName("fullName")]
    public string FullName { get; set; } =
        string.Empty;

    [JsonPropertyName("mobileNumber")]
    public string MobileNumber { get; set; } =
        string.Empty;

    [JsonPropertyName("address")]
    public string Address { get; set; } =
        string.Empty;

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
}

// =============================================================
// STATUS REQUEST
// =============================================================

public sealed class UpdateCustomerStatusRequest
{
    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
}

// =============================================================
// CREATE RESPONSE
// =============================================================

public sealed class CreateCustomerResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("customerId")]
    public int CustomerId { get; set; }
}


// =============================================================
// ACCOUNT SUMMARY
// =============================================================

public sealed class CustomerAccountSummaryResponse
{
    [JsonPropertyName("customerId")]
    public int CustomerId { get; set; }

    [JsonPropertyName("customerCode")]
    public string CustomerCode { get; set; } =
        string.Empty;

    [JsonPropertyName("customerName")]
    public string CustomerName { get; set; } =
        string.Empty;

    [JsonPropertyName("rateGroupId")]
    public int RateGroupId { get; set; }

    [JsonPropertyName("rateGroupName")]
    public string RateGroupName { get; set; } =
        string.Empty;

    [JsonPropertyName("fromDate")]
    public DateTime FromDate { get; set; }

    [JsonPropertyName("toDate")]
    public DateTime ToDate { get; set; }

    [JsonPropertyName("morningQuantity")]
    public decimal MorningQuantity { get; set; }

    [JsonPropertyName("eveningQuantity")]
    public decimal EveningQuantity { get; set; }

    [JsonPropertyName("totalQuantity")]
    public decimal TotalQuantity { get; set; }

    [JsonPropertyName("totalAmount")]
    public decimal TotalAmount { get; set; }
}