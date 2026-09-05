using SaraAgro.Mobile.Services.Api.Expenses;
using System.Globalization;

namespace SaraAgro.Mobile.Pages.Expenses;

public partial class AddExpensePage : ContentPage
{
    private readonly ExpenseApiService _expenseApiService;

    private List<ExpenseAccountResponse> _accounts = new();

    private ExpenseAccountResponse? _selectedAccount;

    public AddExpensePage(ExpenseApiService expenseApiService)
    {
        InitializeComponent();

        _expenseApiService = expenseApiService;

        ExpenseDatePicker.Date = DateTime.Today;
        PaymentModeLabel.Text = "Cash";
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

            _accounts =
                await _expenseApiService.GetExpenseAccountsAsync();

            if (_accounts.Count == 0)
            {
                await DisplayAlert(
                    "Expense Account",
                    "Please create an expense account first.",
                    "OK");

                await Navigation.PopAsync();
                return;
            }
        }
        catch (UnauthorizedAccessException)
        {
            await DisplayAlert(
                "Session Expired",
                "Please login again.",
                "OK");

            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Expense Accounts",
                ex.Message,
                "OK");

            await Navigation.PopAsync();
        }
        finally
        {
            LoadingOverlay.IsVisible = false;
        }
    }

    private async void AccountTapped(
        object sender,
        TappedEventArgs e)
    {
        if (_accounts.Count == 0)
            return;

        string[] accountNames =
            _accounts
                .Select(x => x.Name)
                .ToArray();

        string? selected =
            await DisplayActionSheet(
                "Select Expense Account",
                "Cancel",
                null,
                accountNames);

        if (string.IsNullOrWhiteSpace(selected) ||
            selected == "Cancel")
            return;

        _selectedAccount =
            _accounts.FirstOrDefault(
                x => x.Name == selected);

        if (_selectedAccount != null)
        {
            AccountLabel.Text =
                _selectedAccount.Name;

            AccountLabel.TextColor =
                Application.Current?
                    .RequestedTheme == AppTheme.Dark
                    ? Colors.White
                    : Colors.Black;
        }
    }

    private async void PaymentModeTapped(
        object sender,
        TappedEventArgs e)
    {
        string? selected =
            await DisplayActionSheet(
                "Payment Mode",
                "Cancel",
                null,
                "Cash",
                "UPI",
                "Bank Transfer",
                "Cheque",
                "Card",
                "Other");

        if (string.IsNullOrWhiteSpace(selected) ||
            selected == "Cancel")
            return;

        PaymentModeLabel.Text = selected;
    }

    private async void SaveExpenseClicked(
        object sender,
        EventArgs e)
    {
        if (_selectedAccount == null)
        {
            await DisplayAlert(
                "Expense Account",
                "Please select an expense account.",
                "OK");

            return;
        }

        if (!decimal.TryParse(
                AmountEntry.Text?.Trim(),
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out decimal amount) ||
            amount <= 0)
        {
            await DisplayAlert(
                "Amount",
                "Please enter a valid expense amount.",
                "OK");

            return;
        }

        try
        {
            SaveButton.IsEnabled = false;
            LoadingOverlay.IsVisible = true;

            DateTime expenseDate =
                DateTime.SpecifyKind(
                    (ExpenseDatePicker.Date ?? DateTime.Now).Date,
                    DateTimeKind.Unspecified);

            var request = new CreateExpenseRequest
            {
                ExpenseAccountId = _selectedAccount.Id,
                ExpenseDate = expenseDate,
                Amount = amount,
                Description =
                    string.IsNullOrWhiteSpace(
                        DescriptionEditor.Text)
                        ? null
                        : DescriptionEditor.Text.Trim(),

                PaymentMode =
                    string.IsNullOrWhiteSpace(
                        PaymentModeLabel.Text)
                        ? "Cash"
                        : PaymentModeLabel.Text,

                ReferenceNumber =
                    string.IsNullOrWhiteSpace(
                        ReferenceEntry.Text)
                        ? null
                        : ReferenceEntry.Text.Trim()
            };

            await _expenseApiService.CreateExpenseAsync(
                _selectedAccount.Id,
                expenseDate,
                amount,
                PaymentModeLabel.Text,
                request.Description,
                request.ReferenceNumber,
                CancellationToken.None);

            await DisplayAlert(
                "Success",
                "Expense saved successfully.",
                "OK");

            await Navigation.PopAsync();
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
                "Save Expense",
                ex.Message,
                "OK");
        }
        finally
        {
            SaveButton.IsEnabled = true;
            LoadingOverlay.IsVisible = false;
        }
    }
}