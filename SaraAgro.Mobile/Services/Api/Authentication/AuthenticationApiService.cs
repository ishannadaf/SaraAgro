using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SaraAgro.Mobile.Services.Api.Authentication;

public sealed class AuthenticationApiService
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };


    public AuthenticationApiService(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    // =========================================================
    // LOGIN
    // =========================================================

    public async Task<LoginResponse?> LoginAsync(
        string mobileNumber,
        string password,
        CancellationToken cancellationToken = default)
    {
        var request = new LoginRequest
        {
            MobileNumber = mobileNumber.Trim(),
            Password = password
        };


        using var response =
            await _httpClient.PostAsJsonAsync(
                "api/Authentication/login",
                request,
                JsonOptions,
                cancellationToken);


        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<LoginResponse>(
                JsonOptions,
                cancellationToken);
        }


        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }


    // =========================================================
    // REGISTER
    // =========================================================

    public async Task<RegisterResponse?> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response =
            await _httpClient.PostAsJsonAsync(
                "api/Authentication/register",
                request,
                JsonOptions,
                cancellationToken);


        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<RegisterResponse>(
                JsonOptions,
                cancellationToken);
        }


        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }


    // =========================================================
    // API ERROR
    // =========================================================

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
                using var document =
                    JsonDocument.Parse(responseText);

                var root = document.RootElement;

                // ASP.NET Core validation response
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
                            string.Join(
                                Environment.NewLine,
                                messages));
                    }
                }

                // Normal API error
                if (root.TryGetProperty("message", out var message))
                {
                    var messageText = message.GetString();

                    if (!string.IsNullOrWhiteSpace(messageText))
                    {
                        return new InvalidOperationException(messageText);
                    }
                }

                if (root.TryGetProperty("title", out var title))
                {
                    var titleText = title.GetString();

                    if (!string.IsNullOrWhiteSpace(titleText))
                    {
                        return new InvalidOperationException(titleText);
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
                    "Unauthorized request."),

            HttpStatusCode.BadRequest =>
                new InvalidOperationException(
                    "Invalid registration request."),

            HttpStatusCode.Conflict =>
                new InvalidOperationException(
                    "The registration conflicts with an existing account."),

            _ =>
                new HttpRequestException(
                    $"Authentication request failed ({(int)response.StatusCode}).")
        };
    }
}


// =============================================================
// LOGIN REQUEST
// =============================================================

public sealed class LoginRequest
{
    [JsonPropertyName("mobileNumber")]
    public string MobileNumber { get; set; } = string.Empty;


    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}


// =============================================================
// LOGIN RESPONSE
// =============================================================

public sealed class LoginResponse
{
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; } = string.Empty;


    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; set; } = string.Empty;


    [JsonPropertyName("accessTokenExpiresAt")]
    public DateTime AccessTokenExpiresAt { get; set; }


    [JsonPropertyName("userId")]
    public int UserId { get; set; }


    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = string.Empty;


    [JsonPropertyName("mobileNumber")]
    public string MobileNumber { get; set; } = string.Empty;


    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;


    [JsonPropertyName("clientId")]
    public int ClientId { get; set; }


    [JsonPropertyName("clientName")]
    public string ClientName { get; set; } = string.Empty;
}


// =============================================================
// REGISTER REQUEST
// =============================================================

public sealed class RegisterRequest
{
    [JsonPropertyName("businessName")]
    public string BusinessName { get; set; } = string.Empty;


    [JsonPropertyName("ownerName")]
    public string OwnerName { get; set; } = string.Empty;


    [JsonPropertyName("mobileNumber")]
    public string MobileNumber { get; set; } = string.Empty;


    [JsonPropertyName("email")]
    public string? Email { get; set; }


    [JsonPropertyName("city")]
    public string? City { get; set; }


    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;


    [JsonPropertyName("otp")]
    public string Otp { get; set; } = string.Empty;
}


// =============================================================
// REGISTER RESPONSE
// =============================================================

public sealed class RegisterResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }


    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;


    [JsonPropertyName("userId")]
    public int UserId { get; set; }
}


// =============================================================
// API ERROR RESPONSE
// =============================================================

internal sealed class ApiErrorResponse
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }


    [JsonPropertyName("title")]
    public string? Title { get; set; }
}