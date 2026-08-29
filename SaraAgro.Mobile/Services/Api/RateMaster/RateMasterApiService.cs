using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SaraAgro.Mobile.Services.Api.RateMaster;

public sealed class RateMasterApiService
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public RateMasterApiService(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    // =========================================================
    // GET ALL RATES
    // =========================================================

    public async Task<List<RateMasterResponse>> GetRatesAsync(
        CancellationToken cancellationToken = default)
    {
        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                "api/RateMaster");

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
                .ReadFromJsonAsync<List<RateMasterResponse>>(
                    JsonOptions,
                    cancellationToken)
                ?? new List<RateMasterResponse>();
        }

        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }


    // =========================================================
    // GET APPLICABLE RATE
    // =========================================================

    public async Task<ApplicableRateResponse?> GetApplicableRateAsync(
        int rateGroupId,
        string milkType,
        DateTime distributionDate,
        CancellationToken cancellationToken = default)
    {
        if (rateGroupId <= 0)
        {
            throw new InvalidOperationException(
                "Customer does not have a valid rate group.");
        }

        if (string.IsNullOrWhiteSpace(milkType))
        {
            throw new ArgumentException(
                "Milk type is required.");
        }

        var normalizedMilkType =
            milkType.Trim();

        var url =
            "api/RateMaster/applicable" +
            $"?rateGroupId={rateGroupId}" +
            $"&milkType={Uri.EscapeDataString(normalizedMilkType)}" +
            $"&distributionDate={Uri.EscapeDataString(
                distributionDate.Date.ToString("O"))}";

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
            var result =
                await response.Content
                    .ReadFromJsonAsync<ApplicableRateResponse>(
                        JsonOptions,
                        cancellationToken);

            return result;
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
    // CREATE RATE
    // =========================================================

    public async Task<int> CreateRateAsync(
        int rateGroupId,
        string milkType,
        decimal rate,
        DateTime effectiveDate,
        CancellationToken cancellationToken = default)
    {
        var requestBody =
            new CreateRateMasterRequest
            {
                RateGroupId =
                    rateGroupId,

                MilkType =
                    milkType,

                Rate =
                    rate,

                EffectiveDate =
                    effectiveDate.Date
            };

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "api/RateMaster")
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
                    .ReadFromJsonAsync<CreateRateMasterResponse>(
                        JsonOptions,
                        cancellationToken);

            if (result == null)
            {
                throw new InvalidOperationException(
                    "Invalid response received from the server.");
            }

            return result.RateId;
        }

        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }


    // =========================================================
    // UPDATE RATE
    // =========================================================

    public async Task<bool> UpdateRateAsync(
        int rateMasterId,
        int rateGroupId,
        string milkType,
        decimal rate,
        DateTime effectiveDate,
        CancellationToken cancellationToken = default)
    {
        var requestBody =
            new UpdateRateMasterRequest
            {
                RateGroupId =
                    rateGroupId,

                MilkType =
                    milkType,

                Rate =
                    rate,

                EffectiveDate =
                    effectiveDate.Date
            };

        using var request =
            new HttpRequestMessage(
                HttpMethod.Put,
                $"api/RateMaster/{rateMasterId}")
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
    // DELETE RATE
    // =========================================================

    public async Task<bool> DeleteRateAsync(
        int rateMasterId,
        CancellationToken cancellationToken = default)
    {
        using var request =
            new HttpRequestMessage(
                HttpMethod.Delete,
                $"api/RateMaster/{rateMasterId}");

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

        request.Headers.Remove(
            "Authorization");

        request.Headers.TryAddWithoutValidation(
            "Authorization",
            $"Bearer {accessToken}");
    }


    // =========================================================
    // API ERROR
    // =========================================================

    private static async Task<Exception> CreateApiExceptionAsync(
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
                // Response was not JSON.
            }
        }


        return response.StatusCode switch
        {
            HttpStatusCode.Unauthorized =>
                new UnauthorizedAccessException(
                    "API rejected the access token (401 Unauthorized)."),

            HttpStatusCode.BadRequest =>
                new InvalidOperationException(
                    "Invalid rate request."),

            HttpStatusCode.NotFound =>
                new InvalidOperationException(
                    "Rate not found for the selected rate group, milk type and date."),

            HttpStatusCode.Conflict =>
                new InvalidOperationException(
                    "A rate already exists for this rate group, milk type and effective date."),

            _ =>
                new HttpRequestException(
                    $"Rate request failed ({(int)response.StatusCode}).")
        };
    }
}


// =============================================================
// RATE RESPONSE
// =============================================================

public sealed class RateMasterResponse
{
    [JsonPropertyName("rateMasterId")]
    public int RateMasterId { get; set; }

    [JsonPropertyName("rateGroupId")]
    public int RateGroupId { get; set; }

    [JsonPropertyName("rateGroupName")]
    public string RateGroupName { get; set; } =
        string.Empty;

    [JsonPropertyName("milkType")]
    public string MilkType { get; set; } =
        string.Empty;

    [JsonPropertyName("rate")]
    public decimal Rate { get; set; }

    [JsonPropertyName("effectiveDate")]
    public DateTime EffectiveDate { get; set; }
}


// =============================================================
// APPLICABLE RATE RESPONSE
// =============================================================

public sealed class ApplicableRateResponse
{
    [JsonPropertyName("rateMasterId")]
    public int RateMasterId { get; set; }

    [JsonPropertyName("rateGroupId")]
    public int RateGroupId { get; set; }

    [JsonPropertyName("rateGroupName")]
    public string RateGroupName { get; set; } =
        string.Empty;

    [JsonPropertyName("milkType")]
    public string MilkType { get; set; } =
        string.Empty;

    [JsonPropertyName("rate")]
    public decimal Rate { get; set; }

    // IMPORTANT:
    // Backend returns "effectiveDate".
    [JsonPropertyName("effectiveDate")]
    public DateTime EffectiveDate { get; set; }
}


// =============================================================
// CREATE REQUEST
// =============================================================

public sealed class CreateRateMasterRequest
{
    [JsonPropertyName("rateGroupId")]
    public int RateGroupId { get; set; }

    [JsonPropertyName("milkType")]
    public string MilkType { get; set; } =
        string.Empty;

    [JsonPropertyName("rate")]
    public decimal Rate { get; set; }

    [JsonPropertyName("effectiveDate")]
    public DateTime EffectiveDate { get; set; }
}


// =============================================================
// UPDATE REQUEST
// =============================================================

public sealed class UpdateRateMasterRequest
{
    [JsonPropertyName("rateGroupId")]
    public int RateGroupId { get; set; }

    [JsonPropertyName("milkType")]
    public string MilkType { get; set; } =
        string.Empty;

    [JsonPropertyName("rate")]
    public decimal Rate { get; set; }

    [JsonPropertyName("effectiveDate")]
    public DateTime EffectiveDate { get; set; }
}


// =============================================================
// CREATE RESPONSE
// =============================================================

public sealed class CreateRateMasterResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("rateId")]
    public int RateId { get; set; }
}