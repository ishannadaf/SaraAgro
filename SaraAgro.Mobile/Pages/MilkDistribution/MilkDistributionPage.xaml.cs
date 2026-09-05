using Microsoft.Maui.Controls.Shapes;
using SaraAgro.Mobile.Services.Api.Customer;
using SaraAgro.Mobile.Services.Api.MilkDistribution;
using SaraAgro.Mobile.Services.Api.RateMaster;
using System.Globalization;

namespace SaraAgro.Mobile.Pages.MilkDistribution;

public partial class MilkDistributionPage : ContentPage
{
    private readonly MilkDistributionApiService _milkDistributionApiService;
    private readonly CustomerApiService _customerApiService;
    private readonly RateMasterApiService _rateMasterApiService;
    private bool _suppressSessionReload;

    private readonly List<CustomerOption> _customers = new();
    private readonly List<MilkDistributionResponse> _distributions = new();

    private bool _isLoading;
    private bool _isSaving;
    private bool _isInitializing;

    public MilkDistributionPage(
    MilkDistributionApiService milkDistributionApiService,
    CustomerApiService customerApiService,
    RateMasterApiService rateMasterApiService)
    {
        InitializeComponent();

        _milkDistributionApiService =
            milkDistributionApiService;

        _customerApiService =
            customerApiService;

        _rateMasterApiService =
            rateMasterApiService;

        _isInitializing = true;

        DistributionDatePicker.Date =
            DateTime.Today;

        SessionPicker.SelectedIndex =
            0;

        _isInitializing = false;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await InitializeAndLoadAsync();
    }

    private async Task InitializeAndLoadAsync()
    {
        if (_isLoading)
            return;

        try
        {
            _isInitializing = true;

            DistributionDatePicker.Date =
                DateTime.Today;

            SessionPicker.SelectedIndex = 0;
        }
        finally
        {
            _isInitializing = false;
        }

        await LoadAsync();
    }

    // =========================================================
    // INITIAL LOAD
    // =========================================================

    private async Task LoadAsync()
    {
        if (_isLoading)
            return;

        try
        {
            _isLoading = true;

            DistributionRefreshView.IsRefreshing = true;

            var selectedDate =
                (DistributionDatePicker.Date ?? DateTime.Today).Date;

            var selectedSession =
                GetSelectedSessionCode();

            await LoadCustomersAsync();

            await LoadDistributionsAsync(
                selectedDate,
                selectedSession);

            await LoadSummaryAsync(
                selectedDate);
        }
        catch (UnauthorizedAccessException ex)
        {
            await ShowSessionExpiredAsync(
                ex.Message);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Unable to Load",
                ex.Message,
                "OK");
        }
        finally
        {
            _isLoading = false;

            DistributionRefreshView.IsRefreshing = false;
        }
    }

    // =========================================================
    // LOAD CUSTOMERS
    // =========================================================

    private async Task LoadCustomersAsync()
    {
        var customers =
            await _customerApiService.GetCustomersAsync();

        _customers.Clear();

        foreach (var customer in customers
                     .Where(x => x.IsActive)
                     .OrderBy(x => x.FullName))
        {
            _customers.Add(
                new CustomerOption
                {
                    Id = customer.Id,

                    Code = customer.CustomerCode,

                    Name = customer.FullName,

                    Mobile = customer.MobileNumber,

                    RateGroupId = customer.RateGroupId,

                    RateGroupName =
                        customer.RateGroupName
                });
        }
    }

    // =========================================================
    // LOAD DISTRIBUTIONS
    // =========================================================

    private async Task LoadDistributionsAsync(
    DateTime date,
    string session)
    {
        var search =
            string.IsNullOrWhiteSpace(SearchBar.Text)
                ? null
                : SearchBar.Text.Trim();

        var distributions =
            await _milkDistributionApiService
                .GetDistributionsAsync(
                    date,
                    session,
                    search);

        _distributions.Clear();

        _distributions.AddRange(
            distributions);

        RefreshDistributionList();
    }

    // =========================================================
    // LOAD SUMMARY
    // =========================================================

    private async Task LoadSummaryAsync(
    DateTime date)
    {
        var summary =
            await _milkDistributionApiService
                .GetDailySummaryAsync(
                    date);

        MorningQuantityLabel.Text =
            $"{summary.MorningTotalQuantity:0.00} L";

        MorningAmountLabel.Text =
            $"₹{summary.MorningAmount:0.00}";

        EveningQuantityLabel.Text =
            $"{summary.EveningTotalQuantity:0.00} L";

        EveningAmountLabel.Text =
            $"₹{summary.EveningAmount:0.00}";

        TotalQuantityLabel.Text =
            $"{summary.TotalQuantity:0.00} L";

        TotalAmountLabel.Text =
            $"₹{summary.TotalAmount:0.00}";
    }

    // =========================================================
    // REFRESH DISTRIBUTION LIST
    // =========================================================

    private void RefreshDistributionList()
    {
        DistributionList.Children.Clear();

        var search =
            SearchBar.Text?.Trim();

        IEnumerable<MilkDistributionResponse> filtered =
            _distributions;

        if (!string.IsNullOrWhiteSpace(search))
        {
            filtered =
                filtered.Where(x =>
                    x.CustomerCode.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase)

                    ||

                    x.CustomerName.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase));
        }

        var items =
            filtered
                .OrderBy(x => x.CustomerName)
                .ThenBy(x => x.Session)
                .ToList();

        EmptyState.IsVisible =
            items.Count == 0;

        foreach (var item in items)
        {
            DistributionList.Children.Add(
                CreateDistributionCard(item));
        }
    }

    // =========================================================
    // DISTRIBUTION CARD
    // =========================================================

    private View CreateDistributionCard(
        MilkDistributionResponse item)
    {
        var isDark =
            Application.Current?.RequestedTheme ==
            AppTheme.Dark;

        var sessionText =
            item.Session == "M"
                ? "Morning"
                : "Evening";

        var milkIcon =
            item.MilkType == "Cow"
                ? "🐄"
                : "🐃";

        var card =
            new Border
            {
                Padding = 14,

                StrokeThickness = 1,

                Stroke =
                    isDark
                        ? Color.Parse("#354239")
                        : Color.Parse("#D8E0D8"),

                BackgroundColor =
                    isDark
                        ? Color.Parse("#18201B")
                        : Colors.White,

                StrokeShape =
                    new RoundRectangle
                    {
                        CornerRadius = 16
                    }
            };

        var identity =
            new VerticalStackLayout
            {
                Spacing = 2
            };

        identity.Children.Add(
            new Label
            {
                Text =
                    item.CustomerCode,

                FontSize = 10,

                FontAttributes =
                    FontAttributes.Bold,

                TextColor =
                    isDark
                        ? Color.Parse("#8BD79B")
                        : Color.Parse("#004C22")
            });

        identity.Children.Add(
            new Label
            {
                Text =
                    item.CustomerName,

                FontSize = 16,

                FontAttributes =
                    FontAttributes.Bold
            });

        identity.Children.Add(
            new Label
            {
                Text =
                    $"{sessionText}  •  {milkIcon} {item.MilkType}  •  {item.RateGroupName}",

                FontSize = 11,

                Opacity = 0.7
            });

        var quantityLabel =
            new Label
            {
                Text =
                    $"{item.Quantity:0.00} L",

                FontSize = 18,

                FontAttributes =
                    FontAttributes.Bold,

                HorizontalTextAlignment =
                    TextAlignment.End
            };

        var infoGrid =
            new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(
                        new GridLength(
                            1,
                            GridUnitType.Star)),

                    new ColumnDefinition(
                        GridLength.Auto)
                },

                ColumnSpacing = 12
            };

        Grid.SetColumn(
            identity,
            0);

        Grid.SetColumn(
            quantityLabel,
            1);

        infoGrid.Children.Add(
            identity);

        infoGrid.Children.Add(
            quantityLabel);

        var rateLabel =
            new Label
            {
                Text =
                    $"₹{item.Rate:0.00}/L  •  ₹{item.Amount:0.00}",

                FontSize = 11,

                Opacity = 0.7,

                HorizontalTextAlignment =
                    TextAlignment.End
            };

        var rateRow =
            new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(
                        new GridLength(
                            1,
                            GridUnitType.Star)),

                    new ColumnDefinition(
                        GridLength.Auto)
                },

                Margin =
                    new Thickness(
                        0,
                        8,
                        0,
                        0)
            };

        var groupLabel =
            new Label
            {
                Text =
                    item.RateGroupName,

                FontSize = 11,

                Opacity = 0.65
            };

        Grid.SetColumn(
            groupLabel,
            0);

        Grid.SetColumn(
            rateLabel,
            1);

        rateRow.Children.Add(
            groupLabel);

        rateRow.Children.Add(
            rateLabel);

        var editButton =
            new Button
            {
                Text =
                    "Edit Quantity",

                HeightRequest = 36,

                Padding =
                    new Thickness(
                        12,
                        0),

                CornerRadius = 9,

                BackgroundColor =
                    Colors.Transparent,

                FontSize = 11,

                FontAttributes =
                    FontAttributes.Bold,

                TextColor =
                    isDark
                        ? Color.Parse("#8BD79B")
                        : Color.Parse("#004C22")
            };

        editButton.Clicked +=
            async (_, _) =>
                await EditDistributionAsync(item);

        card.Content =
            new VerticalStackLayout
            {
                Spacing = 4,

                Children =
                {
                    infoGrid,
                    rateRow,
                    editButton
                }
            };

        return card;
    }

    // =========================================================
    // ADD
    // =========================================================

    private async void AddDistributionClicked(
        object? sender,
        EventArgs e)
    {
        await ShowDistributionFormAsync(null);
    }

    // =========================================================
    // ADD / EDIT FORM
    // =========================================================

    private async Task ShowDistributionFormAsync(
        MilkDistributionResponse? existing)
    {
        if (_customers.Count == 0)
        {
            await DisplayAlertAsync(
                "Customers",
                "No active customers are available.",
                "OK");

            return;
        }

        var isEdit =
            existing != null;

        var isDark =
            Application.Current?.RequestedTheme ==
            AppTheme.Dark;

        var background =
            Color.Parse(
                isDark
                    ? "#0F1411"
                    : "#F8FAF8");

        var surface =
            Color.Parse(
                isDark
                    ? "#18201B"
                    : "#FFFFFF");

        var primary =
            Color.Parse(
                isDark
                    ? "#F2F5F1"
                    : "#141B2B");

        var secondary =
            Color.Parse(
                isDark
                    ? "#AAB7AF"
                    : "#667068");

        var borderColor =
            Color.Parse(
                isDark
                    ? "#354239"
                    : "#D8E0D8");

        var accent =
            Color.Parse(
                isDark
                    ? "#8BD79B"
                    : "#004C22");


        // =====================================================
        // SELECTED CUSTOMER
        // =====================================================

        CustomerOption? selectedCustomer =
            null;

        if (existing != null)
        {
            selectedCustomer =
                _customers.FirstOrDefault(
                    x =>
                        x.Id ==
                        existing.CustomerId);
        }


        // =====================================================
        // CUSTOMER DISPLAY
        // =====================================================

        var selectedCustomerLabel =
            new Label
            {
                Text =
                    selectedCustomer == null
                        ? "Select a customer"
                        : $"{selectedCustomer.Code} - {selectedCustomer.Name}",

                FontSize = 14,

                FontAttributes =
                    FontAttributes.Bold,

                TextColor =
                    primary,

                VerticalTextAlignment =
                    TextAlignment.Center
            };


        // =====================================================
        // CUSTOMER SEARCH
        // =====================================================

        var customerSearchBar =
            new SearchBar
            {
                Placeholder =
                    "Search name, code or mobile",

                FontSize = 14,

                TextColor =
                    primary,

                PlaceholderColor =
                    secondary,

                BackgroundColor =
                    surface,

                IsVisible =
                    !isEdit
            };


        var customerCollection =
            new CollectionView
            {
                SelectionMode =
                    SelectionMode.Single,

                HeightRequest = 240,

                BackgroundColor =
                    background,

                IsVisible =
                    !isEdit
            };


        customerCollection.ItemTemplate =
            new DataTemplate(
                () =>
                {
                    var codeLabel =
                        new Label
                        {
                            FontSize = 10,

                            FontAttributes =
                                FontAttributes.Bold,

                            TextColor =
                                accent
                        };

                    codeLabel.SetBinding(
                        Label.TextProperty,
                        nameof(
                            CustomerOption.Code));


                    var nameLabel =
                        new Label
                        {
                            FontSize = 15,

                            FontAttributes =
                                FontAttributes.Bold,

                            TextColor =
                                primary
                        };

                    nameLabel.SetBinding(
                        Label.TextProperty,
                        nameof(
                            CustomerOption.Name));


                    var mobileLabel =
                        new Label
                        {
                            FontSize = 11,

                            TextColor =
                                secondary
                        };

                    mobileLabel.SetBinding(
                        Label.TextProperty,
                        nameof(
                            CustomerOption.Mobile));


                    return new Border
                    {
                        Margin =
                            new Thickness(
                                0,
                                0,
                                0,
                                6),

                        Padding =
                            new Thickness(
                                12,
                                9),

                        Stroke =
                            borderColor,

                        StrokeThickness = 1,

                        BackgroundColor =
                            surface,

                        StrokeShape =
                            new RoundRectangle
                            {
                                CornerRadius = 10
                            },

                        Content =
                            new VerticalStackLayout
                            {
                                Spacing = 1,

                                Children =
                                {
                                    codeLabel,
                                    nameLabel,
                                    mobileLabel
                                }
                            }
                    };
                });


        void RefreshCustomerResults()
        {
            var search =
                customerSearchBar.Text?.Trim();

            IEnumerable<CustomerOption> results =
                _customers;

            if (!string.IsNullOrWhiteSpace(search))
            {
                results =
                    _customers.Where(
                        x =>
                            x.Name.Contains(
                                search,
                                StringComparison.OrdinalIgnoreCase)

                            ||

                            x.Code.Contains(
                                search,
                                StringComparison.OrdinalIgnoreCase)

                            ||

                            x.Mobile.Contains(
                                search,
                                StringComparison.OrdinalIgnoreCase));
            }

            customerCollection.ItemsSource =
                results
                    .OrderBy(x => x.Name)
                    .ToList();
        }


        // =====================================================
        // CUSTOMER INFO
        // =====================================================

        var customerInfo =
            new Label
            {
                FontSize = 11,

                TextColor =
                    secondary,

                Text =
                    selectedCustomer == null
                        ? "Select a customer to load the applicable rate."
                        : $"{selectedCustomer.Mobile}  •  {selectedCustomer.RateGroupName}"
            };


        // =====================================================
        // RATE UI
        // =====================================================

        var rateLabel =
            new Label
            {
                Text =
                    "Rate: Select customer",

                FontSize = 16,

                FontAttributes =
                    FontAttributes.Bold,

                TextColor =
                    accent
            };


        var effectiveDateLabel =
            new Label
            {
                Text =
                    string.Empty,

                FontSize = 11,

                TextColor =
                    secondary
            };


        // =====================================================
        // TOTAL UI
        // =====================================================

        var totalAmountLabel =
            new Label
            {
                Text =
                    "₹0.00",

                FontSize = 23,

                FontAttributes =
                    FontAttributes.Bold,

                TextColor =
                    primary,

                HorizontalTextAlignment =
                    TextAlignment.End
            };


        // =====================================================
        // SESSION
        // =====================================================

        var sessionPicker =
            new Picker
            {
                Title =
                    "Select session",

                FontSize = 14,

                TextColor =
                    primary,

                TitleColor =
                    secondary,

                BackgroundColor =
                    surface,

                IsEnabled =
                    !isEdit
            };

        sessionPicker.Items.Add(
            "Morning");

        sessionPicker.Items.Add(
            "Evening");

        sessionPicker.SelectedIndex =
            existing?.Session == "E"
                ? 1
                : 0;


        // =====================================================
        // MILK TYPE
        // =====================================================

        var milkTypePicker =
            new Picker
            {
                Title =
                    "Select milk type",

                FontSize = 14,

                TextColor =
                    primary,

                TitleColor =
                    secondary,

                BackgroundColor =
                    surface,

                IsEnabled =
                    !isEdit
            };

        milkTypePicker.Items.Add(
            "Cow");

        milkTypePicker.Items.Add(
            "Buffalo");

        milkTypePicker.SelectedIndex =
            string.Equals(
                existing?.MilkType,
                "Buffalo",
                StringComparison.OrdinalIgnoreCase)
                ? 1
                : 0;


        // =====================================================
        // QUANTITY
        // =====================================================

        var quantityEntry =
            new Entry
            {
                Placeholder =
                    "Enter litres",

                Keyboard =
                    Keyboard.Numeric,

                FontSize = 15,

                TextColor =
                    primary,

                PlaceholderColor =
                    secondary,

                BackgroundColor =
                    surface,

                Text =
                    existing == null
                        ? string.Empty
                        : existing.Quantity.ToString(
                            "0.00",
                            CultureInfo.InvariantCulture)
            };


        // =====================================================
        // DATE
        // =====================================================

        var datePicker =
            new DatePicker
            {
                Date =
                    existing?.DistributionDate.Date
                    ?? DistributionDatePicker.Date,

                IsEnabled =
                    !isEdit,

                TextColor =
                    primary,

                BackgroundColor =
                    surface
            };


        // =====================================================
        // RATE CARD
        // =====================================================

        var rateCard =
            new Border
            {
                Padding =
                    new Thickness(
                        14,
                        11),

                StrokeThickness = 1,

                Stroke =
                    borderColor,

                BackgroundColor =
                    surface,

                StrokeShape =
                    new RoundRectangle
                    {
                        CornerRadius = 12
                    },

                Content =
                    new VerticalStackLayout
                    {
                        Spacing = 2,

                        Children =
                        {
                            new Label
                            {
                                Text =
                                    "Applicable Rate",

                                FontSize = 10,

                                FontAttributes =
                                    FontAttributes.Bold,

                                TextColor =
                                    secondary
                            },

                            rateLabel,

                            effectiveDateLabel
                        }
                    }
            };


        // =====================================================
        // TOTAL CARD
        // =====================================================

        var totalCard =
            new Border
            {
                Padding =
                    new Thickness(
                        14,
                        11),

                StrokeThickness = 0,

                BackgroundColor =
                    isDark
                        ? Color.Parse("#193522")
                        : Color.Parse("#E6F4EA"),

                StrokeShape =
                    new RoundRectangle
                    {
                        CornerRadius = 12
                    },

                Content =
                    new Grid
                    {
                        ColumnDefinitions =
                        {
                            new ColumnDefinition(
                                new GridLength(
                                    1,
                                    GridUnitType.Star)),

                            new ColumnDefinition(
                                GridLength.Auto)
                        },

                        Children =
                        {
                            new VerticalStackLayout
                            {
                                Spacing = 2,

                                Children =
                                {
                                    new Label
                                    {
                                        Text =
                                            "Total Amount",

                                        FontSize = 10,

                                        FontAttributes =
                                            FontAttributes.Bold,

                                        TextColor =
                                            secondary
                                    },

                                    new Label
                                    {
                                        Text =
                                            "Quantity × Rate",

                                        FontSize = 10,

                                        TextColor =
                                            secondary
                                    }
                                }
                            },

                            totalAmountLabel
                        }
                    }
            };

        Grid.SetColumn(
            totalAmountLabel,
            1);


        // =====================================================
        // CHANGE CUSTOMER
        // =====================================================

        var changeCustomerButton =
            new Button
            {
                Text =
                    isEdit
                        ? "Locked"
                        : "Change",

                FontSize = 11,

                HeightRequest = 38,

                Padding =
                    new Thickness(
                        12,
                        0),

                CornerRadius = 9,

                BackgroundColor =
                    Colors.Transparent,

                TextColor =
                    accent,

                IsEnabled =
                    !isEdit
            };


        // =====================================================
        // CUSTOMER HEADER
        // =====================================================

        var selectedCustomerRow =
            new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(
                        new GridLength(
                            1,
                            GridUnitType.Star)),

                    new ColumnDefinition(
                        GridLength.Auto)
                },

                Padding =
                    new Thickness(
                        12,
                        8),

                BackgroundColor =
                    surface
            };

        Grid.SetColumn(
            selectedCustomerLabel,
            0);

        Grid.SetColumn(
            changeCustomerButton,
            1);

        selectedCustomerRow.Children.Add(
            selectedCustomerLabel);

        selectedCustomerRow.Children.Add(
            changeCustomerButton);


        // =====================================================
        // CUSTOMER EVENTS
        // =====================================================

        customerCollection.SelectionChanged +=
            (_, args) =>
            {
                if (args.CurrentSelection.FirstOrDefault()
                    is not CustomerOption customer)
                {
                    return;
                }

                selectedCustomer =
                    customer;

                selectedCustomerLabel.Text =
                    $"{customer.Code} - {customer.Name}";

                customerInfo.Text =
                    $"{customer.Mobile}  •  {customer.RateGroupName}";

                customerSearchBar.Text =
                    string.Empty;

                customerCollection.IsVisible =
                    false;

                customerCollection.SelectedItem =
                    null;

                _ = RefreshRateAsync();
            };


        customerSearchBar.TextChanged +=
            (_, _) =>
            {
                RefreshCustomerResults();
            };


        changeCustomerButton.Clicked +=
            (_, _) =>
            {
                if (isEdit)
                    return;

                customerCollection.IsVisible =
                    true;

                customerSearchBar.Focus();

                RefreshCustomerResults();
            };


        // =====================================================
        // CUSTOMER CONTAINER
        // =====================================================

        var customerContainer =
            new VerticalStackLayout
            {
                Spacing = 8,

                Children =
                {
                    selectedCustomerRow,
                    customerSearchBar,
                    customerCollection,
                    customerInfo
                }
            };


        if (!isEdit)
        {
            customerCollection.IsVisible =
                true;

            RefreshCustomerResults();
        }


        // =====================================================
        // FORM
        // =====================================================

        var form =
            new VerticalStackLayout
            {
                Spacing = 8,

                Children =
                {
                    new Label
                    {
                        Text =
                            "Customer",

                        FontSize = 11,

                        FontAttributes =
                            FontAttributes.Bold,

                        TextColor =
                            secondary
                    },

                    customerContainer,


                    new Label
                    {
                        Text =
                            "Date",

                        FontSize = 11,

                        FontAttributes =
                            FontAttributes.Bold,

                        TextColor =
                            secondary
                    },

                    datePicker,


                    new Label
                    {
                        Text =
                            "Session",

                        FontSize = 11,

                        FontAttributes =
                            FontAttributes.Bold,

                        TextColor =
                            secondary
                    },

                    sessionPicker,


                    new Label
                    {
                        Text =
                            "Milk Type",

                        FontSize = 11,

                        FontAttributes =
                            FontAttributes.Bold,

                        TextColor =
                            secondary
                    },

                    milkTypePicker,


                    rateCard,


                    new Label
                    {
                        Text =
                            "Quantity (Litres)",

                        FontSize = 11,

                        FontAttributes =
                            FontAttributes.Bold,

                        TextColor =
                            secondary
                    },

                    quantityEntry,


                    totalCard
                }
            };


        // =====================================================
        // CURRENT RATE
        // =====================================================

        var currentRate =
            0m;


        async Task RefreshRateAsync()
        {
            await LoadApplicableRateAsync(
                selectedCustomer,
                milkTypePicker,
                datePicker,
                rateLabel,
                effectiveDateLabel,
                totalAmountLabel,
                quantityEntry,
                rate =>
                {
                    currentRate =
                        rate;
                });
        }


        // =====================================================
        // MILK TYPE CHANGED
        // =====================================================

        milkTypePicker.SelectedIndexChanged +=
            async (_, _) =>
            {
                await RefreshRateAsync();
            };


        // =====================================================
        // DATE CHANGED
        // =====================================================

        datePicker.DateSelected +=
            async (_, _) =>
            {
                await RefreshRateAsync();
            };


        // =====================================================
        // QUANTITY CHANGED
        // =====================================================

        quantityEntry.TextChanged +=
            (_, _) =>
            {
                UpdateDistributionTotal(
                    quantityEntry,
                    totalAmountLabel,
                    currentRate);
            };


        // =====================================================
        // SAVE BUTTON
        // =====================================================

        var saveButton =
            new Button
            {
                Text =
                    isEdit
                        ? "Update Quantity"
                        : "Save Distribution",

                HeightRequest = 50,

                CornerRadius = 11,

                FontAttributes =
                    FontAttributes.Bold,

                BackgroundColor =
                    accent,

                TextColor =
                    isDark
                        ? Color.Parse("#06210F")
                        : Colors.White
            };


        // =====================================================
        // CANCEL BUTTON
        // =====================================================

        var cancelButton =
            new Button
            {
                Text =
                    "Cancel",

                HeightRequest = 46,

                CornerRadius = 11,

                BackgroundColor =
                    Colors.Transparent,

                FontAttributes =
                    FontAttributes.Bold,

                TextColor =
                    accent
            };


        // =====================================================
        // DIALOG
        // =====================================================

        var dialog =
            new ContentPage
            {
                BackgroundColor =
                    background,

                Title =
                    isEdit
                        ? "Edit Distribution"
                        : "Add Milk Distribution",

                Content =
                    new ScrollView
                    {
                        BackgroundColor =
                            background,

                        Content =
                            new VerticalStackLayout
                            {
                                Padding = 20,

                                Spacing = 14,

                                BackgroundColor =
                                    background,

                                Children =
                                {
                                    new Label
                                    {
                                        Text =
                                            isEdit
                                                ? "Edit Distribution"
                                                : "Add Milk Distribution",

                                        FontAttributes =
                                            FontAttributes.Bold,

                                        FontSize = 22,

                                        TextColor =
                                            primary
                                    },

                                    form,

                                    saveButton,

                                    cancelButton
                                }
                            }
                    }
            };


        // =====================================================
        // SAVE CLICK
        // =====================================================

        saveButton.Clicked +=
            async (_, _) =>
            {
                await SaveDistributionAsync(
                    existing,
                    selectedCustomer,
                    sessionPicker,
                    milkTypePicker,
                    quantityEntry,
                    datePicker,
                    saveButton);
            };


        // =====================================================
        // CANCEL CLICK
        // =====================================================

        cancelButton.Clicked +=
            async (_, _) =>
            {
                await Navigation.PopModalAsync();
            };


        await Navigation.PushModalAsync(
            new NavigationPage(dialog));


        // =====================================================
        // LOAD INITIAL RATE
        // =====================================================

        if (selectedCustomer != null)
        {
            await RefreshRateAsync();
        }
    }

    // =========================================================
    // LOAD APPLICABLE RATE
    // =========================================================

    private async Task LoadApplicableRateAsync(
    CustomerOption? customer,
    Picker milkTypePicker,
    DatePicker datePicker,
    Label rateLabel,
    Label effectiveDateLabel,
    Label totalAmountLabel,
    Entry quantityEntry,
    Action<decimal>? setRate = null)
    {
        rateLabel.Text = "Rate: Loading...";
        effectiveDateLabel.Text = string.Empty;
        totalAmountLabel.Text = "₹0.00";

        setRate?.Invoke(0m);

        if (customer == null)
        {
            rateLabel.Text = "Rate: Select customer";
            return;
        }

        if (customer.RateGroupId <= 0)
        {
            rateLabel.Text = "Rate: Rate group not assigned";

            effectiveDateLabel.Text =
                "This customer does not have a valid rate group.";

            return;
        }

        if (milkTypePicker.SelectedIndex < 0)
        {
            rateLabel.Text = "Rate: Select milk type";
            return;
        }

        var milkType =
            milkTypePicker.SelectedItem?.ToString()?.Trim();

        if (string.IsNullOrWhiteSpace(milkType))
        {
            rateLabel.Text = "Rate: Select milk type";
            return;
        }

        var selectedDate =
            datePicker.Date ?? DateTime.Today;

        var distributionDate =
            DateTime.SpecifyKind(
                selectedDate.Date,
                DateTimeKind.Unspecified);

        try
        {
            // =====================================================
            // IMPORTANT DEBUG VALUES
            // =====================================================

            System.Diagnostics.Debug.WriteLine(
                "========== RATE LOOKUP ==========");

            System.Diagnostics.Debug.WriteLine(
                $"Customer Id      : {customer.Id}");

            System.Diagnostics.Debug.WriteLine(
                $"Customer Code    : {customer.Code}");

            System.Diagnostics.Debug.WriteLine(
                $"Rate Group Id    : {customer.RateGroupId}");

            System.Diagnostics.Debug.WriteLine(
                $"Rate Group Name  : {customer.RateGroupName}");

            System.Diagnostics.Debug.WriteLine(
                $"Milk Type        : {milkType}");

            System.Diagnostics.Debug.WriteLine(
                $"Distribution Date: {distributionDate:yyyy-MM-dd}");

            System.Diagnostics.Debug.WriteLine(
                "=================================");


            var result =
                await _rateMasterApiService
                    .GetApplicableRateAsync(
                        customer.RateGroupId,
                        milkType,
                        distributionDate);


            if (result == null)
            {
                rateLabel.Text =
                    "Rate: Not available";

                effectiveDateLabel.Text =
                    $"No {milkType} rate found for " +
                    $"{customer.RateGroupName}.";

                return;
            }


            // =====================================================
            // RATE FOUND
            // =====================================================

            var rate =
                result.Rate;


            setRate?.Invoke(rate);


            rateLabel.Text =
                $"₹{rate:0.00} / L";


            effectiveDateLabel.Text =
                $"Effective from {result.EffectiveDate:dd MMM yyyy}";


            UpdateDistributionTotal(
                quantityEntry,
                totalAmountLabel,
                rate);
        }
        catch (UnauthorizedAccessException)
        {
            rateLabel.Text =
                "Rate: Session expired";

            effectiveDateLabel.Text =
                "Please login again.";

            await ShowSessionExpiredAsync(
                "Your session has expired. Please login again.");
        }
        catch (Exception ex)
        {
            rateLabel.Text =
                "Rate: Unable to load";

            effectiveDateLabel.Text =
                ex.Message;

            System.Diagnostics.Debug.WriteLine(
                $"RATE LOOKUP ERROR: {ex}");
        }
    }

    // =========================================================
    // LIVE TOTAL
    // =========================================================

    private static void UpdateDistributionTotal(
        Entry quantityEntry,
        Label totalAmountLabel,
        decimal rate)
    {
        if (rate <= 0)
        {
            totalAmountLabel.Text =
                "₹0.00";

            return;
        }


        if (!decimal.TryParse(
                quantityEntry.Text?.Trim(),
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var quantity))
        {
            totalAmountLabel.Text =
                "₹0.00";

            return;
        }


        var total =
            Math.Round(
                quantity * rate,
                2,
                MidpointRounding.AwayFromZero);


        totalAmountLabel.Text =
            $"₹{total:0.00}";
    }

    // =========================================================
    // SAVE / UPDATE
    // =========================================================

    private async Task SaveDistributionAsync(
    MilkDistributionResponse? existing,
    CustomerOption? selectedCustomer,
    Picker sessionPicker,
    Picker milkTypePicker,
    Entry quantityEntry,
    DatePicker datePicker,
    Button saveButton)
    {
        if (_isSaving)
            return;


        // =========================================================
        // VALIDATION
        // =========================================================

        if (existing == null &&
            selectedCustomer == null)
        {
            await DisplayAlertAsync(
                "Customer",
                "Please search and select a customer.",
                "OK");

            return;
        }


        if (sessionPicker.SelectedIndex < 0)
        {
            await DisplayAlertAsync(
                "Session",
                "Please select Morning or Evening.",
                "OK");

            return;
        }


        if (milkTypePicker.SelectedIndex < 0)
        {
            await DisplayAlertAsync(
                "Milk Type",
                "Please select Cow or Buffalo.",
                "OK");

            return;
        }


        if (!decimal.TryParse(
                quantityEntry.Text?.Trim(),
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var quantity))
        {
            await DisplayAlertAsync(
                "Quantity",
                "Please enter a valid quantity.",
                "OK");

            quantityEntry.Focus();

            return;
        }


        quantity =
            Math.Round(
                quantity,
                2,
                MidpointRounding.AwayFromZero);


        if (quantity <= 0)
        {
            await DisplayAlertAsync(
                "Quantity",
                "Quantity must be greater than zero.",
                "OK");

            quantityEntry.Focus();

            return;
        }


        // =========================================================
        // CAPTURE DATE + SESSION BEFORE API CALL
        // =========================================================

        var distributionDate =
            (datePicker.Date ?? DateTime.Today).Date;

        var selectedSession =
            sessionPicker.SelectedIndex == 0
                ? "M"
                : "E";


        try
        {
            _isSaving = true;

            saveButton.IsEnabled = false;

            saveButton.Text =
                existing == null
                    ? "Saving..."
                    : "Updating...";


            // =====================================================
            // CREATE
            // =====================================================

            if (existing == null)
            {
                if (selectedCustomer == null)
                {
                    throw new InvalidOperationException(
                        "Please select a customer.");
                }


                var milkType =
                    milkTypePicker.SelectedIndex == 0
                        ? "Cow"
                        : "Buffalo";


                await _milkDistributionApiService
                    .CreateDistributionAsync(
                        selectedCustomer.Id,
                        distributionDate,
                        selectedSession,
                        milkType,
                        quantity);
            }


            // =====================================================
            // UPDATE
            // =====================================================

            else
            {
                await _milkDistributionApiService
                    .UpdateDistributionAsync(
                        existing.Id,
                        quantity);
            }


            // =====================================================
            // CLOSE MODAL
            // =====================================================

            await Navigation.PopModalAsync();


            // =====================================================
            // SET THE MAIN PAGE DATE + SESSION
            // =====================================================

            _suppressSessionReload = true;

            try
            {
                DistributionDatePicker.Date =
                    distributionDate;

                SessionPicker.SelectedIndex =
                    existing == null
                        ? sessionPicker.SelectedIndex
                        : existing.Session == "E"
                            ? 1
                            : 0;
            }
            finally
            {
                _suppressSessionReload = false;
            }


            // =====================================================
            // IMPORTANT:
            // EXPLICITLY RELOAD USING THE EXACT DATE + SESSION
            //
            // Do NOT call LoadDistributionsAsync() without
            // parameters anymore.
            // =====================================================

            var refreshDate =
                (DistributionDatePicker.Date ?? DateTime.Today).Date;

            var refreshSession =
                GetSelectedSessionCode();


            await LoadDistributionsAsync(
                refreshDate,
                refreshSession);


            // =====================================================
            // REFRESH DAILY SUMMARY
            // =====================================================

            await LoadSummaryAsync(
                refreshDate);


            // =====================================================
            // SUCCESS MESSAGE
            // =====================================================

            await DisplayAlertAsync(
                "Success",
                existing == null
                    ? "Milk distribution saved successfully."
                    : "Milk quantity updated successfully.",
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
                existing == null
                    ? "Unable to Save"
                    : "Unable to Update",

                ex.Message,

                "OK");
        }
        finally
        {
            _isSaving = false;

            saveButton.IsEnabled = true;

            saveButton.Text =
                existing == null
                    ? "Save Distribution"
                    : "Update Quantity";
        }
    }

    // =========================================================
    // EDIT
    // =========================================================

    private async Task EditDistributionAsync(
        MilkDistributionResponse item)
    {
        await ShowDistributionFormAsync(
            item);
    }

    // =========================================================
    // SEARCH
    // =========================================================

    private void SearchTextChanged(
        object? sender,
        TextChangedEventArgs e)
    {
        _ = ReloadForSearchAsync();
    }


    private async Task ReloadForSearchAsync()
    {
        if (_isLoading)
        {
            return;
        }

        try
        {
            var selectedDate =
                (DistributionDatePicker.Date ?? DateTime.Today).Date;

            var selectedSession =
                GetSelectedSessionCode();

            await LoadDistributionsAsync(
                selectedDate,
                selectedSession);
        }
        catch (UnauthorizedAccessException ex)
        {
            await ShowSessionExpiredAsync(
                ex.Message);
        }
        catch
        {
            // Keep current list while typing.
        }
    }

    // =========================================================
    // DATE
    // =========================================================

    private async void DistributionDateChanged(
    object? sender,
    DateChangedEventArgs e)
    {
        if (_isInitializing || _isLoading)
            return;

        await ReloadCurrentSelectionAsync();
    }

    // =========================================================
    // SESSION
    // =========================================================

    private async void SessionChanged(
    object? sender,
    EventArgs e)
    {
        if (_isInitializing || _isLoading)
            return;

        await ReloadCurrentSelectionAsync();
    }

    private async Task ReloadCurrentSelectionAsync()
    {
        if (_isLoading)
            return;

        try
        {
            _isLoading = true;

            DistributionRefreshView.IsRefreshing = true;

            var date =
                (DistributionDatePicker.Date ?? DateTime.Today).Date;

            var session =
                GetSelectedSessionCode();

            await LoadDistributionsAsync(
                date,
                session);

            await LoadSummaryAsync(
                date);
        }
        catch (UnauthorizedAccessException ex)
        {
            await ShowSessionExpiredAsync(
                ex.Message);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Unable to Refresh",
                ex.Message,
                "OK");
        }
        finally
        {
            _isLoading = false;

            DistributionRefreshView.IsRefreshing = false;
        }
    }


    
    // =========================================================
    // REFRESH
    // =========================================================

    private async void RefreshViewRefreshing(
        object? sender,
        EventArgs e)
    {
        await LoadAsync();
    }

    // =========================================================
    // SESSION CODE
    // =========================================================

    private string GetSelectedSessionCode()
    {
        return SessionPicker.SelectedIndex == 1
            ? "E"
            : "M";
    }

    // =========================================================
    // SESSION EXPIRED
    // =========================================================

    private async Task ShowSessionExpiredAsync(
        string message)
    {
        await DisplayAlertAsync(
            "Session Expired",
            message,
            "OK");

        await Shell.Current.GoToAsync(
            "LoginPage");
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

        public int RateGroupId { get; init; }

        public string RateGroupName { get; init; } =
            string.Empty;
    }
}