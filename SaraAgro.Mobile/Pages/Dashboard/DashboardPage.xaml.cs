using SaraAgro.Mobile.Pages.Billing;
using SaraAgro.Mobile.Pages.Expenses;
using SaraAgro.Mobile.Pages.MilkDistribution;
using SaraAgro.Mobile.Pages.Reports;
using SaraAgro.Mobile.Services.Api.Customer;
using SaraAgro.Mobile.Services.Api.MilkDistribution;
using SaraAgro.Mobile.Services.Api.Reports;
using SaraAgro.Mobile.Services.Authentication;

namespace SaraAgro.Mobile.Pages.Dashboard;

public partial class DashboardPage : ContentPage
{
    private bool _isLoading;

    private MilkDistributionApiService? _milkDistributionApiService;
    private CustomerApiService? _customerApiService;
    private ReportsApiService? _reportsApiService;
    private AuthSessionService? _authSessionService;


    // =========================================================
    // CONSTRUCTOR
    // =========================================================

    public DashboardPage()
    {
        InitializeComponent();
    }


    // =========================================================
    // PAGE APPEARING
    // =========================================================

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Give Shell/DI time to finish attaching the page
        // when coming directly from Login.
        await Task.Yield();

        await LoadDashboardAsync();
    }


    // =========================================================
    // LOAD DASHBOARD
    // =========================================================

    private async Task LoadDashboardAsync()
    {
        if (_isLoading)
            return;

        try
        {
            _isLoading = true;


            // =================================================
            // RESOLVE SERVICES
            // =================================================

            ResolveServices();

            // Give DI one short opportunity to become available
            // when Dashboard is opened directly after Login.
            if (_milkDistributionApiService == null ||
                _customerApiService == null ||
                _reportsApiService == null ||
                _authSessionService == null)
            {
                await Task.Delay(150);

                ResolveServices();
            }


            if (_milkDistributionApiService == null ||
                _customerApiService == null ||
                _reportsApiService == null)
            {
                throw new InvalidOperationException(
                    "Dashboard services could not be initialized.");
            }


            // =================================================
            // TODAY
            // =================================================

            var today =
                DateTime.Today;

            // =================================================
            // LOGGED-IN USER / DAIRY
            // =================================================

            var dairyName =
                await _authSessionService
                    .GetClientNameAsync();

            var ownerName =
                await _authSessionService
                    .GetFullNameAsync();


            DairyNameLabel.Text =
                string.IsNullOrWhiteSpace(dairyName)
                    ? "Your Dairy"
                    : dairyName;


            OwnerNameLabel.Text =
                string.IsNullOrWhiteSpace(ownerName)
                    ? "Dairy Owner"
                    : ownerName;

            TodayDateLabel.Text =
                today.ToString("dd MMM yyyy");


            // =================================================
            // LOAD DATA
            // =================================================

            var todaySummaryTask =
                _milkDistributionApiService
                    .GetDailySummaryAsync(
                        today);

            var monthSummaryTask =
                _milkDistributionApiService
                    .GetMonthlySummaryAsync(
                        today);

            var customersTask =
                _customerApiService
                    .GetCustomersAsync();

            var financialSummaryTask =
                _reportsApiService
                    .GetFinancialSummaryAsync(
                        today);


            await Task.WhenAll(
                todaySummaryTask,
                monthSummaryTask,
                customersTask,
                financialSummaryTask);


            var summary =
                await todaySummaryTask;

            var monthSummary =
                await monthSummaryTask;

            var customers =
                await customersTask;

            var financialSummary =
                await financialSummaryTask;


            // =================================================
            // FINANCIAL SUMMARY
            // =================================================

            TodayRevenueLabel.Text =
                $"₹{financialSummary.TodayRevenue:0.00}";

            TodayCollectionLabel.Text =
                $"₹{financialSummary.TodayCollection:0.00}";

            TodayExpenseLabel.Text =
                $"₹{financialSummary.TodayExpense:0.00}";

            TodayProfitLabel.Text =
                $"₹{financialSummary.TodayProfit:0.00}";

            MonthlyRevenueLabel.Text =
                $"₹{financialSummary.MonthlyRevenue:0.00}";

            MonthlyCollectionLabel.Text =
                $"₹{financialSummary.MonthlyCollection:0.00}";

            MonthlyExpenseLabel.Text =
                $"₹{financialSummary.MonthlyExpense:0.00}";

            MonthlyProfitLabel.Text =
                $"₹{financialSummary.MonthlyProfit:0.00}";


            // =================================================
            // MONTH OVERVIEW
            // =================================================

            MonthOverviewTitleLabel.Text =
                today.ToString("MMMM yyyy");


            MonthActiveCustomersLabel.Text =
                monthSummary.ActiveCustomerCount
                    .ToString();


            MonthCowQuantityLabel.Text =
                $"{monthSummary.CowQuantity:0.00} L";


            MonthBuffaloQuantityLabel.Text =
                $"{monthSummary.BuffaloQuantity:0.00} L";


            MonthTotalQuantityLabel.Text =
                $"{monthSummary.TotalQuantity:0.00} L";


            MonthTotalAmountLabel.Text =
                $"₹{monthSummary.TotalAmount:0.00}";


            MonthDaysRecordedLabel.Text =
                $"{monthSummary.DaysRecorded} " +
                $"{(
                    monthSummary.DaysRecorded == 1
                        ? "day"
                        : "days"
                )}";


            // =================================================
            // ACTIVE CUSTOMERS
            // =================================================

            var activeCustomerCount =
                customers.Count(
                    x => x.IsActive);


            CustomerCountLabel.Text =
                activeCustomerCount.ToString();


            // =================================================
            // TODAY - MORNING
            // =================================================

            MorningCowLabel.Text =
                $"{summary.MorningCowQuantity:0.00} L";


            MorningBuffaloLabel.Text =
                $"{summary.MorningBuffaloQuantity:0.00} L";


            MorningTotalLabel.Text =
                $"{summary.MorningTotalQuantity:0.00} L";


            MorningAmountLabel.Text =
                $"₹{summary.MorningAmount:0.00}";


            // =================================================
            // TODAY - EVENING
            // =================================================

            EveningCowLabel.Text =
                $"{summary.EveningCowQuantity:0.00} L";


            EveningBuffaloLabel.Text =
                $"{summary.EveningBuffaloQuantity:0.00} L";


            EveningTotalLabel.Text =
                $"{summary.EveningTotalQuantity:0.00} L";


            EveningAmountLabel.Text =
                $"₹{summary.EveningAmount:0.00}";


            // =================================================
            // TODAY - TOTAL
            // =================================================

            var todayCowQuantity =
                summary.MorningCowQuantity +
                summary.EveningCowQuantity;


            var todayBuffaloQuantity =
                summary.MorningBuffaloQuantity +
                summary.EveningBuffaloQuantity;


            TotalCowLabel.Text =
                $"{todayCowQuantity:0.00} L";


            TotalBuffaloLabel.Text =
                $"{todayBuffaloQuantity:0.00} L";


            TotalQuantityLabel.Text =
                $"{summary.TotalQuantity:0.00} L";


            TotalAmountLabel.Text =
                $"₹{summary.TotalAmount:0.00}";
        }
        catch (UnauthorizedAccessException ex)
        {
            await ShowSessionExpiredAsync(
                ex.Message);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Unable to Load Dashboard",
                ex.Message,
                "OK");
        }
        finally
        {
            _isLoading = false;
        }
    }


    // =========================================================
    // RESOLVE API SERVICES
    // =========================================================

    private void ResolveServices()
    {
        var services =
            Handler?.MauiContext?.Services;

        if (services == null)
            return;


        _milkDistributionApiService ??=
            services.GetService<
                MilkDistributionApiService>();


        _customerApiService ??=
            services.GetService<
                CustomerApiService>();


        _reportsApiService ??=
            services.GetService<
                ReportsApiService>();


        _authSessionService ??=
            services.GetService<
                AuthSessionService>();
    }


    // =========================================================
    // SESSION EXPIRED
    // =========================================================

    private static async Task ShowSessionExpiredAsync(
        string message)
    {
        var page =
            Application.Current?
                .Windows
                .FirstOrDefault()?
                .Page;


        if (page == null)
            return;


        await page.DisplayAlertAsync(
            "Session Expired",
            message,
            "OK");


        await Shell.Current.GoToAsync(
            "//LoginPage");
    }


    // =========================================================
    // RATE GROUPS
    // =========================================================

    private async void RateGroupsTapped(
        object? sender,
        TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(
            "RateGroupPage");
    }


    // =========================================================
    // QUICK ACTIONS
    // =========================================================

    private async void DistributionTapped(
        object? sender,
        TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(
            nameof(MilkDistributionPage));
    }


    private async void CustomersTapped(
        object? sender,
        TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(
            "CustomersPage");
    }


    private async void RatesTapped(
        object? sender,
        TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(
            "RateMasterPage");
    }


    // =========================================================
    // BILLING
    // =========================================================

    private async void BillingTapped(
        object? sender,
        TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(
            nameof(BillingPage));
    }


    // =========================================================
    // EXPENSES
    // =========================================================

    private async void ExpensesTapped(
        object? sender,
        TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(
            nameof(ExpensePage));
    }


    // =========================================================
    // REPORTS
    // =========================================================

    private async void ReportsTapped(
        object? sender,
        TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(
            nameof(ReportsPage));
    }


    // =========================================================
    // SETTINGS
    // =========================================================

    private async void SettingsTapped(
        object? sender,
        TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(
            "SettingsPage");
    }
}