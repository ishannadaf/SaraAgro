using SaraAgro.Mobile.Services.Authentication;

namespace SaraAgro.Mobile;

public partial class App : Application
{
    private readonly AuthSessionService
        _authSessionService;

    private bool _startupCompleted;


    // =========================================================
    // CONSTRUCTOR
    // =========================================================

    public App(
        AuthSessionService authSessionService)
    {
        InitializeComponent();

        _authSessionService =
            authSessionService;
    }


    // =========================================================
    // CREATE WINDOW
    // =========================================================

    protected override Window CreateWindow(
        IActivationState? activationState)
    {
        var shell =
            new AppShell();


        var window =
            new Window(shell);


        window.Created +=
            Window_Created;


        return window;
    }


    // =========================================================
    // WINDOW CREATED
    // =========================================================

    private async void Window_Created(
        object? sender,
        EventArgs e)
    {
        if (_startupCompleted)
            return;


        _startupCompleted = true;


        await InitializeApplicationAsync();
    }


    // =========================================================
    // APPLICATION STARTUP
    // =========================================================

    private async Task InitializeApplicationAsync()
    {
        try
        {
            // Give Shell enough time to finish creating
            // its initial LoginPage.
            await Task.Delay(300);


            // -------------------------------------------------
            // CHECK FOR SAVED SESSION
            // -------------------------------------------------

            var hasSession =
                await _authSessionService
                    .HasSessionAsync();


            // -------------------------------------------------
            // NO SAVED SESSION
            // -------------------------------------------------

            if (!hasSession)
            {
                await GoToLoginAsync();

                return;
            }


            // -------------------------------------------------
            // SAVED SESSION EXISTS
            // -------------------------------------------------

            var sessionValid =
                await _authSessionService
                    .EnsureValidSessionAsync();


            if (sessionValid)
            {
                await GoToDashboardAsync();

                return;
            }


            // -------------------------------------------------
            // SESSION INVALID / EXPIRED
            // -------------------------------------------------

            await _authSessionService
                .ClearSessionAsync();


            await GoToLoginAsync();
        }
        catch (Exception ex)
        {
            // Do not show "Session Expired" for every
            // startup exception.
            //
            // If startup session validation itself fails,
            // clear the local session and show Login.

            await _authSessionService
                .ClearSessionAsync();


            System.Diagnostics.Debug.WriteLine(
                $"Application startup error: {ex}");


            await GoToLoginAsync();
        }
    }


    // =========================================================
    // GO TO LOGIN
    // =========================================================

    private static async Task GoToLoginAsync()
    {
        try
        {
            if (Shell.Current == null)
                return;


            if (Shell.Current.CurrentState.Location
                .OriginalString
                .Contains("LoginPage"))
            {
                return;
            }


            await Shell.Current.GoToAsync(
                "//LoginPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Navigation to Login failed: {ex}");
        }
    }


    // =========================================================
    // GO TO DASHBOARD
    // =========================================================

    private static async Task GoToDashboardAsync()
    {
        try
        {
            if (Shell.Current == null)
                return;


            await Shell.Current.GoToAsync(
                "DashboardPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Navigation to Dashboard failed: {ex}");
        }
    }
}