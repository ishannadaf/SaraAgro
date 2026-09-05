using SaraAgro.Mobile.Services.Api.Expenses;

namespace SaraAgro.Mobile.Pages.Expenses;

public partial class ExpensePage : ContentPage
{
    private readonly ExpenseApiService _expenseApiService;

    private DateTime _selectedMonth;

    public ExpensePage(
        ExpenseApiService expenseApiService)
    {
        InitializeComponent();

        _expenseApiService = expenseApiService;

        _selectedMonth = new DateTime(
            DateTime.Today.Year,
            DateTime.Today.Month,
            1);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadExpensesAsync();
    }

    private async Task LoadExpensesAsync()
    {
        try
        {
            LoadingOverlay.IsVisible = true;

            MonthLabel.Text =
                _selectedMonth.ToString("MMMM yyyy");

            ExpensePeriodLabel.Text =
                $"For {_selectedMonth:MMMM yyyy}";

            var fromDate = _selectedMonth;

            var toDate = _selectedMonth.AddMonths(1).AddDays(-1);

            var accounts =
                await _expenseApiService
                    .GetExpenseAccountsAsync();

            AccountCollectionView.ItemsSource =
                accounts;

            var expenses =
                await _expenseApiService
                    .GetExpensesAsync(
                        fromDate,
                        toDate);

            ExpenseCollectionView.ItemsSource =
                expenses;

            var total =
                expenses.Sum(x => x.Amount);

            TotalExpensesLabel.Text =
                $"₹ {total:N2}";
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
                "Expenses",
                ex.Message,
                "OK");
        }
        finally
        {
            LoadingOverlay.IsVisible = false;
        }
    }


    private async void AddExpenseClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(
            nameof(AddExpensePage));
    }


    private async void ManageAccountsClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(
            nameof(ExpenseAccountPage));
    }
}