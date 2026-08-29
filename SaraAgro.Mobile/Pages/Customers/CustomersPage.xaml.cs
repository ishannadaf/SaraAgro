using Microsoft.Maui.Controls.Shapes;
using SaraAgro.Mobile.Services.Api.Customer;
using SaraAgro.Mobile.Services.Api.RateGroup;
using System.Globalization;

namespace SaraAgro.Mobile.Pages.Customers;

public partial class CustomersPage : ContentPage
{
    // =========================================================
    // PREVIEW MODELS
    // =========================================================

    private sealed class RateGroup
    {
        public int Id { get; set; }

        public string RouteName { get; set; } = string.Empty;

        public string MilkType { get; set; } = string.Empty;

        public decimal Rate { get; set; }

        public DateTime EffectiveDate { get; set; }

        public string DisplayName =>
            $"{RouteName} • {MilkType} • ₹{Rate:0.00}/L";
    }


    private sealed class CustomerItem
    {
        public int Id { get; set; }

        public string CustomerCode { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string MobileNumber { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public int RateGroupId { get; set; }

        public bool IsActive { get; set; }

        public RateGroup RateGroup { get; set; } = null!;
    }


    // =========================================================
    // PREVIEW CUSTOMERS
    // =========================================================


    private string _selectedRoute = "All Routes";

    private string _selectedMilkType = "All Types";


    // =========================================================
    // CONSTRUCTOR
    // =========================================================

    private readonly CustomerApiService _customerApiService;

    private readonly RateGroupApiService _rateGroupApiService;

    private bool _isLoading;
    private readonly List<CustomerItem> _customers = new();

    private readonly List<RateGroup> _rateGroups = new();


    public CustomersPage(
        CustomerApiService customerApiService,
        RateGroupApiService rateGroupApiService)
    {
        InitializeComponent();

        _customerApiService =
            customerApiService;

        _rateGroupApiService =
            rateGroupApiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadCustomersAsync();
    }

    private async Task LoadCustomersAsync()
    {
        if (_isLoading)
            return;

        try
        {
            _isLoading = true;
            
    await LoadRateGroupsAsync();

            var customers =
                await _customerApiService
                    .GetCustomersAsync();

            _customers.Clear();

            foreach (var customer in customers)
            {
                _customers.Add(
                    new CustomerItem
                    {
                        Id =
                            customer.Id,

                        CustomerCode =
                            customer.CustomerCode,

                        FullName =
                            customer.FullName,

                        MobileNumber =
                            customer.MobileNumber,

                        Address =
                            customer.Address,

                        RateGroupId =
                            customer.RateGroupId,

                        IsActive =
                            customer.IsActive,

                        RateGroup =
                            _rateGroups.FirstOrDefault(
                                x => x.Id == customer.RateGroupId)
                            ?? new RateGroup
                            {
                                Id =
                                    customer.RateGroupId,

                                RouteName =
                                    customer.RateGroupName,

                                MilkType =
                                    string.Empty,

                                Rate = 0,

                                EffectiveDate =
                                    DateTime.Today
                            }
                    });
            }

            InitializeFilters();

            RefreshCustomers();
        }
        catch (UnauthorizedAccessException ex)
        {
            await DisplayAlertAsync(
                "Session Expired",
                ex.Message,
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Unable to Load Customers",
                ex.Message,
                "OK");
        }
        finally
        {
            _isLoading = false;
        }
    }

    // =========================================================
    // CONNECT RATE GROUP REFERENCES
    // =========================================================

    private async Task LoadRateGroupsAsync()
    {
        try
        {
            var groups =
                await _rateGroupApiService
                    .GetRateGroupsAsync();

            _rateGroups.Clear();

            foreach (var group in groups)
            {
                if (!group.IsActive)
                    continue;

                _rateGroups.Add(
                    new RateGroup
                    {
                        Id =
                            group.Id,

                        RouteName =
                            group.Name,

                        MilkType =
                            string.Empty,

                        Rate = 0,

                        EffectiveDate =
                            DateTime.Today
                    });
            }
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
    }


    // =========================================================
    // FILTERS
    // =========================================================

    private void InitializeFilters()
    {
        RouteFilterPicker.Items.Clear();

        RouteFilterPicker.Items.Add("All Routes");

        foreach (var route in _rateGroups
                     .Select(x => x.RouteName)
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .OrderBy(x => x))
        {
            RouteFilterPicker.Items.Add(route);
        }


        MilkTypeFilterPicker.Items.Clear();

        MilkTypeFilterPicker.Items.Add("All Types");
        MilkTypeFilterPicker.Items.Add("Cow");
        MilkTypeFilterPicker.Items.Add("Buffalo");


        RouteFilterPicker.SelectedIndex = 0;

        MilkTypeFilterPicker.SelectedIndex = 0;
    }


    // =========================================================
    // REFRESH CUSTOMERS
    // =========================================================

    private void RefreshCustomers()
    {
        CustomerList.Children.Clear();


        IEnumerable<CustomerItem> filtered =
            _customers;


        // -----------------------------------------------------
        // SEARCH
        // -----------------------------------------------------

        var search =
            SearchEntry.Text?.Trim();


        if (!string.IsNullOrWhiteSpace(search))
        {
            filtered =
                filtered.Where(x =>
                    x.FullName.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase)

                    ||

                    x.CustomerCode.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase)

                    ||

                    x.MobileNumber.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase));
        }


        // -----------------------------------------------------
        // ROUTE
        // -----------------------------------------------------

        if (_selectedRoute != "All Routes")
        {
            filtered =
                filtered.Where(
                    x => string.Equals(
                        x.RateGroup.RouteName,
                        _selectedRoute,
                        StringComparison.OrdinalIgnoreCase));
        }


        // -----------------------------------------------------
        // MILK TYPE
        // -----------------------------------------------------

        if (_selectedMilkType != "All Types")
        {
            filtered =
                filtered.Where(
                    x => string.Equals(
                        x.RateGroup.MilkType,
                        _selectedMilkType,
                        StringComparison.OrdinalIgnoreCase));
        }


        var customers =
            filtered
                .OrderBy(x => x.FullName)
                .ToList();


        CustomerCountLabel.Text =
            customers.Count == 1
                ? "1 customer"
                : $"{customers.Count} customers";


        EmptyState.IsVisible =
            customers.Count == 0;


        foreach (var customer in customers)
        {
            CustomerList.Children.Add(
                CreateCustomerCard(customer));
        }
    }


    // =========================================================
    // CUSTOMER CARD
    // =========================================================

    private View CreateCustomerCard(
        CustomerItem customer)
    {
        bool isDark =
            Application.Current?.RequestedTheme ==
            AppTheme.Dark;


        bool isCow =
            string.Equals(
                customer.RateGroup.MilkType,
                "Cow",
                StringComparison.OrdinalIgnoreCase);


        // -----------------------------------------------------
        // STATUS
        // -----------------------------------------------------

        var statusColor =
            customer.IsActive
                ? isDark
                    ? Color.Parse("#8BD79B")
                    : Color.Parse("#087A38")
                : isDark
                    ? Color.Parse("#FFB4AB")
                    : Color.Parse("#BA1A1A");


        var statusText =
            customer.IsActive
                ? "Active"
                : "Inactive";


        var statusDot =
            new Label
            {
                Text = "●",
                FontSize = 10,
                TextColor = statusColor
            };


        var statusLabel =
            new Label
            {
                Text = statusText,
                FontSize = 10,
                FontAttributes = FontAttributes.Bold,
                TextColor = statusColor
            };


        var statusLayout =
            new HorizontalStackLayout
            {
                Spacing = 3,
                HorizontalOptions = LayoutOptions.End
            };


        statusLayout.Children.Add(statusDot);
        statusLayout.Children.Add(statusLabel);


        // -----------------------------------------------------
        // CUSTOMER CODE
        // -----------------------------------------------------

        var codeLabel =
            new Label
            {
                Text = customer.CustomerCode,
                FontSize = 10,
                FontAttributes = FontAttributes.Bold,
                TextColor =
                    isDark
                        ? Color.Parse("#8BD79B")
                        : Color.Parse("#004C22")
            };


        // -----------------------------------------------------
        // NAME
        // -----------------------------------------------------

        var nameLabel =
            new Label
            {
                Text = customer.FullName,
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor =
                    isDark
                        ? Color.Parse("#F2F5F1")
                        : Color.Parse("#141B2B")
            };


        // -----------------------------------------------------
        // MOBILE
        // -----------------------------------------------------

        var mobileLabel =
            new Label
            {
                Text = $"📱  {customer.MobileNumber}",
                FontSize = 11,
                TextColor =
                    isDark
                        ? Color.Parse("#C9D1CA")
                        : Color.Parse("#404940")
            };


        // -----------------------------------------------------
        // ADDRESS
        // -----------------------------------------------------

        var addressLabel =
            new Label
            {
                Text =
                    $"📍  {customer.Address}",

                FontSize = 11,

                TextColor =
                    isDark
                        ? Color.Parse("#C9D1CA")
                        : Color.Parse("#404940")
            };


        // -----------------------------------------------------
        // MILK TYPE
        // -----------------------------------------------------

        //var milkLabel =
        //    new Label
        //    {
        //        Text =
        //            isCow
        //                ? "🐄  Cow Milk"
        //                : "🐃  Buffalo Milk",

        //        FontSize = 11,

        //        FontAttributes =
        //            FontAttributes.Bold,

        //        TextColor =
        //            isDark
        //                ? Color.Parse("#8BD79B")
        //                : Color.Parse("#004C22")
        //    };


        //// -----------------------------------------------------
        //// RATE
        //// -----------------------------------------------------

        //var rateLabel =
        //    new Label
        //    {
        //        Text =
        //            $"₹{customer.RateGroup.Rate.ToString(
        //                "0.00",
        //                CultureInfo.InvariantCulture)}/L",

        //        FontSize = 13,

        //        FontAttributes =
        //            FontAttributes.Bold,

        //        TextColor =
        //            isDark
        //                ? Color.Parse("#F2F5F1")
        //                : Color.Parse("#141B2B")
        //    };


        // -----------------------------------------------------
        // EDIT
        // -----------------------------------------------------

        var editButton =
            new Button
            {
                Text = "Edit",
                HeightRequest = 34,
                Padding = new Thickness(10, 0),
                CornerRadius = 9,
                BackgroundColor = Colors.Transparent,
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                TextColor =
                    isDark
                        ? Color.Parse("#8BD79B")
                        : Color.Parse("#004C22")
            };


        editButton.Clicked +=
            async (_, _) =>
                await ShowCustomerFormAsync(customer);


        // -----------------------------------------------------
        // STATUS ACTION
        // -----------------------------------------------------

        var statusButton =
            new Button
            {
                Text =
                    customer.IsActive
                        ? "Deactivate"
                        : "Activate",

                HeightRequest = 34,

                Padding =
                    new Thickness(8, 0),

                CornerRadius = 9,

                BackgroundColor =
                    Colors.Transparent,

                FontSize = 11,

                FontAttributes =
                    FontAttributes.Bold,

                TextColor =
                    customer.IsActive
                        ? isDark
                            ? Color.Parse("#FFB4AB")
                            : Color.Parse("#BA1A1A")
                        : isDark
                            ? Color.Parse("#8BD79B")
                            : Color.Parse("#004C22")
            };


        statusButton.Clicked +=
            async (_, _) =>
                await ToggleCustomerStatusAsync(customer);


        // -----------------------------------------------------
        // ACTIONS
        // -----------------------------------------------------

        var actions =
            new HorizontalStackLayout
            {
                Spacing = 0,
                HorizontalOptions = LayoutOptions.End
            };


        actions.Children.Add(editButton);
        actions.Children.Add(statusButton);


        // -----------------------------------------------------
        // TOP GRID
        // -----------------------------------------------------

        var topGrid =
            new Grid
            {
                ColumnDefinitions =
                    new ColumnDefinitionCollection
                    {
                        new ColumnDefinition(
                            new GridLength(
                                1,
                                GridUnitType.Star)),

                        new ColumnDefinition(
                            GridLength.Auto)
                    }
            };


        var identity =
            new VerticalStackLayout
            {
                Spacing = 2
            };


        identity.Children.Add(codeLabel);
        identity.Children.Add(nameLabel);


        Grid.SetColumn(identity, 0);
        Grid.SetColumn(statusLayout, 1);


        topGrid.Children.Add(identity);
        topGrid.Children.Add(statusLayout);


        // -----------------------------------------------------
        // RATE ROW
        // -----------------------------------------------------

        //var rateRow =
        //    new Grid
        //    {
        //        ColumnDefinitions =
        //            new ColumnDefinitionCollection
        //            {
        //                new ColumnDefinition(
        //                    new GridLength(
        //                        1,
        //                        GridUnitType.Star)),

        //                new ColumnDefinition(
        //                    GridLength.Auto)
        //            },

        //        Margin = new Thickness(
        //            0,
        //            7,
        //            0,
        //            0)
        //    };


        //Grid.SetColumn(milkLabel, 0);
        //Grid.SetColumn(rateLabel, 1);


        //rateRow.Children.Add(milkLabel);
        //rateRow.Children.Add(rateLabel);


        // -----------------------------------------------------
        // MAIN LAYOUT
        // -----------------------------------------------------

        var content =
            new VerticalStackLayout
            {
                Spacing = 5
            };


        content.Children.Add(topGrid);
        content.Children.Add(mobileLabel);
        content.Children.Add(addressLabel);
        //content.Children.Add(rateRow);
        content.Children.Add(actions);


        // -----------------------------------------------------
        // CARD
        // -----------------------------------------------------

        var card =
            new Border
            {
                Padding = 14,

                Stroke =
                    isDark
                        ? Color.Parse("#354239")
                        : Color.Parse("#D8E0D8"),

                StrokeThickness = 1,

                StrokeShape =
                    new RoundRectangle
                    {
                        CornerRadius = 16
                    },

                BackgroundColor =
                    isDark
                        ? Color.Parse("#18201B")
                        : Color.Parse("#FFFFFF"),

                Content = content
            };


        return card;
    }


    // =========================================================
    // ADD / EDIT CUSTOMER
    // =========================================================

    private async void AddCustomerTapped(
        object? sender,
        TappedEventArgs e)
    {
        await ShowCustomerFormAsync(null);
    }


    private async Task ShowCustomerFormAsync(
        CustomerItem? existingCustomer)
    {
        bool isEdit =
            existingCustomer != null;

        bool isDark =
            Application.Current?.RequestedTheme ==
            AppTheme.Dark;


        // -----------------------------------------------------
        // CUSTOMER CODE
        // -----------------------------------------------------

        var codeEntry =
            new Entry
            {
                Placeholder =
                    "Example: CUST005",

                Text =
                    existingCustomer?.CustomerCode
                    ?? string.Empty,

                FontSize = 14,

                TextColor =
                    isDark
                        ? Color.Parse("#F2F5F1")
                        : Color.Parse("#141B2B")
            };


        // -----------------------------------------------------
        // NAME
        // -----------------------------------------------------

        var nameEntry =
            new Entry
            {
                Placeholder =
                    "Enter customer name",

                Text =
                    existingCustomer?.FullName
                    ?? string.Empty,

                FontSize = 14,

                TextColor =
                    isDark
                        ? Color.Parse("#F2F5F1")
                        : Color.Parse("#141B2B")
            };


        // -----------------------------------------------------
        // MOBILE
        // -----------------------------------------------------

        var mobileEntry =
            new Entry
            {
                Placeholder =
                    "10-digit mobile number",

                Text =
                    existingCustomer?.MobileNumber
                    ?? string.Empty,

                Keyboard =
                    Keyboard.Numeric,

                MaxLength = 10,

                FontSize = 14,

                TextColor =
                    isDark
                        ? Color.Parse("#F2F5F1")
                        : Color.Parse("#141B2B")
            };


        // -----------------------------------------------------
        // ADDRESS
        // -----------------------------------------------------

        var addressEditor =
            new Editor
            {
                Placeholder =
                    "Enter address",

                Text =
                    existingCustomer?.Address
                    ?? string.Empty,

                HeightRequest = 80,

                AutoSize =
                    EditorAutoSizeOption.TextChanges,

                FontSize = 14,

                TextColor =
                    isDark
                        ? Color.Parse("#F2F5F1")
                        : Color.Parse("#141B2B")
            };


        // -----------------------------------------------------
        // RATE GROUP
        // -----------------------------------------------------

        var ratePicker =
            new Picker
            {
                Title =
                    "Select rate group",

                FontSize = 14,

                TextColor =
                    isDark
                        ? Color.Parse("#F2F5F1")
                        : Color.Parse("#141B2B")
            };


        foreach (var rateGroup in _rateGroups)
        {
            ratePicker.Items.Add(
                rateGroup.RouteName);
        }


        if (existingCustomer != null)
        {
            var index =
                _rateGroups.FindIndex(
                    x => x.Id ==
                         existingCustomer.RateGroupId);

            if (index >= 0)
            {
                ratePicker.SelectedIndex =
                    index;
            }
        }


        // -----------------------------------------------------
        // ACTIVE
        // -----------------------------------------------------

        var activeSwitch =
            new Switch
            {
                IsToggled =
                    existingCustomer?.IsActive
                    ?? true,

                OnColor =
                    isDark
                        ? Color.Parse("#8BD79B")
                        : Color.Parse("#004C22")
            };


        var activeRow =
            new Grid
            {
                ColumnDefinitions =
                    new ColumnDefinitionCollection
                    {
                        new ColumnDefinition(
                            new GridLength(
                                1,
                                GridUnitType.Star)),

                        new ColumnDefinition(
                            GridLength.Auto)
                    }
            };


        var activeLabel =
            new Label
            {
                Text = "Active Customer",

                FontAttributes =
                    FontAttributes.Bold,

                FontSize = 12,

                VerticalTextAlignment =
                    TextAlignment.Center,

                TextColor =
                    isDark
                        ? Color.Parse("#F2F5F1")
                        : Color.Parse("#141B2B")
            };


        Grid.SetColumn(activeLabel, 0);
        Grid.SetColumn(activeSwitch, 1);


        activeRow.Children.Add(activeLabel);
        activeRow.Children.Add(activeSwitch);


        // -----------------------------------------------------
        // FORM
        // -----------------------------------------------------

        var form =
            new VerticalStackLayout
            {
                Spacing = 8
            };


        AddFormLabel(
            form,
            "Customer Code");

        form.Children.Add(codeEntry);


        AddFormLabel(
            form,
            "Full Name");

        form.Children.Add(nameEntry);


        AddFormLabel(
            form,
            "Mobile Number");

        form.Children.Add(mobileEntry);


        AddFormLabel(
            form,
            "Address");

        form.Children.Add(addressEditor);


        AddFormLabel(
            form,
            "Rate Group");

        form.Children.Add(ratePicker);


        form.Children.Add(
            new BoxView
            {
                HeightRequest = 1,
                BackgroundColor =
                    isDark
                        ? Color.Parse("#354239")
                        : Color.Parse("#D8E0D8"),

                Margin =
                    new Thickness(
                        0,
                        10)
            });


        form.Children.Add(activeRow);


        // -----------------------------------------------------
        // DIALOG
        // -----------------------------------------------------

        var dialog =
            new ContentPage
            {
                BackgroundColor =
                    isDark
                        ? Color.Parse("#0F1411")
                        : Color.Parse("#F8FAF8"),

                Content =
                    new ScrollView
                    {
                        Content =
                            new VerticalStackLayout
                            {
                                Padding = 22,
                                Spacing = 16,

                                Children =
                                {
                                    new Label
                                    {
                                        Text =
                                            isEdit
                                                ? "Edit Customer"
                                                : "Add Customer",

                                        FontAttributes =
                                            FontAttributes.Bold,

                                        FontSize = 22,

                                        TextColor =
                                            isDark
                                                ? Color.Parse("#F2F5F1")
                                                : Color.Parse("#141B2B")
                                    },

                                    form
                                }
                            }
                    }
            };


        // -----------------------------------------------------
        // SAVE
        // -----------------------------------------------------

        var saveButton =
            new Button
            {
                Text =
                    isEdit
                        ? "Update Customer"
                        : "Save Customer",

                HeightRequest = 50,

                CornerRadius = 11,

                FontAttributes =
                    FontAttributes.Bold,

                BackgroundColor =
                    isDark
                        ? Color.Parse("#8BD79B")
                        : Color.Parse("#004C22"),

                TextColor =
                    isDark
                        ? Color.Parse("#06210F")
                        : Colors.White
            };


        // -----------------------------------------------------
        // CANCEL
        // -----------------------------------------------------

        var cancelButton =
            new Button
            {
                Text = "Cancel",

                HeightRequest = 46,

                CornerRadius = 11,

                BackgroundColor =
                    Colors.Transparent,

                FontAttributes =
                    FontAttributes.Bold,

                TextColor =
                    isDark
                        ? Color.Parse("#8BD79B")
                        : Color.Parse("#004C22")
            };


        saveButton.Clicked +=
            async (_, _) =>
            {
                await SaveCustomerAsync(
                    existingCustomer,
                    codeEntry,
                    nameEntry,
                    mobileEntry,
                    addressEditor,
                    ratePicker,
                    activeSwitch);
            };


        cancelButton.Clicked +=
            async (_, _) =>
            {
                await Navigation.PopModalAsync();
            };


        var buttons =
            new VerticalStackLayout
            {
                Spacing = 7,

                Padding =
                    new Thickness(
                        22,
                        0,
                        22,
                        22),

                Children =
                {
                    saveButton,
                    cancelButton
                }
            };


        var pageContent =
            (VerticalStackLayout)
            ((ScrollView)dialog.Content).Content;


        pageContent.Children.Add(buttons);


        await Navigation.PushModalAsync(
            new NavigationPage(dialog));
    }


    // =========================================================
    // FORM LABEL
    // =========================================================

    private static void AddFormLabel(
        VerticalStackLayout layout,
        string text)
    {
        layout.Children.Add(
            new Label
            {
                Text = text,

                FontAttributes =
                    FontAttributes.Bold,

                FontSize = 11,

                Margin =
                    new Thickness(
                        0,
                        6,
                        0,
                        0)
            });
    }


    // =========================================================
    // SAVE CUSTOMER
    // =========================================================

    private async Task SaveCustomerAsync(
        CustomerItem? existingCustomer,
        Entry codeEntry,
        Entry nameEntry,
        Entry mobileEntry,
        Editor addressEditor,
        Picker ratePicker,
        Switch activeSwitch)
    {
        var code =
            codeEntry.Text?.Trim()
            ?? string.Empty;

        var name =
            nameEntry.Text?.Trim()
            ?? string.Empty;

        var mobile =
            mobileEntry.Text?.Trim()
            ?? string.Empty;

        var address =
            addressEditor.Text?.Trim()
            ?? string.Empty;


        // -----------------------------------------------------
        // VALIDATION
        // -----------------------------------------------------

        if (string.IsNullOrWhiteSpace(code) &&
            existingCustomer == null)
        {
            await DisplayAlertAsync(
                "Customer Code",
                "Please enter customer code.",
                "OK");

            codeEntry.Focus();

            return;
        }


        if (string.IsNullOrWhiteSpace(name))
        {
            await DisplayAlertAsync(
                "Customer Name",
                "Please enter customer name.",
                "OK");

            nameEntry.Focus();

            return;
        }


        if (mobile.Length != 10 ||
            !mobile.All(char.IsDigit))
        {
            await DisplayAlertAsync(
                "Mobile Number",
                "Please enter a valid 10-digit mobile number.",
                "OK");

            mobileEntry.Focus();

            return;
        }


        if (ratePicker.SelectedIndex < 0 ||
            ratePicker.SelectedIndex >= _rateGroups.Count)
        {
            await DisplayAlertAsync(
                "Rate Group",
                "Please select a rate group.",
                "OK");

            return;
        }


        var selectedRateGroup =
            _rateGroups[
                ratePicker.SelectedIndex];


        try
        {
            // -------------------------------------------------
            // EDIT
            // -------------------------------------------------

            if (existingCustomer != null)
            {
                await _customerApiService
                    .UpdateCustomerAsync(
                        existingCustomer.Id,
                        selectedRateGroup.Id,
                        name,
                        mobile,
                        address,
                        activeSwitch.IsToggled);


                await Navigation.PopModalAsync();

                await LoadCustomersAsync();

                await DisplayAlertAsync(
                    "Success",
                    "Customer updated successfully.",
                    "OK");

                return;
            }


            // -------------------------------------------------
            // CREATE
            // -------------------------------------------------

            await _customerApiService
                .CreateCustomerAsync(
                    selectedRateGroup.Id,
                    code,
                    name,
                    mobile,
                    address);


            await Navigation.PopModalAsync();

            await LoadCustomersAsync();

            await DisplayAlertAsync(
                "Success",
                "Customer added successfully.",
                "OK");
        }
        catch (UnauthorizedAccessException ex)
        {
            await DisplayAlertAsync(
                "Session Expired",
                ex.Message,
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Unable to Save Customer",
                ex.Message,
                "OK");
        }
    }


    // =========================================================
    // ACTIVATE / DEACTIVATE
    // =========================================================

    private async Task ToggleCustomerStatusAsync(
        CustomerItem customer)
    {
        var newStatus =
            !customer.IsActive;

        var action =
            newStatus
                ? "activate"
                : "deactivate";


        bool confirmed =
            await DisplayAlertAsync(
                newStatus
                    ? "Activate Customer"
                    : "Deactivate Customer",

                $"Are you sure you want to {action} {customer.FullName}?",

                newStatus
                    ? "Activate"
                    : "Deactivate",

                "Cancel");


        if (!confirmed)
            return;


        try
        {
            await _customerApiService
                .UpdateCustomerStatusAsync(
                    customer.Id,
                    newStatus);


            await LoadCustomersAsync();


            await DisplayAlertAsync(
                "Success",
                newStatus
                    ? "Customer activated successfully."
                    : "Customer deactivated successfully.",
                "OK");
        }
        catch (UnauthorizedAccessException ex)
        {
            await DisplayAlertAsync(
                "Session Expired",
                ex.Message,
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Unable to Update Customer",
                ex.Message,
                "OK");
        }
    }


    // =========================================================
    // SEARCH
    // =========================================================

    private void SearchTextChanged(
        object? sender,
        TextChangedEventArgs e)
    {
        RefreshCustomers();
    }


    // =========================================================
    // FILTER EVENTS
    // =========================================================

    private void RouteFilterChanged(
        object? sender,
        EventArgs e)
    {
        if (RouteFilterPicker.SelectedItem
            is string route)
        {
            _selectedRoute = route;

            RefreshCustomers();
        }
    }


    private void MilkTypeFilterChanged(
        object? sender,
        EventArgs e)
    {
        if (MilkTypeFilterPicker.SelectedItem
            is string milkType)
        {
            _selectedMilkType =
                milkType;

            RefreshCustomers();
        }
    }


    // =========================================================
    // NAVIGATION
    // =========================================================

    private async void BackClicked(
        object? sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }


    private async void HomeTapped(
        object? sender,
        TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(
            "DashboardPage");
    }


    private async void DistributionTapped(
        object? sender,
        TappedEventArgs e)
    {
        await DisplayAlertAsync(
            "Distribution",
            "Milk Distribution will be connected next.",
            "OK");
    }


    private async void MoreClicked(
        object? sender,
        EventArgs e)
    {
        await DisplayAlertAsync(
            "Customers",
            "Customer account and other options will be added later.",
            "OK");
    }
}