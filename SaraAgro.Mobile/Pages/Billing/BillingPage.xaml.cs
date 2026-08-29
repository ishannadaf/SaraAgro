using SaraAgro.Mobile.Services.Api.Billing;
using SaraAgro.Mobile.Services.Api.Customer;

namespace SaraAgro.Mobile.Pages.Billing;

public partial class BillingPage : ContentPage
{
    private readonly BillingApiService _billingApiService;
    private readonly CustomerApiService _customerApiService;

    private readonly List<CustomerOption> _allCustomers = new();
    private readonly List<BillListItem> _bills = new();

    private CustomerOption? _selectedCustomer;
    private BillResponse? _selectedBill;

    private bool _isLoading;
    private bool _isSaving;
    private bool _hasAppeared;


    // =========================================================
    // CONSTRUCTOR
    // =========================================================

    public BillingPage(
        BillingApiService billingApiService,
        CustomerApiService customerApiService)
    {
        InitializeComponent();

        _billingApiService =
            billingApiService;

        _customerApiService =
            customerApiService;

        FromDatePicker.Date =
            DateTime.Today.AddDays(-29);

        ToDatePicker.Date =
            DateTime.Today;
    }


    // =========================================================
    // PAGE APPEARING
    // =========================================================

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_hasAppeared)
            return;

        _hasAppeared = true;

        await LoadInitialDataAsync();
    }


    private async Task LoadInitialDataAsync()
    {
        if (_isLoading)
            return;

        try
        {
            _isLoading = true;

            BillsRefreshView.IsRefreshing =
                true;

            await LoadCustomersAsync();

            await LoadBillsAsync();
        }
        catch (UnauthorizedAccessException ex)
        {
            await ShowSessionExpiredAsync(
                ex.Message);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Unable to Load Billing",
                ex.Message,
                "OK");
        }
        finally
        {
            _isLoading = false;

            BillsRefreshView.IsRefreshing =
                false;
        }
    }


    // =========================================================
    // LOAD CUSTOMERS
    // =========================================================

    private async Task LoadCustomersAsync()
    {
        try
        {
            var customers =
                await _customerApiService
                    .GetCustomersAsync();

            _allCustomers.Clear();

            foreach (var customer in customers)
            {
                _allCustomers.Add(
                    new CustomerOption
                    {
                        Id =
                            customer.Id,

                        Code =
                            customer.CustomerCode
                            ?? string.Empty,

                        Name =
                            customer.FullName
                            ?? string.Empty,

                        Mobile =
                            customer.MobileNumber
                            ?? string.Empty
                    });
            }
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Unable to Load Customers",
                ex.Message,
                "OK");
        }
    }


    // =========================================================
    // CUSTOMER SEARCH
    // =========================================================

    private void CustomerSearchTextChanged(
        object? sender,
        TextChangedEventArgs e)
    {
        var search =
            e.NewTextValue?.Trim()
            ?? string.Empty;

        if (string.IsNullOrWhiteSpace(search))
        {
            CustomerSearchResults.ItemsSource =
                Array.Empty<CustomerOption>();

            CustomerSearchResultsBorder.IsVisible =
                false;

            return;
        }

        var results =
            _allCustomers
                .Where(
                    customer =>
                        customer.Code.Contains(
                            search,
                            StringComparison.OrdinalIgnoreCase)

                        ||

                        customer.Name.Contains(
                            search,
                            StringComparison.OrdinalIgnoreCase)

                        ||

                        customer.Mobile.Contains(
                            search,
                            StringComparison.OrdinalIgnoreCase))
                .Take(30)
                .ToList();

        CustomerSearchResults.ItemsSource =
            results;

        CustomerSearchResultsBorder.IsVisible =
            results.Count > 0;
    }


    // =========================================================
    // CUSTOMER SELECTED
    // =========================================================

    private async void CustomerSelected(
        object? sender,
        SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault()
            is not CustomerOption customer)
        {
            return;
        }

        _selectedCustomer =
            customer;

        SelectedCustomerNameLabel.Text =
            $"{customer.Code} - {customer.Name}";

        SelectedCustomerDetailsLabel.Text =
            customer.Mobile;

        SelectedCustomerCard.IsVisible =
            true;

        CustomerSearchResultsBorder.IsVisible =
            false;

        CustomerSearchBar.Text =
            string.Empty;

        CustomerSearchResults.SelectedItem =
            null;

        await LoadBillsAsync();
    }


    // =========================================================
    // CLEAR CUSTOMER
    // =========================================================

    private async void ClearCustomerClicked(
        object? sender,
        EventArgs e)
    {
        _selectedCustomer = null;

        _selectedBill = null;

        SelectedCustomerCard.IsVisible =
            false;

        CurrentBillCard.IsVisible =
            false;

        CustomerSearchBar.Text =
            string.Empty;

        CustomerSearchResults.ItemsSource =
            Array.Empty<CustomerOption>();

        CustomerSearchResultsBorder.IsVisible =
            false;

        await LoadBillsAsync();
    }


    // =========================================================
    // LOAD BILLS
    // =========================================================

    private async Task LoadBillsAsync()
    {
        if (_isLoading)
            return;

        try
        {
            _isLoading = true;

            BillsRefreshView.IsRefreshing =
                true;

            int? customerId =
                _selectedCustomer?.Id;

            var bills =
                await _billingApiService
                    .GetBillsAsync(
                        customerId,
                        null,
                        null);

            _bills.Clear();

            foreach (var bill in bills)
            {
                _bills.Add(
                    CreateBillListItem(bill));
            }

            BillsCollection.ItemsSource =
                null;

            BillsCollection.ItemsSource =
                _bills.ToList();


            // =================================================
            // AUTOMATIC BILL SELECTION
            //
            // IMPORTANT:
            //
            // Do NOT simply select the latest Bill ID.
            //
            // If a customer has:
            //
            // Bill 1 -> ₹111
            // Paid    -> ₹100
            // Balance -> ₹11
            //
            // and another duplicate/old bill exists:
            //
            // Bill 2 -> ₹111
            // Paid    -> ₹0
            //
            // the old code would select Bill 2 because
            // it has the higher ID.
            //
            // We now prioritize the bill which actually
            // contains the customer's outstanding amount.
            // =================================================

            if (_selectedCustomer != null &&
                _bills.Count > 0)
            {
                BillListItem? selectedBill =
                    null;


                // -------------------------------------------------
                // 1. PARTIALLY PAID BILL
                // -------------------------------------------------

                selectedBill =
                    _bills
                        .Where(
                            x =>
                                x.PaidAmount > 0m &&
                                x.BalanceAmount > 0m)
                        .OrderByDescending(
                            x => x.ToDate)
                        .ThenByDescending(
                            x => x.Id)
                        .FirstOrDefault();


                // -------------------------------------------------
                // 2. ANY BILL WITH OUTSTANDING BALANCE
                // -------------------------------------------------

                selectedBill ??=
                    _bills
                        .Where(
                            x =>
                                x.BalanceAmount > 0m)
                        .OrderByDescending(
                            x => x.ToDate)
                        .ThenByDescending(
                            x => x.Id)
                        .FirstOrDefault();


                // -------------------------------------------------
                // 3. OTHERWISE LATEST BILL
                // -------------------------------------------------

                selectedBill ??=
                    _bills
                        .OrderByDescending(
                            x => x.ToDate)
                        .ThenByDescending(
                            x => x.Id)
                        .FirstOrDefault();


                if (selectedBill != null)
                {
                    await ShowBillAsync(
                        selectedBill.Id);
                }
            }
            else if (_selectedCustomer == null)
            {
                CurrentBillCard.IsVisible =
                    false;
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            await ShowSessionExpiredAsync(
                ex.Message);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Unable to Load Bills",
                ex.Message,
                "OK");
        }
        finally
        {
            _isLoading = false;

            BillsRefreshView.IsRefreshing =
                false;
        }
    }


    // =========================================================
    // BILL SELECTED
    // =========================================================

    private async void BillSelected(
        object? sender,
        SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault()
            is not BillListItem bill)
        {
            return;
        }

        BillsCollection.SelectedItem =
            null;

        await ShowBillAsync(
            bill.Id);
    }


    // =========================================================
    // SHOW BILL
    // =========================================================

    private async Task ShowBillAsync(
        int billId)
    {
        try
        {
            var bill =
                await _billingApiService
                    .GetBillAsync(
                        billId);

            if (bill == null)
            {
                _selectedBill = null;

                CurrentBillCard.IsVisible =
                    false;

                return;
            }

            _selectedBill =
                bill;

            UpdateCurrentBillUI(
                bill);
        }
        catch (UnauthorizedAccessException ex)
        {
            await ShowSessionExpiredAsync(
                ex.Message);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Unable to Load Bill",
                ex.Message,
                "OK");
        }
    }


    // =========================================================
    // UPDATE CURRENT BILL UI
    // =========================================================

    private void UpdateCurrentBillUI(
        BillResponse bill)
    {
        BillNumberLabel.Text =
            bill.BillNumber;

        BillPeriodLabel.Text =
            $"{bill.FromDate:dd MMM yyyy} - " +
            $"{bill.ToDate:dd MMM yyyy}";

        BillStatusLabel.Text =
            bill.Status;

        MilkAmountLabel.Text =
            $"₹{bill.MilkAmount:0.00}";

        PreviousOutstandingLabel.Text =
            $"₹{bill.PreviousOutstanding:0.00}";

        TotalPayableLabel.Text =
            $"₹{bill.TotalPayable:0.00}";

        PaidAmountLabel.Text =
            $"₹{bill.PaidAmount:0.00}";

        BalanceLabel.Text =
            $"₹{bill.BalanceAmount:0.00}";

        CurrentBillCard.IsVisible =
            true;

        AddPaymentButtonState(
            bill.BalanceAmount);
    }


    // =========================================================
    // GENERATE BILL
    // =========================================================

    private async void GenerateBillClicked(
        object? sender,
        EventArgs e)
    {
        await GenerateBillAsync();
    }


    private async Task GenerateBillAsync()
    {
        if (_isSaving)
            return;

        if (_selectedCustomer == null)
        {
            await DisplayAlertAsync(
                "Customer Required",
                "Please search and select a customer first.",
                "OK");

            return;
        }

        var fromDate =
            FromDatePicker.Date ?? DateTime.Now;

        var toDate =
            ToDatePicker.Date ?? DateTime.Now;

        if (toDate < fromDate)
        {
            await DisplayAlertAsync(
                "Invalid Period",
                "To date cannot be earlier than From date.",
                "OK");

            return;
        }

        try
        {
            _isSaving = true;

            GenerateBillButton.IsEnabled =
                false;

            GenerateBillButton.Text =
                "Generating...";


            var bill =
                await _billingApiService
                    .GenerateBillAsync(
                        _selectedCustomer.Id,
                        fromDate,
                        toDate);


            _selectedBill =
                bill;

            UpdateCurrentBillUI(
                bill);


            await LoadBillsAsync();


            await DisplayAlertAsync(
                "Bill Generated",
                $"{bill.BillNumber} generated successfully.",
                "OK");
        }
        catch (UnauthorizedAccessException ex)
        {
            await ShowSessionExpiredAsync(
                ex.Message);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Unable to Generate Bill",
                ex.Message,
                "OK");
        }
        finally
        {
            _isSaving = false;

            GenerateBillButton.IsEnabled =
                true;

            GenerateBillButton.Text =
                "Generate Bill";
        }
    }


    // =========================================================
    // ADD PAYMENT
    // =========================================================

    private async void AddPaymentClicked(
        object? sender,
        EventArgs e)
    {
        if (_selectedBill == null)
            return;

        if (_selectedBill.BalanceAmount <= 0)
        {
            await DisplayAlertAsync(
                "Bill Paid",
                "This bill has no outstanding balance.",
                "OK");

            return;
        }

        await ShowAddPaymentDialogAsync();
    }


    private async Task ShowAddPaymentDialogAsync()
    {
        if (_selectedBill == null)
            return;

        decimal maxAmount =
            _selectedBill.BalanceAmount;


        var amountText =
            await DisplayPromptAsync(
                "Add Payment",
                $"Outstanding: ₹{maxAmount:0.00}\n\nEnter payment amount:",
                "Continue",
                "Cancel",
                "Amount",
                maxLength: 12,
                keyboard: Keyboard.Numeric);


        if (string.IsNullOrWhiteSpace(amountText))
            return;


        if (!decimal.TryParse(
                amountText,
                out decimal amount))
        {
            await DisplayAlertAsync(
                "Invalid Amount",
                "Please enter a valid payment amount.",
                "OK");

            return;
        }


        if (amount <= 0)
        {
            await DisplayAlertAsync(
                "Invalid Amount",
                "Payment amount must be greater than zero.",
                "OK");

            return;
        }


        if (amount > maxAmount)
        {
            await DisplayAlertAsync(
                "Invalid Amount",
                $"Payment cannot exceed ₹{maxAmount:0.00}.",
                "OK");

            return;
        }


        var paymentMode =
            await DisplayActionSheetAsync(
                "Payment Mode",
                "Cancel",
                null,
                "Cash",
                "UPI",
                "Bank",
                "Cheque",
                "Other");


        if (string.IsNullOrWhiteSpace(paymentMode) ||
            paymentMode == "Cancel")
        {
            return;
        }


        var notes =
            await DisplayPromptAsync(
                "Payment Notes",
                "Optional notes:",
                "Save Payment",
                "Cancel",
                "Notes",
                maxLength: 200);


        if (notes == null)
            return;


        try
        {
            _isSaving = true;

            var billId =
                _selectedBill.Id;


            var payment =
                await _billingApiService
                    .AddPaymentAsync(
                        billId,
                        DateTime.Today,
                        amount,
                        paymentMode,
                        string.Empty,
                        notes);


            // =================================================
            // IMPORTANT
            //
            // Always reload the selected bill from API.
            // This ensures PaidAmount, BalanceAmount and
            // Status come from database, not stale UI data.
            // =================================================

            await ShowBillAsync(
                billId);


            // Reload the list after payment.
            await LoadBillsAsync();


            // IMPORTANT:
            // LoadBillsAsync() may select another bill if
            // duplicate/old records exist.
            //
            // Force the exact bill we just paid to be shown.
            await ShowBillAsync(
                billId);


            await DisplayAlertAsync(
                "Payment Saved",
                $"₹{payment.Amount:0.00} payment recorded successfully.",
                "OK");
        }
        catch (UnauthorizedAccessException ex)
        {
            await ShowSessionExpiredAsync(
                ex.Message);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Unable to Save Payment",
                ex.Message,
                "OK");
        }
        finally
        {
            _isSaving = false;
        }
    }


    // =========================================================
    // PAYMENT HISTORY
    // =========================================================

    private async void ViewPaymentHistoryClicked(
        object? sender,
        EventArgs e)
    {
        if (_selectedBill == null)
            return;

        try
        {
            var payments =
                await _billingApiService
                    .GetPaymentsAsync(
                        _selectedBill.Id);


            if (payments == null ||
                payments.Count == 0)
            {
                await DisplayAlertAsync(
                    "Payment History",
                    "No payments have been recorded for this bill.",
                    "OK");

                return;
            }


            var lines =
                payments
                    .OrderByDescending(
                        x => x.PaymentDate)
                    .Select(
                        x =>
                            $"{x.PaymentDate:dd MMM yyyy}  •  " +
                            $"₹{x.Amount:0.00}  •  " +
                            $"{x.PaymentMode}");


            await DisplayAlertAsync(
                "Payment History",
                string.Join(
                    Environment.NewLine,
                    lines),
                "Close");
        }
        catch (UnauthorizedAccessException ex)
        {
            await ShowSessionExpiredAsync(
                ex.Message);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Unable to Load Payments",
                ex.Message,
                "OK");
        }
    }


    // =========================================================
    // REFRESH
    // =========================================================

    private async void BillsRefreshRequested(
        object? sender,
        EventArgs e)
    {
        await LoadBillsAsync();
    }


    // =========================================================
    // BACK
    // =========================================================

    private async void BackClicked(
        object? sender,
        EventArgs e)
    {
        await Navigation.PopAsync();
    }


    // =========================================================
    // PAYMENT BUTTON STATE
    // =========================================================

    private void AddPaymentButtonState(
        decimal balance)
    {
        // Payment button remains visible.
        //
        // AddPaymentClicked() itself prevents payment
        // when the balance is zero.
    }


    // =========================================================
    // BILL LIST MAPPER
    // =========================================================

    private static BillListItem CreateBillListItem(
        BillResponse bill)
    {
        return new BillListItem
        {
            Id =
                bill.Id,

            BillNumber =
                bill.BillNumber,

            CustomerName =
                bill.CustomerName,

            Status =
                bill.Status,

            FromDate =
                bill.FromDate,

            ToDate =
                bill.ToDate,

            PaidAmount =
                bill.PaidAmount,

            BalanceAmount =
                bill.BalanceAmount,

            TotalPayable =
                bill.TotalPayable,

            PeriodText =
                $"{bill.FromDate:dd MMM yyyy} - " +
                $"{bill.ToDate:dd MMM yyyy}",

            TotalPayableText =
                $"₹{bill.TotalPayable:0.00}",

            PaidAmountText =
                $"₹{bill.PaidAmount:0.00}",

            BalanceAmountText =
                $"₹{bill.BalanceAmount:0.00}"
        };
    }


    // =========================================================
    // SESSION EXPIRED
    // =========================================================

    private async Task ShowSessionExpiredAsync(
        string message)
    {
        await DisplayAlertAsync(
            "Session Expired",
            string.IsNullOrWhiteSpace(message)
                ? "Your session has expired. Please login again."
                : message,
            "OK");
    }
}


// =============================================================
// CUSTOMER OPTION
// =============================================================

public sealed class CustomerOption
{
    public int Id { get; set; }

    public string Code { get; set; } =
        string.Empty;

    public string Name { get; set; } =
        string.Empty;

    public string Mobile { get; set; } =
        string.Empty;

    public string Details =>
        string.IsNullOrWhiteSpace(Code)
            ? Mobile
            : $"{Code} • {Mobile}";
}


// =============================================================
// BILL LIST ITEM
// =============================================================

public sealed class BillListItem
{
    public int Id { get; set; }

    public string BillNumber { get; set; } =
        string.Empty;

    public string CustomerName { get; set; } =
        string.Empty;

    public string Status { get; set; } =
        string.Empty;


    // ---------------------------------------------------------
    // IMPORTANT:
    // Keep numeric values in the list item.
    //
    // We need these values to correctly identify the bill
    // that has an outstanding balance.
    // ---------------------------------------------------------

    public decimal TotalPayable { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal BalanceAmount { get; set; }


    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }


    public string PeriodText { get; set; } =
        string.Empty;

    public string TotalPayableText { get; set; } =
        string.Empty;

    public string PaidAmountText { get; set; } =
        string.Empty;

    public string BalanceAmountText { get; set; } =
        string.Empty;
}