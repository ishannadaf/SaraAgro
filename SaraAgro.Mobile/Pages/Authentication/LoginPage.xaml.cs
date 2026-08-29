using SaraAgro.Mobile.Services.Api.Authentication;

namespace SaraAgro.Mobile.Pages.Authentication;

public partial class LoginPage : ContentPage
{
    private readonly AuthenticationApiService _authenticationApiService;

    private bool _isPasswordVisible;
    private bool _isLoggingIn;


    public LoginPage(
        AuthenticationApiService authenticationApiService)
    {
        InitializeComponent();

        _authenticationApiService =
            authenticationApiService;

        MobileNumberEntry.TextChanged +=
            MobileNumberEntry_TextChanged;
    }


    // =========================================================
    // MOBILE NUMBER
    // =========================================================

    private void MobileNumberEntry_TextChanged(
        object? sender,
        TextChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.NewTextValue))
            return;

        var digitsOnly =
            new string(
                e.NewTextValue
                    .Where(char.IsDigit)
                    .Take(10)
                    .ToArray());

        if (MobileNumberEntry.Text != digitsOnly)
        {
            MobileNumberEntry.Text =
                digitsOnly;
        }
    }


    // =========================================================
    // PASSWORD VISIBILITY
    // =========================================================

    private void PasswordVisibilityClicked(
        object sender,
        EventArgs e)
    {
        _isPasswordVisible =
            !_isPasswordVisible;

        PasswordEntry.IsPassword =
            !_isPasswordVisible;

        PasswordVisibilityButton.Text =
            "◉";

        PasswordEntry.Focus();
    }


    // =========================================================
    // FORGOT PASSWORD
    // =========================================================

    private async void ForgotPasswordClicked(
        object sender,
        EventArgs e)
    {
        await DisplayAlertAsync(
            "Forgot Password",
            "Password recovery will be implemented with OTP.",
            "OK");
    }


    // =========================================================
    // REGISTRATION
    // =========================================================

    private async void RegistrationClicked(
        object sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync(
            "SignupPage");
    }


    // =========================================================
    // PASSWORD ENTER / GO
    // =========================================================

    private async void PasswordEntryCompleted(
        object sender,
        EventArgs e)
    {
        await LoginAsync();
    }


    // =========================================================
    // LOGIN BUTTON
    // =========================================================

    private async void LoginClicked(
        object sender,
        EventArgs e)
    {
        await LoginAsync();
    }


    // =========================================================
    // LOGIN LOGIC
    // =========================================================

    private async Task LoginAsync()
    {
        if (_isLoggingIn)
            return;

        HideError();


        var mobile =
            MobileNumberEntry.Text?.Trim()
            ?? string.Empty;

        var password =
            PasswordEntry.Text
            ?? string.Empty;


        // ---------------------------------------------------------
        // MOBILE VALIDATION
        // ---------------------------------------------------------

        if (mobile.Length != 10 ||
            !mobile.All(char.IsDigit))
        {
            ShowError(
                "Please enter a valid 10-digit mobile number.");

            MobileNumberEntry.Focus();

            return;
        }


        // ---------------------------------------------------------
        // PASSWORD VALIDATION
        // ---------------------------------------------------------

        if (string.IsNullOrWhiteSpace(password))
        {
            ShowError(
                "Please enter your password.");

            PasswordEntry.Focus();

            return;
        }


        try
        {
            _isLoggingIn = true;

            LoginButton.IsEnabled = false;

            LoginButton.Text =
                "Signing in...";


            // =====================================================
            // CALL LOGIN API
            // =====================================================

            var response =
                await _authenticationApiService.LoginAsync(
                    mobile,
                    password);


            if (response == null)
            {
                ShowError(
                    "Invalid response received from the server.");

                return;
            }


            // =====================================================
            // STORE AUTHENTICATION DATA
            // =====================================================

            await SecureStorage.Default.SetAsync(
                "access_token",
                response.AccessToken);

            var savedToken =
    await SecureStorage.Default.GetAsync(
        "access_token");

            if (string.IsNullOrWhiteSpace(savedToken))
            {
                await DisplayAlertAsync(
                    "Token Error",
                    "Login succeeded, but access token was not saved.",
                    "OK");
            }

            await SecureStorage.Default.SetAsync(
                "refresh_token",
                response.RefreshToken);

            await SecureStorage.Default.SetAsync(
                "user_id",
                response.UserId.ToString());

            await SecureStorage.Default.SetAsync(
                "client_id",
                response.ClientId.ToString());

            await SecureStorage.Default.SetAsync(
                "full_name",
                response.FullName);

            await SecureStorage.Default.SetAsync(
                "mobile_number",
                response.MobileNumber);

            await SecureStorage.Default.SetAsync(
                "user_role",
                response.Role);

            await SecureStorage.Default.SetAsync(
                "client_name",
                response.ClientName);

            await SecureStorage.Default.SetAsync(
                "access_token_expires_at",
                response.AccessTokenExpiresAt.ToString("O"));


            // =====================================================
            // SUCCESS
            // =====================================================

            await Shell.Current.GoToAsync(
                "DashboardPage");
        }
        catch (UnauthorizedAccessException)
        {
            ShowError(
                "Invalid mobile number or password.");
        }
        catch (InvalidOperationException ex)
        {
            ShowError(
                ex.Message);
        }
        catch (HttpRequestException ex)
        {
            ShowError(
                $"HTTP Error: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            ShowError(
                "The request timed out. Please try again.");
        }
        catch (Exception ex)
        {
            ShowError(
                ex.Message);
        }
        finally
        {
            _isLoggingIn = false;

            LoginButton.IsEnabled = true;

            LoginButton.Text =
                "Login  →";
        }
    }


    // =========================================================
    // ERROR
    // =========================================================

    private void ShowError(
        string message)
    {
        ErrorLabel.Text =
            message;

        ErrorContainer.IsVisible =
            true;
    }


    private void HideError()
    {
        ErrorLabel.Text =
            string.Empty;

        ErrorContainer.IsVisible =
            false;
    }
}