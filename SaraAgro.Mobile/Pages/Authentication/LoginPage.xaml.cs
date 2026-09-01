using SaraAgro.Mobile.Services.Api.Authentication;
using SaraAgro.Mobile.Services.Authentication;

namespace SaraAgro.Mobile.Pages.Authentication;

public partial class LoginPage : ContentPage
{
    private readonly AuthenticationApiService
        _authenticationApiService;

    private readonly AuthSessionService
        _authSessionService;

    private bool _isPasswordVisible;
    private bool _isLoggingIn;


    // =========================================================
    // CONSTRUCTOR
    // =========================================================

    public LoginPage(
        AuthenticationApiService authenticationApiService,
        AuthSessionService authSessionService)
    {
        InitializeComponent();

        _authenticationApiService =
            authenticationApiService;

        _authSessionService =
            authSessionService;

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
        object? sender,
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
        object? sender,
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
        object? sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync(
            "SignupPage");
    }


    // =========================================================
    // PASSWORD ENTER / GO
    // =========================================================

    private async void PasswordEntryCompleted(
        object? sender,
        EventArgs e)
    {
        await LoginAsync();
    }


    // =========================================================
    // LOGIN BUTTON
    // =========================================================

    private async void LoginClicked(
        object? sender,
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


            LoginButton.IsEnabled =
                false;


            LoginButton.Text =
                "Signing in...";


            // =====================================================
            // CALL LOGIN API
            // =====================================================

            var response =
                await _authenticationApiService.LoginAsync(
                    mobile,
                    password);


            // =====================================================
            // VALIDATE RESPONSE
            // =====================================================

            if (response == null)
            {
                ShowError(
                    "Invalid response received from the server.");

                return;
            }


            if (string.IsNullOrWhiteSpace(
                    response.AccessToken))
            {
                ShowError(
                    "Login succeeded, but the server did not return an access token.");

                return;
            }


            if (string.IsNullOrWhiteSpace(
                    response.RefreshToken))
            {
                ShowError(
                    "Login succeeded, but the server did not return a refresh token.");

                return;
            }


            // =====================================================
            // SAVE COMPLETE AUTH SESSION
            // =====================================================
            //
            // AuthSessionService is the single source of truth
            // for SecureStorage.
            //
            // It stores:
            //   access token
            //   refresh token
            //   access token expiry
            //   user information
            //   client information
            //
            // =====================================================

            await _authSessionService.SaveSessionAsync(
                response);


            // =====================================================
            // VERIFY SESSION WAS SAVED
            // =====================================================

            var savedAccessToken =
                await _authSessionService
                    .GetAccessTokenAsync();


            var savedRefreshToken =
                await _authSessionService
                    .GetRefreshTokenAsync();


            if (string.IsNullOrWhiteSpace(
                    savedAccessToken) ||
                string.IsNullOrWhiteSpace(
                    savedRefreshToken))
            {
                await _authSessionService
                    .ClearSessionAsync();


                ShowError(
                    "Login succeeded, but the authentication session could not be saved.");

                return;
            }


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


            LoginButton.IsEnabled =
                true;


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