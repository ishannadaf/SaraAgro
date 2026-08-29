using SaraAgro.Mobile.Services.Api.Customer;
using SaraAgro.Mobile.Services.Api.Reports;
using System.Globalization;
using Microsoft.Maui.Controls.Shapes;
namespace SaraAgro.Mobile.Pages.Reports;

using Microsoft.Maui.ApplicationModel;

public partial class ReportsPage : ContentPage
{
    private ReportsApiService? _reportsApiService;
    private CustomerApiService? _customerApiService;


    private readonly List<CustomerOption> _customers = new();
    private CustomerOption? _selectedCustomer;

    private bool _isLoading;
    private CancellationTokenSource? _searchCancellation;


    // =========================================================
    // CONSTRUCTOR
    // =========================================================

    public ReportsPage()
    {
        InitializeComponent();

        FromDatePicker.Date =
            DateTime.Today.AddDays(-6);

        ToDatePicker.Date =
            DateTime.Today;

        DailyDatePicker.Date =
            DateTime.Today;

        MonthlyDatePicker.Date =
            new DateTime(
                DateTime.Today.Year,
                DateTime.Today.Month,
                1);

        PendingDatePicker.Date =
            DateTime.Today;

        HideAllResults();
    }


    // =========================================================
    // PAGE APPEARING
    // =========================================================

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            ResolveServices();

            if (_reportsApiService == null ||
                _customerApiService == null)
            {
                await Task.Delay(150);

                ResolveServices();
            }

            if (_reportsApiService == null ||
                _customerApiService == null)
            {
                await DisplayAlertAsync(
                    "Reports",
                    "Unable to initialize report services.",
                    "OK");

                return;
            }

            await LoadCustomersAsync();
        }
        catch (UnauthorizedAccessException ex)
        {
            await ShowSessionExpiredAsync(
                ex.Message);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Reports",
                ex.Message,
                "OK");
        }
    }


    // =========================================================
    // RESOLVE SERVICES
    // =========================================================
    private async Task OpenPdfAsync(
    byte[] pdfBytes,
    string fileName)
    {
        if (pdfBytes == null || pdfBytes.Length == 0)
        {
            throw new InvalidOperationException(
                "The server returned an empty PDF.");
        }

        var filePath =
            System.IO.Path.Combine(
                FileSystem.CacheDirectory,
                fileName);

        await File.WriteAllBytesAsync(
            filePath,
            pdfBytes);

        await Launcher.Default.OpenAsync(
            new OpenFileRequest
            {
                File =
                    new ReadOnlyFile(filePath)
            });
    }
    private void ResolveServices()
    {
        var services =
            Handler?.MauiContext?.Services;

        if (services == null)
            return;

        _reportsApiService ??=
            services.GetService<ReportsApiService>();

        _customerApiService ??=
            services.GetService<CustomerApiService>();
    }


    // =========================================================
    // LOAD CUSTOMERS
    // =========================================================

    private async Task LoadCustomersAsync()
    {
        if (_customerApiService == null)
            return;

        var customers =
            await _customerApiService
                .GetCustomersAsync();

        _customers.Clear();

        foreach (var customer in customers
                     .Where(x => x.IsActive)
                     .OrderBy(x => x.FullName))
        {
            _customers.Add(
                new CustomerOption
                {
                    Id =
                        customer.Id,

                    Code =
                        customer.CustomerCode,

                    Name =
                        customer.FullName,

                    Mobile =
                        customer.MobileNumber
                });
        }
    }


    // =========================================================
    // CUSTOMER SEARCH
    // =========================================================

    private async void CustomerSearchTextChanged(
        object? sender,
        TextChangedEventArgs e)
    {
        _searchCancellation?.Cancel();

        _searchCancellation =
            new CancellationTokenSource();

        var token =
            _searchCancellation.Token;

        try
        {
            await Task.Delay(
                150,
                token);

            var search =
                e.NewTextValue?
                    .Trim();

            if (string.IsNullOrWhiteSpace(search))
            {
                CustomerSearchResults.IsVisible =
                    false;

                CustomerSearchResults.Children.Clear();

                return;
            }

            var matches =
                _customers
                    .Where(
                        x =>
                            x.Code.Contains(
                                search,
                                StringComparison.OrdinalIgnoreCase)
                            ||
                            x.Name.Contains(
                                search,
                                StringComparison.OrdinalIgnoreCase)
                            ||
                            x.Mobile.Contains(
                                search,
                                StringComparison.OrdinalIgnoreCase))
                    .Take(15)
                    .ToList();

            CustomerSearchResults.Children.Clear();

            foreach (var customer in matches)
            {
                var button =
                    new Button
                    {
                        Text =
                            string.IsNullOrWhiteSpace(
                                customer.Mobile)
                                ? $"{customer.Code}  •  {customer.Name}"
                                : $"{customer.Code}  •  {customer.Name}\n{customer.Mobile}",

                        HorizontalOptions =
                            LayoutOptions.Fill,

                        BackgroundColor =
                            Colors.Transparent,

                        TextColor =
                            Application.Current?
                                .Resources["Secondary"]
                                as Color
                            ?? (
                                Application.Current?
                                    .RequestedTheme == AppTheme.Dark
                                    ? Colors.White
                                    : Colors.Black),

                        Padding =
                            new Thickness(
                                12,
                                10)
                    };

                button.Clicked +=
                    (_, _) =>
                    {
                        SelectCustomer(
                            customer);
                    };

                CustomerSearchResults.Children.Add(
                    button);
            }

            CustomerSearchResults.IsVisible =
                matches.Count > 0;
        }
        catch (TaskCanceledException)
        {
        }
    }


    // =========================================================
    // SELECT CUSTOMER
    // =========================================================

    private void SelectCustomer(
    CustomerOption customer)
    {
        _selectedCustomer =
            customer;

        SelectedCustomerId =
            customer.Id;

        SelectedCustomerCodeLabel.Text =
            customer.Code;

        SelectedCustomerNameLabel.Text =
            customer.Name;

        SelectedCustomerCard.IsVisible =
            true;

        CustomerSearchResults.IsVisible =
            false;

        CustomerSearchEntry.Text =
            string.Empty;
    }


    private int? SelectedCustomerId;


    // =========================================================
    // CLEAR CUSTOMER
    // =========================================================

    private void ClearCustomerClicked(
    object? sender,
    EventArgs e)
    {
        _selectedCustomer =
            null;

        SelectedCustomerId =
            null;

        SelectedCustomerCard.IsVisible =
            false;

        CustomerSearchEntry.Text =
            string.Empty;

        CustomerSearchResults.IsVisible =
            false;
    }


    // =========================================================
    // CUSTOMER LEDGER
    // =========================================================

    private async void CustomerLedgerClicked(
    object? sender,
    EventArgs e)
    {
        try
        {
            // =====================================================
            // VALIDATE CUSTOMER
            // =====================================================

            if (_reportsApiService == null)
                ResolveServices();

            if (_selectedCustomer == null ||
        !SelectedCustomerId.HasValue)
                {
                    await DisplayAlertAsync(
                        "Customer",
                        "Please search and select a customer.",
                        "OK");

                    return;
                }


            // =====================================================
            // VALIDATE DATES
            // =====================================================

            var fromDate =
                FromDatePicker.Date ?? DateTime.Today;

            var toDate =
                ToDatePicker.Date ?? DateTime.Today;


            if (toDate < fromDate)
            {
                await DisplayAlertAsync(
                    "Date Range",
                    "To date cannot be earlier than From date.",
                    "OK");

                return;
            }


            // =====================================================
            // BUTTON STATE
            // =====================================================

            CustomerLedgerButton.IsEnabled =
                false;

            CustomerLedgerButton.Text =
                "Generating PDF...";


            // =====================================================
            // DOWNLOAD PDF
            // =====================================================

            var pdf =
                await _reportsApiService
                    .DownloadCustomerLedgerPdfAsync(
                        SelectedCustomerId.Value,
                        fromDate,
                        toDate);


            if (pdf == null ||
                pdf.Length == 0)
            {
                throw new InvalidOperationException(
                    "The server returned an empty PDF.");
            }


            // =====================================================
            // OPEN PDF
            // =====================================================

            var safeName =
                string.IsNullOrWhiteSpace(
                    _selectedCustomer.Name)
                    ? "Customer"
                    : _selectedCustomer.Name;


            foreach (
                var invalidChar
                in System.IO.Path.GetInvalidFileNameChars())
            {
                safeName =
                    safeName.Replace(
                        invalidChar,
                        '_');
            }


            var fileName =
                $"CustomerLedger_{safeName}_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.pdf";


            await OpenPdfAsync(
                pdf,
                fileName);
        }
        catch (UnauthorizedAccessException ex)
        {
            await ShowSessionExpiredAsync(
                ex.Message);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Unable to Generate Report",
                ex.Message,
                "OK");
        }
        finally
        {
            CustomerLedgerButton.IsEnabled =
                true;

            CustomerLedgerButton.Text =
                "Generate Ledger";
        }
    }



    // =========================================================
    // DAILY REPORT
    // =========================================================

    private async void DailyReportClicked(
    object? sender,
    EventArgs e)
    {
        try
        {
            if (_reportsApiService == null)
                ResolveServices();

            if (_reportsApiService == null)
            {
                await DisplayAlertAsync(
                    "Reports",
                    "Unable to initialize report service.",
                    "OK");

                return;
            }

            var selectedDate =
                DailyDatePicker.Date ?? DateTime.Today;

            DailyReportButton.IsEnabled = false;
            DailyReportButton.Text = "Generating PDF...";

            var pdf =
                await _reportsApiService
                    .DownloadDailyReportPdfAsync(
                        selectedDate);

            await OpenPdfAsync(
                pdf,
                $"DailyReport_{selectedDate:yyyyMMdd}.pdf");
        }
        catch (UnauthorizedAccessException ex)
        {
            await ShowSessionExpiredAsync(
                ex.Message);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Unable to Generate Report",
                ex.Message,
                "OK");
        }
        finally
        {
            DailyReportButton.IsEnabled = true;
            DailyReportButton.Text = "View Daily Report";
        }
    }


    // =========================================================
    // MONTHLY REPORT
    // =========================================================

    private async void MonthlyReportClicked(
    object? sender,
    EventArgs e)
    {
        try
        {
            if (_reportsApiService == null)
                ResolveServices();

            if (_reportsApiService == null)
            {
                await DisplayAlertAsync(
                    "Reports",
                    "Unable to initialize report service.",
                    "OK");

                return;
            }

            var selectedMonth =
                MonthlyDatePicker.Date ?? DateTime.Today;

            MonthlyReportButton.IsEnabled = false;
            MonthlyReportButton.Text = "Generating PDF...";

            var pdf =
                await _reportsApiService
                    .DownloadMonthlyReportPdfAsync(
                        selectedMonth);

            await OpenPdfAsync(
                pdf,
                $"MonthlyReport_{selectedMonth:yyyyMM}.pdf");
        }
        catch (UnauthorizedAccessException ex)
        {
            await ShowSessionExpiredAsync(
                ex.Message);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Unable to Generate Report",
                ex.Message,
                "OK");
        }
        finally
        {
            MonthlyReportButton.IsEnabled = true;
            MonthlyReportButton.Text = "View Monthly Report";
        }
    }


    // =========================================================
    // PENDING REPORT
    // =========================================================

    private async void PendingReportClicked(
    object? sender,
    EventArgs e)
    {
        try
        {
            if (_reportsApiService == null)
                ResolveServices();

            if (_reportsApiService == null)
            {
                await DisplayAlertAsync(
                    "Reports",
                    "Unable to initialize report service.",
                    "OK");

                return;
            }

            var asOfDate =
                PendingDatePicker.Date ?? DateTime.Today;

            PendingReportButton.IsEnabled = false;
            PendingReportButton.Text = "Generating PDF...";

            var pdf =
                await _reportsApiService
                    .DownloadPendingAmountReportPdfAsync(
                        asOfDate);

            await OpenPdfAsync(
                pdf,
                $"PendingAmount_{asOfDate:yyyyMMdd}.pdf");
        }
        catch (UnauthorizedAccessException ex)
        {
            await ShowSessionExpiredAsync(
                ex.Message);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Unable to Generate Report",
                ex.Message,
                "OK");
        }
        finally
        {
            PendingReportButton.IsEnabled = true;
            PendingReportButton.Text = "View Pending Amount";
        }
    }


    // =========================================================
    // PAYMENTS REPORT
    // =========================================================

    private async void PaymentsReportClicked(
    object? sender,
    EventArgs e)
    {
        try
        {
            if (_reportsApiService == null)
                ResolveServices();

            if (_reportsApiService == null)
            {
                await DisplayAlertAsync(
                    "Reports",
                    "Unable to initialize report service.",
                    "OK");

                return;
            }

            var fromDate =
                PaymentsFromDatePicker.Date ?? DateTime.Today;

            var toDate =
                PaymentsToDatePicker.Date ?? DateTime.Today;

            if (toDate < fromDate)
            {
                await DisplayAlertAsync(
                    "Date Range",
                    "To date cannot be earlier than From date.",
                    "OK");

                return;
            }

            PaymentsReportButton.IsEnabled = false;
            PaymentsReportButton.Text = "Generating PDF...";

            var pdf =
                await _reportsApiService
                    .DownloadAllPaymentReportPdfAsync(
                        fromDate,
                        toDate);

            await OpenPdfAsync(
                pdf,
                $"AllPayments_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.pdf");
        }
        catch (UnauthorizedAccessException ex)
        {
            await ShowSessionExpiredAsync(
                ex.Message);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Unable to Generate Report",
                ex.Message,
                "OK");
        }
        finally
        {
            PaymentsReportButton.IsEnabled = true;
            PaymentsReportButton.Text = "View All Payments";
        }
    }


    // =========================================================
    // CUSTOMER LEDGER RENDER
    // =========================================================

    private void RenderCustomerLedger(
        CustomerLedgerResponse report)
    {
        HideAllResults();

        CustomerLedgerResult.IsVisible =
            true;


        CustomerLedgerTitleLabel.Text =
            $"{report.CustomerCode} • {report.CustomerName}";


        CustomerLedgerPeriodLabel.Text =
            $"{report.FromDate:dd MMM yyyy} - " +
            $"{report.ToDate:dd MMM yyyy}";


        LedgerOpeningLabel.Text =
            $"₹{report.OpeningBalance:0.00}";


        LedgerMilkQuantityLabel.Text =
            $"{report.TotalMilkQuantity:0.00} L";


        LedgerMilkAmountLabel.Text =
            $"₹{report.TotalMilkAmount:0.00}";


        LedgerPaymentsLabel.Text =
            $"₹{report.TotalPayments:0.00}";


        LedgerClosingLabel.Text =
            $"₹{report.ClosingBalance:0.00}";


        CustomerLedgerList.Children.Clear();


        if (report.Entries.Count == 0)
        {
            CustomerLedgerEmptyLabel.IsVisible =
                true;

            return;
        }


        CustomerLedgerEmptyLabel.IsVisible =
            false;


        foreach (var entry in report.Entries)
        {
            var debitText =
                entry.Debit > 0
                    ? $"₹{entry.Debit:0.00}"
                    : "-";


            var creditText =
                entry.Credit > 0
                    ? $"₹{entry.Credit:0.00}"
                    : "-";


            var quantityText =
                entry.Quantity > 0
                    ? $"{entry.Quantity:0.00} L"
                    : "-";


            var card =
                new Border
                {
                    Padding =
                        new Thickness(
                            12),

                    StrokeThickness =
                        1,

                    Stroke =
                        Application.Current?
                            .Resources["BorderLight"]
                            as Color
                        ?? Colors.LightGray,

                    BackgroundColor =
                        Application.Current?
                            .Resources["CardLight"]
                            as Color
                        ?? Colors.White,

                    StrokeShape =
                        new RoundRectangle
                        {
                            CornerRadius = 12
                        }
                };


            var layout =
                new VerticalStackLayout
                {
                    Spacing = 5
                };


            layout.Children.Add(
                new Label
                {
                    Text =
                        entry.Date.ToString(
                            "dd MMM yyyy"),

                    FontAttributes =
                        FontAttributes.Bold,

                    FontSize =
                        13
                });


            layout.Children.Add(
                new Label
                {
                    Text =
                        entry.Description,

                    FontSize =
                        12,

                    Opacity =
                        0.75
                });


            var details =
                new Grid
                {
                    ColumnDefinitions =
                        new ColumnDefinitionCollection
                        {
                            new ColumnDefinition
                            {
                                Width = GridLength.Star
                            },

                            new ColumnDefinition
                            {
                                Width = GridLength.Auto
                            }
                        }
                };


            details.Add(
                new Label
                {
                    Text =
                        $"Qty: {quantityText}",

                    FontSize =
                        11
                },
                0,
                0);


            details.Add(
                new Label
                {
                    Text =
                        entry.Type == "Payment"
                            ? $"Credit: {creditText}"
                            : $"Debit: {debitText}",

                    FontAttributes =
                        FontAttributes.Bold,

                    FontSize =
                        11
                },
                1,
                0);


            layout.Children.Add(
                details);


            layout.Children.Add(
                new Label
                {
                    Text =
                        $"Balance: ₹{entry.RunningBalance:0.00}",

                    FontSize =
                        11,

                    Opacity =
                        0.7
                });


            card.Content =
                layout;


            CustomerLedgerList.Children.Add(
                card);
        }
    }


    // =========================================================
    // DAILY REPORT RENDER
    // =========================================================

    private void RenderDailyReport(
        DailyReportResponse report)
    {
        HideAllResults();

        DailyReportResult.IsVisible =
            true;


        DailyReportTitleLabel.Text =
            report.Date.ToString(
                "dd MMM yyyy");


        DailyCowLabel.Text =
            $"{report.TotalCowQuantity:0.00} L";


        DailyBuffaloLabel.Text =
            $"{report.TotalBuffaloQuantity:0.00} L";


        DailyTotalQuantityLabel.Text =
            $"{report.TotalQuantity:0.00} L";


        DailyTotalAmountLabel.Text =
            $"₹{report.TotalAmount:0.00}";


        DailyCustomerCountLabel.Text =
            report.CustomerCount.ToString();


        DailyMorningLabel.Text =
            $"{report.MorningTotalQuantity:0.00} L";

        DailyMorningAmountLabel.Text =
            $"₹{report.MorningAmount:0.00}";


        DailyEveningLabel.Text =
            $"{report.EveningTotalQuantity:0.00} L";

        DailyEveningAmountLabel.Text =
            $"₹{report.EveningAmount:0.00}";


        DailyCustomerList.Children.Clear();


        foreach (var customer in report.Customers)
        {
            DailyCustomerList.Children.Add(
                CreateSimpleReportCard(
                    customer.CustomerCode,
                    customer.CustomerName,
                    $"Cow: {customer.CowQuantity:0.00} L   " +
                    $"Buffalo: {customer.BuffaloQuantity:0.00} L",
                    $"Total: {customer.TotalQuantity:0.00} L   " +
                    $"₹{customer.TotalAmount:0.00}"));
        }


        DailyEmptyLabel.IsVisible =
            report.Customers.Count == 0;
    }


    // =========================================================
    // MONTHLY REPORT RENDER
    // =========================================================

    private void RenderMonthlyReport(
        MonthlyReportResponse report)
    {
        HideAllResults();

        MonthlyReportResult.IsVisible =
            true;


        MonthlyReportTitleLabel.Text =
            report.Month.ToString(
                "MMMM yyyy");


        MonthlyCowLabel.Text =
            $"{report.CowQuantity:0.00} L";


        MonthlyBuffaloLabel.Text =
            $"{report.BuffaloQuantity:0.00} L";


        MonthlyTotalQuantityLabel.Text =
            $"{report.TotalQuantity:0.00} L";


        MonthlyTotalAmountLabel.Text =
            $"₹{report.TotalAmount:0.00}";


        MonthlyCustomerCountLabel.Text =
            report.CustomerCount.ToString();


        MonthlyDaysLabel.Text =
            report.DaysRecorded.ToString();


        MonthlyCustomerList.Children.Clear();


        foreach (var customer in report.Customers)
        {
            MonthlyCustomerList.Children.Add(
                CreateSimpleReportCard(
                    customer.CustomerCode,
                    customer.CustomerName,
                    $"Cow: {customer.CowQuantity:0.00} L   " +
                    $"Buffalo: {customer.BuffaloQuantity:0.00} L",
                    $"Total: {customer.TotalQuantity:0.00} L   " +
                    $"₹{customer.TotalAmount:0.00}"));
        }


        MonthlyEmptyLabel.IsVisible =
            report.Customers.Count == 0;
    }


    // =========================================================
    // PENDING REPORT RENDER
    // =========================================================

    private void RenderPendingReport(
        PendingAmountReportResponse report)
    {
        HideAllResults();

        PendingReportResult.IsVisible =
            true;


        PendingReportTitleLabel.Text =
            $"As of {report.AsOfDate:dd MMM yyyy}";


        PendingCustomerCountLabel.Text =
            report.CustomerCount.ToString();


        PendingTotalAmountLabel.Text =
            $"₹{report.TotalPendingAmount:0.00}";


        PendingCustomerList.Children.Clear();


        foreach (var customer in report.Customers)
        {
            var card =
                CreateSimpleReportCard(
                    customer.CustomerCode,
                    customer.CustomerName,
                    $"Bill: {customer.BillNumber}",
                    $"Pending: ₹{customer.PendingAmount:0.00}");

            PendingCustomerList.Children.Add(
                card);
        }


        PendingEmptyLabel.IsVisible =
            report.Customers.Count == 0;
    }


    // =========================================================
    // PAYMENTS REPORT RENDER
    // =========================================================

    private void RenderPaymentsReport(
        AllPaymentReportResponse report)
    {
        HideAllResults();

        PaymentsReportResult.IsVisible =
            true;


        PaymentsReportTitleLabel.Text =
            $"{report.FromDate:dd MMM yyyy} - " +
            $"{report.ToDate:dd MMM yyyy}";


        PaymentsTotalAmountLabel.Text =
            $"₹{report.TotalPaymentAmount:0.00}";


        PaymentsCountLabel.Text =
            report.PaymentCount.ToString();


        PaymentsList.Children.Clear();


        foreach (var payment in report.Payments)
        {
            var details =
                $"Bill: {payment.BillNumber}\n" +
                $"Date: {payment.PaymentDate:dd MMM yyyy}\n" +
                $"Mode: {payment.PaymentMode}";


            if (!string.IsNullOrWhiteSpace(
                    payment.ReferenceNumber))
            {
                details +=
                    $"\nRef: {payment.ReferenceNumber}";
            }


            PaymentsList.Children.Add(
                CreateSimpleReportCard(
                    payment.CustomerCode,
                    payment.CustomerName,
                    details,
                    $"₹{payment.Amount:0.00}"));
        }


        PaymentsEmptyLabel.IsVisible =
            report.Payments.Count == 0;
    }


    // =========================================================
    // SIMPLE REPORT CARD
    // =========================================================

    private Border CreateSimpleReportCard(
        string title,
        string subtitle,
        string details,
        string total)
    {
        var card =
            new Border
            {
                Padding =
                    new Thickness(12),

                StrokeThickness =
                    1,

                Stroke =
                    Application.Current?
                        .Resources["BorderLight"]
                        as Color
                    ?? Colors.LightGray,

                BackgroundColor =
                    Application.Current?
                        .Resources["CardLight"]
                        as Color
                    ?? Colors.White,

                StrokeShape =
                    new RoundRectangle
                    {
                        CornerRadius = 12
                    }
            };


        var layout =
            new Grid
            {
                ColumnDefinitions =
                    new ColumnDefinitionCollection
                    {
                        new ColumnDefinition
                        {
                            Width = GridLength.Star
                        },

                        new ColumnDefinition
                        {
                            Width = GridLength.Auto
                        }
                    },

                RowDefinitions =
                    new RowDefinitionCollection
                    {
                        new RowDefinition
                        {
                            Height = GridLength.Auto
                        },

                        new RowDefinition
                        {
                            Height = GridLength.Auto
                        },

                        new RowDefinition
                        {
                            Height = GridLength.Auto
                        }
                    }
            };


        layout.Add(
            new Label
            {
                Text =
                    title,

                FontAttributes =
                    FontAttributes.Bold,

                FontSize =
                    13
            },
            0,
            0);


        layout.Add(
            new Label
            {
                Text =
                    total,

                FontAttributes =
                    FontAttributes.Bold,

                FontSize =
                    13
            },
            1,
            0);


        layout.Add(
    new Label
    {
        Text = subtitle,
        FontSize = 12,
        Opacity = 0.75
    },
    0,
    1);

        layout.Add(
            new Label
            {
                Text = details,
                FontSize = 11,
                Opacity = 0.7
            },
            0,
            2);


        card.Content =
            layout;


        return card;
    }


    // =========================================================
    // RESULT VISIBILITY
    // =========================================================

    private void HideAllResults()
    {
        CustomerLedgerResult.IsVisible =
            false;

        DailyReportResult.IsVisible =
            false;

        MonthlyReportResult.IsVisible =
            false;

        PendingReportResult.IsVisible =
            false;

        PaymentsReportResult.IsVisible =
            false;

        EmptyResult.IsVisible =
            false;
    }


    private void ShowEmpty(
        string message)
    {
        HideAllResults();

        EmptyResult.IsVisible =
            true;

        EmptyResultLabel.Text =
            message;
    }


    // =========================================================
    // BUTTON HELPERS
    // =========================================================

    private void SetLoading(
        Button button,
        string text)
    {
        if (_isLoading)
            return;

        _isLoading = true;

        button.Text =
            text;

        button.IsEnabled =
            false;
    }


    private void ResetButton(
        Button button,
        string text)
    {
        button.Text =
            text;

        button.IsEnabled =
            true;

        _isLoading =
            false;
    }


    // =========================================================
    // SESSION
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
    // BACK
    // =========================================================

    private async void BackClicked(
        object? sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }


    // =========================================================
    // CUSTOMER OPTION
    // =========================================================

    private sealed class CustomerOption
    {
        public int Id { get; init; }

        public string Code { get; init; } =
            string.Empty;

        public string Name { get; init; } =
            string.Empty;

        public string Mobile { get; init; } =
            string.Empty;
    }
}