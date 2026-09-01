using SaraAgro.Mobile.Services.Api.Authentication;

namespace SaraAgro.Mobile.Services.Authentication;

public sealed class AuthSessionService
{
    // =========================================================
    // SECURE STORAGE KEYS
    // =========================================================

    // IMPORTANT:
    // These keys are intentionally kept compatible with the
    // existing API services in the application.
    //
    // Existing services read:
    //     access_token
    //
    // Therefore AuthSessionService must use the same key.

    private const string AccessTokenKey =
        "access_token";

    private const string RefreshTokenKey =
        "refresh_token";

    private const string AccessTokenExpiryKey =
        "access_token_expires_at";

    private const string UserIdKey =
        "user_id";

    private const string FullNameKey =
        "full_name";

    private const string MobileNumberKey =
        "mobile_number";

    private const string RoleKey =
        "user_role";

    private const string ClientIdKey =
        "client_id";

    private const string ClientNameKey =
        "client_name";


    private readonly AuthenticationApiService
        _authenticationApiService;


    // =========================================================
    // CONSTRUCTOR
    // =========================================================

    public AuthSessionService(
        AuthenticationApiService authenticationApiService)
    {
        _authenticationApiService =
            authenticationApiService;
    }


    // =========================================================
    // SAVE LOGIN SESSION
    // =========================================================

    public async Task SaveSessionAsync(
        LoginResponse response)
    {
        if (response == null)
        {
            throw new ArgumentNullException(
                nameof(response));
        }


        if (string.IsNullOrWhiteSpace(
                response.AccessToken))
        {
            throw new InvalidOperationException(
                "Access token was not returned by the server.");
        }


        if (string.IsNullOrWhiteSpace(
                response.RefreshToken))
        {
            throw new InvalidOperationException(
                "Refresh token was not returned by the server.");
        }


        // -----------------------------------------------------
        // ACCESS TOKEN
        // -----------------------------------------------------

        await SecureStorage.Default.SetAsync(
            AccessTokenKey,
            response.AccessToken);


        // -----------------------------------------------------
        // REFRESH TOKEN
        // -----------------------------------------------------

        await SecureStorage.Default.SetAsync(
            RefreshTokenKey,
            response.RefreshToken);


        // -----------------------------------------------------
        // ACCESS TOKEN EXPIRY
        // -----------------------------------------------------

        await SecureStorage.Default.SetAsync(
            AccessTokenExpiryKey,
            response.AccessTokenExpiresAt
                .ToUniversalTime()
                .ToString("O"));


        // -----------------------------------------------------
        // USER
        // -----------------------------------------------------

        await SecureStorage.Default.SetAsync(
            UserIdKey,
            response.UserId.ToString());


        await SecureStorage.Default.SetAsync(
            FullNameKey,
            response.FullName ?? string.Empty);


        await SecureStorage.Default.SetAsync(
            MobileNumberKey,
            response.MobileNumber ?? string.Empty);


        await SecureStorage.Default.SetAsync(
            RoleKey,
            response.Role ?? string.Empty);


        // -----------------------------------------------------
        // CLIENT
        // -----------------------------------------------------

        await SecureStorage.Default.SetAsync(
            ClientIdKey,
            response.ClientId.ToString());


        await SecureStorage.Default.SetAsync(
            ClientNameKey,
            response.ClientName ?? string.Empty);
    }


    // =========================================================
    // GET ACCESS TOKEN
    // =========================================================

    public async Task<string?> GetAccessTokenAsync()
    {
        return await SecureStorage.Default.GetAsync(
            AccessTokenKey);
    }


    // =========================================================
    // GET REFRESH TOKEN
    // =========================================================

    public async Task<string?> GetRefreshTokenAsync()
    {
        return await SecureStorage.Default.GetAsync(
            RefreshTokenKey);
    }


    // =========================================================
    // GET ACCESS TOKEN EXPIRY
    // =========================================================

    public async Task<DateTime?> GetAccessTokenExpiryAsync()
    {
        var value =
            await SecureStorage.Default.GetAsync(
                AccessTokenExpiryKey);


        if (string.IsNullOrWhiteSpace(value))
            return null;


        if (DateTime.TryParse(
                value,
                null,
                System.Globalization.DateTimeStyles.RoundtripKind,
                out var expiry))
        {
            return expiry.ToUniversalTime();
        }


        return null;
    }


    // =========================================================
    // IS SESSION AVAILABLE
    // =========================================================

    public async Task<bool> HasSessionAsync()
    {
        var accessToken =
            await GetAccessTokenAsync();


        var refreshToken =
            await GetRefreshTokenAsync();


        return
            !string.IsNullOrWhiteSpace(accessToken) &&
            !string.IsNullOrWhiteSpace(refreshToken);
    }


    // =========================================================
    // ENSURE VALID SESSION
    // =========================================================

    public async Task<bool> EnsureValidSessionAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var accessToken =
                await GetAccessTokenAsync();


            var refreshToken =
                await GetRefreshTokenAsync();


            // -------------------------------------------------
            // NO SESSION
            // -------------------------------------------------

            if (string.IsNullOrWhiteSpace(
                    accessToken) ||
                string.IsNullOrWhiteSpace(
                    refreshToken))
            {
                return false;
            }


            // -------------------------------------------------
            // CHECK ACCESS TOKEN EXPIRY
            // -------------------------------------------------

            var expiry =
                await GetAccessTokenExpiryAsync();


            // -------------------------------------------------
            // ACCESS TOKEN STILL VALID
            // -------------------------------------------------

            if (expiry.HasValue &&
                expiry.Value >
                    DateTime.UtcNow.AddSeconds(30))
            {
                return true;
            }


            // -------------------------------------------------
            // ACCESS TOKEN EXPIRED
            // -------------------------------------------------
            //
            // Use the refresh token.
            //
            // The refresh token is valid for 7 days.
            //
            // -------------------------------------------------

            var refreshed =
                await _authenticationApiService
                    .RefreshAsync(
                        refreshToken,
                        cancellationToken);


            if (refreshed == null)
            {
                await ClearSessionAsync();

                return false;
            }


            // -------------------------------------------------
            // SAVE NEW TOKEN PAIR
            // -------------------------------------------------

            await SaveSessionAsync(
                refreshed);


            return true;
        }
        catch (UnauthorizedAccessException)
        {
            await ClearSessionAsync();

            return false;
        }
        catch (HttpRequestException)
        {
            // Do NOT automatically destroy the session if
            // the API is temporarily unreachable.
            return false;
        }
        catch
        {
            return false;
        }
    }


    // =========================================================
    // FORCE REFRESH
    // =========================================================

    public async Task<bool> RefreshSessionAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var refreshToken =
                await GetRefreshTokenAsync();


            if (string.IsNullOrWhiteSpace(
                    refreshToken))
            {
                await ClearSessionAsync();

                return false;
            }


            var response =
                await _authenticationApiService
                    .RefreshAsync(
                        refreshToken,
                        cancellationToken);


            if (response == null)
            {
                await ClearSessionAsync();

                return false;
            }


            await SaveSessionAsync(
                response);


            return true;
        }
        catch
        {
            await ClearSessionAsync();

            return false;
        }
    }


    // =========================================================
    // LOGOUT
    // =========================================================

    public async Task LogoutAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var refreshToken =
                await GetRefreshTokenAsync();


            if (!string.IsNullOrWhiteSpace(
                    refreshToken))
            {
                await _authenticationApiService
                    .LogoutAsync(
                        refreshToken,
                        cancellationToken);
            }
        }
        catch
        {
            // Local logout must still happen even if
            // the API cannot be reached.
        }
        finally
        {
            await ClearSessionAsync();
        }
    }


    // =========================================================
    // CLEAR LOCAL SESSION
    // =========================================================

    public Task ClearSessionAsync()
    {
        SecureStorage.Default.Remove(
            AccessTokenKey);

        SecureStorage.Default.Remove(
            RefreshTokenKey);

        SecureStorage.Default.Remove(
            AccessTokenExpiryKey);

        SecureStorage.Default.Remove(
            UserIdKey);

        SecureStorage.Default.Remove(
            FullNameKey);

        SecureStorage.Default.Remove(
            MobileNumberKey);

        SecureStorage.Default.Remove(
            RoleKey);

        SecureStorage.Default.Remove(
            ClientIdKey);

        SecureStorage.Default.Remove(
            ClientNameKey);


        return Task.CompletedTask;
    }


    // =========================================================
    // USER INFORMATION
    // =========================================================

    public async Task<int?> GetUserIdAsync()
    {
        var value =
            await SecureStorage.Default.GetAsync(
                UserIdKey);


        return int.TryParse(
            value,
            out var id)
            ? id
            : null;
    }


    public async Task<int?> GetClientIdAsync()
    {
        var value =
            await SecureStorage.Default.GetAsync(
                ClientIdKey);


        return int.TryParse(
            value,
            out var id)
            ? id
            : null;
    }


    public async Task<string?> GetFullNameAsync()
    {
        return await SecureStorage.Default.GetAsync(
            FullNameKey);
    }


    public async Task<string?> GetClientNameAsync()
    {
        return await SecureStorage.Default.GetAsync(
            ClientNameKey);
    }


    public async Task<string?> GetRoleAsync()
    {
        return await SecureStorage.Default.GetAsync(
            RoleKey);
    }


    public async Task<string?> GetMobileNumberAsync()
    {
        return await SecureStorage.Default.GetAsync(
            MobileNumberKey);
    }
}