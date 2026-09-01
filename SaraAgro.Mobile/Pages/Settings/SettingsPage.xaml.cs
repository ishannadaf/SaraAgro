using SaraAgro.Mobile.Services.Authentication;

namespace SaraAgro.Mobile.Pages.Settings;

public partial class SettingsPage : ContentPage
{
    private readonly AuthSessionService
        _authSessionService;

    private bool _isLoggingOut;


    // =========================================================
    // CONSTRUCTOR
    // =========================================================

    public SettingsPage(
        AuthSessionService authSessionService)
    {
        InitializeComponent();

        _authSessionService =
            authSessionService;
    }


    // =========================================================
    // PAGE APPEARING
    // =========================================================

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadUserInformationAsync();
    }


    // =========================================================
    // LOAD USER INFORMATION
    // =========================================================

    private async Task LoadUserInformationAsync()
    {
        try
        {
            var fullName =
                await _authSessionService
                    .GetFullNameAsync();

            var clientName =
                await _authSessionService
                    .GetClientNameAsync();

            var mobileNumber =
                await _authSessionService
                    .GetMobileNumberAsync();


            UserNameLabel.Text =
                string.IsNullOrWhiteSpace(fullName)
                    ? "User"
                    : fullName;


            ClientNameLabel.Text =
                string.IsNullOrWhiteSpace(clientName)
                    ? "SaraAgro"
                    : clientName;


            MobileNumberLabel.Text =
                string.IsNullOrWhiteSpace(mobileNumber)
                    ? "-"
                    : mobileNumber;
        }
        catch
        {
            UserNameLabel.Text =
                "User";

            ClientNameLabel.Text =
                "SaraAgro";

            MobileNumberLabel.Text =
                "-";
        }
    }


    // =========================================================
    // LOGOUT
    // =========================================================

    private async void LogoutClicked(
        object? sender,
        EventArgs e)
    {
        if (_isLoggingOut)
            return;


        var confirm =
            await DisplayAlertAsync(
                "Logout",
                "Are you sure you want to logout?",
                "Logout",
                "Cancel");


        if (!confirm)
            return;


        try
        {
            _isLoggingOut = true;

            LogoutButton.IsEnabled =
                false;

            LogoutButton.Text =
                "Logging out...";


            // -------------------------------------------------
            // SERVER LOGOUT + LOCAL SESSION CLEAR
            // -------------------------------------------------

            await _authSessionService
                .LogoutAsync();


            // -------------------------------------------------
            // GO TO LOGIN
            // -------------------------------------------------

            await Shell.Current.GoToAsync(
                "//LoginPage");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Logout",
                $"Unable to logout: {ex.Message}",
                "OK");
        }
        finally
        {
            _isLoggingOut = false;

            LogoutButton.IsEnabled =
                true;

            LogoutButton.Text =
                "Logout";
        }
    }
}