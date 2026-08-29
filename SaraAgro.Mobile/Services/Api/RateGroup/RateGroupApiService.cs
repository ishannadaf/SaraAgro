using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SaraAgro.Mobile.Services.Api.RateGroup;

public sealed class RateGroupApiService
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };


    public RateGroupApiService(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    // =========================================================
    // GET RATE GROUPS
    // =========================================================

    public async Task<List<RateGroupResponse>> GetRateGroupsAsync(
        CancellationToken cancellationToken = default)
    {
        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                "api/RateGroup");

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
                .ReadFromJsonAsync<List<RateGroupResponse>>(
                    JsonOptions,
                    cancellationToken)
                ?? new List<RateGroupResponse>();
        }

        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }


    // =========================================================
    // CREATE RATE GROUP
    // =========================================================

    public async Task<int> CreateRateGroupAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        var requestBody =
            new CreateRateGroupRequest
            {
                Name = name.Trim()
            };


        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "api/RateGroup")
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
                    .ReadFromJsonAsync<CreateRateGroupResponse>(
                        JsonOptions,
                        cancellationToken);

            if (result == null)
            {
                throw new InvalidOperationException(
                    "Invalid response received from the server.");
            }

            return result.RateGroupId;
        }


        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }


    // =========================================================
    // UPDATE STATUS
    // =========================================================

    public async Task UpdateRateGroupStatusAsync(
        int rateGroupId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        using var request =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"api/RateGroup/{rateGroupId}/status")
            {
                Content =
                    JsonContent.Create(
                        isActive,
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
            return;
        }


        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }
    // =========================================================
    // UPDATE RATE GROUP
    // =========================================================

    public async Task<bool> UpdateRateGroupAsync(
        int rateGroupId,
        string name,
        CancellationToken cancellationToken = default)
    {
        var requestBody = new UpdateRateGroupRequest
        {
            Name = name.Trim()
        };

        using var request =
            new HttpRequestMessage(
                HttpMethod.Put,
                $"api/RateGroup/{rateGroupId}");

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

        request.Headers.Remove("Authorization");

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


                // ASP.NET validation errors
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
                    "Invalid rate group request."),

            HttpStatusCode.NotFound =>
                new InvalidOperationException(
                    "Rate group not found."),

            HttpStatusCode.Conflict =>
                new InvalidOperationException(
                    "A rate group with this name already exists."),

            _ =>
                new HttpRequestException(
                    $"Rate group request failed ({(int)response.StatusCode}).")
        };
    }
}


// =============================================================
// RESPONSE
// =============================================================

public sealed class RateGroupResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }


    [JsonPropertyName("clientId")]
    public int ClientId { get; set; }


    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;


    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }


    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}


// =============================================================
// CREATE REQUEST
// =============================================================

public sealed class CreateRateGroupRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}


// =============================================================
// CREATE RESPONSE
// =============================================================

public sealed class CreateRateGroupResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }


    [JsonPropertyName("rateGroupId")]
    public int RateGroupId { get; set; }
}

public sealed class UpdateRateGroupRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}