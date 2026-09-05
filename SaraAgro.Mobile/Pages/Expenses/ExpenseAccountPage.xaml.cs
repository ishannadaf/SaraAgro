using SaraAgro.Mobile.Services.Api.Expenses;

namespace SaraAgro.Mobile.Pages.Expenses;

public partial class ExpenseAccountPage : ContentPage
{
    private readonly ExpenseApiService _expenseApiService;

    public ExpenseAccountPage(ExpenseApiService expenseApiService)
    {
        InitializeComponent();
        _expenseApiService = expenseApiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAccountsAsync();
    }

    private async Task LoadAccountsAsync()
    {
        try
        {
            LoadingOverlay.IsVisible = true;

            var accounts =
                await _expenseApiService.GetExpenseAccountsAsync(
                    includeInactive: true);

            AccountCollectionView.ItemsSource = accounts;
        }
        catch (UnauthorizedAccessException)
        {
            await DisplayAlert(
                "Session Expired",
                "Please login again.",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Expense Accounts",
                ex.Message,
                "OK");
        }
        finally
        {
            LoadingOverlay.IsVisible = false;
        }
    }

    private async void AddAccountClicked(
        object sender,
        EventArgs e)
    {
        string? name = await DisplayPromptAsync(
            "Add Expense Account",
            "Enter account name:",
            "Save",
            "Cancel",
            "e.g. Cattle Feed");

        if (string.IsNullOrWhiteSpace(name))
            return;

        string? description = await DisplayPromptAsync(
            "Description",
            "Enter description (optional):",
            "Save",
            "Skip",
            "e.g. Daily cattle feed expenses");

        try
        {
            LoadingOverlay.IsVisible = true;

            await _expenseApiService.CreateExpenseAccountAsync(
                name.Trim(),
                string.IsNullOrWhiteSpace(description)
                    ? null
                    : description.Trim());

            await LoadAccountsAsync();

            await DisplayAlert(
                "Success",
                "Expense account created successfully.",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Add Account",
                ex.Message,
                "OK");
        }
        finally
        {
            LoadingOverlay.IsVisible = false;
        }
    }

    private async void EditAccountClicked(
        object sender,
        EventArgs e)
    {
        if (sender is not Button button ||
            button.CommandParameter is not ExpenseAccountResponse account)
            return;

        string? name = await DisplayPromptAsync(
            "Edit Expense Account",
            "Account name:",
            "Save",
            "Cancel",
            initialValue: account.Name);

        if (string.IsNullOrWhiteSpace(name))
            return;

        string? description = await DisplayPromptAsync(
            "Description",
            "Account description:",
            "Save",
            "Skip",
            initialValue: account.Description ?? string.Empty);

        try
        {
            LoadingOverlay.IsVisible = true;

            await _expenseApiService.UpdateExpenseAccountAsync(
                account.Id,
                name.Trim(),
                string.IsNullOrWhiteSpace(description)
                    ? null
                    : description.Trim(),
                account.IsActive);

            await LoadAccountsAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Edit Account",
                ex.Message,
                "OK");
        }
        finally
        {
            LoadingOverlay.IsVisible = false;
        }
    }

    private async void ToggleAccountClicked(
        object sender,
        EventArgs e)
    {
        if (sender is not Button button ||
            button.CommandParameter is not ExpenseAccountResponse account)
            return;

        string action = account.IsActive
            ? "deactivate"
            : "activate";

        bool confirm = await DisplayAlert(
            account.IsActive
                ? "Deactivate Account"
                : "Activate Account",
            $"Do you want to {action} \"{account.Name}\"?",
            "Yes",
            "No");

        if (!confirm)
            return;

        try
        {
            LoadingOverlay.IsVisible = true;

            await _expenseApiService.UpdateExpenseAccountAsync(
                account.Id,
                account.Name,
                account.Description,
                !account.IsActive);

            await LoadAccountsAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Expense Account",
                ex.Message,
                "OK");
        }
        finally
        {
            LoadingOverlay.IsVisible = false;
        }
    }
}