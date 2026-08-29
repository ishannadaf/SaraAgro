namespace SaraAgro.Mobile.Pages.Authentication;

using SaraAgro.Mobile.Services.Api.Authentication;
public partial class SignupPage : ContentPage
{
    // Development OTP only.
    // This will be replaced by the real SMS OTP service later.
    private const string DevelopmentOtp = "003777";

    private bool _isPasswordVisible;
    private bool _isSubmitting;
    private bool _isOtpVerified;
    private bool _otpSent;
    private readonly AuthenticationApiService _authenticationApiService;

    public SignupPage(AuthenticationApiService authenticationApiService)
    {
        InitializeComponent();

        _authenticationApiService = authenticationApiService;

        MobileNumberEntry.TextChanged += MobileNumberEntry_TextChanged;
        OtpEntry.TextChanged += OtpEntry_TextChanged;
    }


    // =========================================================
    // MOBILE NUMBER
    // =========================================================

    private void MobileNumberEntry_TextChanged(
        object? sender,
        TextChangedEventArgs e)
    {
        var value = e.NewTextValue ?? string.Empty;

        var digitsOnly = new string(
            value
                .Where(char.IsDigit)
                .Take(10)
                .ToArray());

        if (MobileNumberEntry.Text != digitsOnly)
        {
            MobileNumberEntry.Text = digitsOnly;
            return;
        }

        // Any change to the mobile number invalidates
        // an OTP that was already sent/verified.
        if (_otpSent || _isOtpVerified)
        {
            ResetOtpVerification();
        }
    }


    // =========================================================
    // OTP INPUT
    // =========================================================

    private void OtpEntry_TextChanged(
        object? sender,
        TextChangedEventArgs e)
    {
        var value = e.NewTextValue ?? string.Empty;

        var digitsOnly = new string(
            value
                .Where(char.IsDigit)
                .Take(6)
                .ToArray());

        if (OtpEntry.Text != digitsOnly)
        {
            OtpEntry.Text = digitsOnly;
        }
    }


    // =========================================================
    // SEND OTP
    // =========================================================

    private async void SendOtpClicked(
        object sender,
        EventArgs e)
    {
        HideError();

        var mobile =
            MobileNumberEntry.Text?.Trim()
            ?? string.Empty;

        if (mobile.Length != 10 ||
            !mobile.All(char.IsDigit))
        {
            ShowError(
                "Please enter a valid 10-digit mobile number.");

            MobileNumberEntry.Focus();

            return;
        }

        try
        {
            SendOtpButton.IsEnabled = false;
            SendOtpButton.Text = "Sending...";

            // -------------------------------------------------
            // DEVELOPMENT OTP
            // -------------------------------------------------

            await Task.Delay(500);

            _otpSent = true;
            _isOtpVerified = false;

            OtpContainer.IsVisible = true;

            OtpEntry.Text = string.Empty;

            OtpStatusLabel.Text = string.Empty;
            OtpStatusLabel.IsVisible = false;

            VerifyOtpButton.IsEnabled = true;
            VerifyOtpButton.Text = "Verify";

            SubmitButton.IsEnabled = false;
            SubmitButton.Opacity = 0.55;

            SendOtpButton.Text = "Resend OTP";

            await DisplayAlertAsync(
                "OTP Sent",
                $"Development OTP for {mobile} is {DevelopmentOtp}.",
                "OK");

            OtpEntry.Focus();
        }
        catch (Exception ex)
        {
            ShowError(
                ex.Message);

            SendOtpButton.Text = "Send OTP";
        }
        finally
        {
            SendOtpButton.IsEnabled = true;
        }
    }


    // =========================================================
    // VERIFY OTP
    // =========================================================

    private async void VerifyOtpClicked(
        object sender,
        EventArgs e)
    {
        HideError();

        if (!_otpSent)
        {
            ShowOtpStatus(
                "Please request an OTP first.",
                false);

            return;
        }

        var enteredOtp =
            OtpEntry.Text?.Trim()
            ?? string.Empty;

        if (enteredOtp.Length != 6)
        {
            ShowOtpStatus(
                "Please enter the 6-digit OTP.",
                false);

            OtpEntry.Focus();

            return;
        }

        VerifyOtpButton.IsEnabled = false;
        VerifyOtpButton.Text = "Checking...";

        await Task.Delay(300);

        if (enteredOtp == DevelopmentOtp)
        {
            _isOtpVerified = true;

            ShowOtpStatus(
                "Mobile number verified successfully.",
                true);

            VerifyOtpButton.Text = "Verified";
            VerifyOtpButton.IsEnabled = false;

            SendOtpButton.IsEnabled = false;

            MobileNumberEntry.IsEnabled = false;

            SubmitButton.IsEnabled = true;
            SubmitButton.Opacity = 1.0;
        }
        else
        {
            _isOtpVerified = false;

            ShowOtpStatus(
                "Invalid OTP. Please try again.",
                false);

            VerifyOtpButton.Text = "Verify";
            VerifyOtpButton.IsEnabled = true;

            SubmitButton.IsEnabled = false;
            SubmitButton.Opacity = 0.55;
        }
    }


    // =========================================================
    // RESET OTP
    // =========================================================

    private void ResetOtpVerification()
    {
        _otpSent = false;
        _isOtpVerified = false;

        OtpContainer.IsVisible = false;

        OtpEntry.Text = string.Empty;

        OtpStatusLabel.Text = string.Empty;
        OtpStatusLabel.IsVisible = false;

        SendOtpButton.Text = "Send OTP";
        SendOtpButton.IsEnabled = true;

        VerifyOtpButton.Text = "Verify";
        VerifyOtpButton.IsEnabled = true;

        MobileNumberEntry.IsEnabled = true;

        SubmitButton.IsEnabled = false;
        SubmitButton.Opacity = 0.55;
    }


    // =========================================================
    // OTP STATUS
    // =========================================================

    private void ShowOtpStatus(
        string message,
        bool success)
    {
        OtpStatusLabel.Text = message;

        OtpStatusLabel.TextColor =
            success
                ? Color.FromArgb("#16803C")
                : Color.FromArgb("#BA1A1A");

        OtpStatusLabel.IsVisible = true;
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

        PasswordVisibilityButton.Text = "◉";

        PasswordEntry.Focus();
    }


    // =========================================================
    // BACK
    // =========================================================

    private async void BackClicked(
        object sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }


    // =========================================================
    // LOGIN
    // =========================================================

    private async void LoginClicked(
        object sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }


    // =========================================================
    // SUBMIT REGISTRATION
    // =========================================================

    private async void SubmitRegistrationClicked(
    object sender,
    EventArgs e)
    {
        if (_isSubmitting)
            return;

        HideError();

        // =====================================================
        // OTP MUST BE VERIFIED FIRST
        // =====================================================

        if (!_isOtpVerified)
        {
            ShowError(
                "Please verify your mobile number before submitting.");

            return;
        }


        // =====================================================
        // READ FORM VALUES
        // =====================================================

        var businessName =
            BusinessNameEntry.Text?.Trim()
            ?? string.Empty;

        var ownerName =
            OwnerNameEntry.Text?.Trim()
            ?? string.Empty;

        var mobile =
            MobileNumberEntry.Text?.Trim()
            ?? string.Empty;

        var email =
            EmailEntry.Text?.Trim()
            ?? string.Empty;

        var city =
            CityEntry.Text?.Trim()
            ?? string.Empty;

        var password =
            PasswordEntry.Text
            ?? string.Empty;

        var confirmPassword =
            ConfirmPasswordEntry.Text
            ?? string.Empty;


        // =====================================================
        // VALIDATION
        // =====================================================

        if (string.IsNullOrWhiteSpace(businessName))
        {
            ShowError(
                "Please enter your dairy or business name.");

            BusinessNameEntry.Focus();

            return;
        }


        if (string.IsNullOrWhiteSpace(ownerName))
        {
            ShowError(
                "Please enter the owner's name.");

            OwnerNameEntry.Focus();

            return;
        }


        if (mobile.Length != 10 ||
            !mobile.All(char.IsDigit))
        {
            ShowError(
                "Please enter a valid 10-digit mobile number.");

            MobileNumberEntry.Focus();

            return;
        }


        if (!string.IsNullOrWhiteSpace(email) &&
            !IsValidEmail(email))
        {
            ShowError(
                "Please enter a valid email address.");

            EmailEntry.Focus();

            return;
        }


        if (string.IsNullOrWhiteSpace(city))
        {
            ShowError(
                "Please enter your city or location.");

            CityEntry.Focus();

            return;
        }


        if (password.Length < 8)
        {
            ShowError(
                "Password must contain at least 8 characters.");

            PasswordEntry.Focus();

            return;
        }


        if (password != confirmPassword)
        {
            ShowError(
                "Passwords do not match.");

            ConfirmPasswordEntry.Focus();

            return;
        }


        // =====================================================
        // SUBMIT TO API
        // =====================================================

        try
        {
            _isSubmitting = true;

            SubmitButton.IsEnabled = false;
            SubmitButton.Text = "Submitting...";


            var request =
                new RegisterRequest
                {
                    BusinessName =
                        businessName,

                    OwnerName =
                        ownerName,

                    MobileNumber =
                        mobile,

                    Email =
                        string.IsNullOrWhiteSpace(email)
                            ? null
                            : email,

                    City =
                        city,

                    Password =
                        password,

                    Otp =
                        DevelopmentOtp
                };


            var response =
                await _authenticationApiService.RegisterAsync(
                    request);


            if (response == null ||
                !response.Success)
            {
                ShowError(
                    response?.Message
                    ?? "Registration failed. Please try again.");

                return;
            }


            // =================================================
            // SUCCESS
            // =================================================

            await DisplayAlertAsync(
                "Registration Successful",
                "Your account has been created successfully. Please login to continue.",
                "OK");


            await Shell.Current.GoToAsync(
                "..");
        }
        catch (UnauthorizedAccessException)
        {
            ShowError(
                "Your OTP verification is no longer valid. Please verify again.");
        }
        catch (InvalidOperationException ex)
        {
            ShowError(
                ex.Message);
        }
        catch (HttpRequestException)
        {
            ShowError(
                "Unable to connect to the server. Please check your internet connection and try again.");
        }
        catch (Exception ex)
        {
            ShowError(
                ex.Message);
        }
        finally
        {
            _isSubmitting = false;

            if (_isOtpVerified)
            {
                SubmitButton.IsEnabled = true;
                SubmitButton.Opacity = 1.0;
            }

            SubmitButton.Text =
                "Submit Registration  →";
        }
    }


    // =========================================================
    // EMAIL VALIDATION
    // =========================================================

    private static bool IsValidEmail(
        string email)
    {
        try
        {
            var address =
                new System.Net.Mail.MailAddress(email);

            return address.Address == email;
        }
        catch
        {
            return false;
        }
    }


    // =========================================================
    // ERROR
    // =========================================================

    private void ShowError(
        string message)
    {
        ErrorLabel.Text = message;
        ErrorContainer.IsVisible = true;
    }


    private void HideError()
    {
        ErrorLabel.Text = string.Empty;
        ErrorContainer.IsVisible = false;
    }
}