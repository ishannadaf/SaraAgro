using System.Globalization;
using Microsoft.Maui.Controls.Shapes;
using SaraAgro.Mobile.Services.Api.RateGroup;
using SaraAgro.Mobile.Services.Api.RateMaster;

namespace SaraAgro.Mobile.Pages.RateMaster;

public partial class RateMasterPage : ContentPage
{
    // =========================================================
    // MODELS
    // =========================================================

    private sealed class RateGroupItem
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }


    private sealed class RateMasterItem
    {
        public int Id { get; set; }

        public int RateGroupId { get; set; }

        public string RateGroupName { get; set; } = string.Empty;

        public string MilkType { get; set; } = string.Empty;

        public decimal Rate { get; set; }

        public DateTime EffectiveDate { get; set; }
    }


    // =========================================================
    // SERVICES
    // =========================================================

    private readonly RateMasterApiService _rateMasterApiService;

    private readonly RateGroupApiService _rateGroupApiService;


    // =========================================================
    // DATA
    // =========================================================

    private readonly List<RateGroupItem> _rateGroups = new();

    private readonly List<RateMasterItem> _rates = new();


    private bool _isLoading;


    private string _selectedRateGroupFilter =
        "All Rate Groups";

    private string _selectedMilkTypeFilter =
        "All Milk Types";


    // =========================================================
    // CONSTRUCTOR
    // =========================================================

    public RateMasterPage(
        RateMasterApiService rateMasterApiService,
        RateGroupApiService rateGroupApiService)
    {
        InitializeComponent();

        _rateMasterApiService =
            rateMasterApiService;

        _rateGroupApiService =
            rateGroupApiService;
    }


    // =========================================================
    // PAGE APPEARING
    // =========================================================

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadDataAsync();
    }


    // =========================================================
    // LOAD DATA
    // =========================================================

    private async Task LoadDataAsync()
    {
        if (_isLoading)
            return;

        try
        {
            _isLoading = true;

            var groups =
                await _rateGroupApiService
                    .GetRateGroupsAsync();

            var rates =
                await _rateMasterApiService
                    .GetRatesAsync();


            // -------------------------------------------------
            // RATE GROUPS
            // -------------------------------------------------

            _rateGroups.Clear();

            foreach (var group in groups)
            {
                _rateGroups.Add(
                    new RateGroupItem
                    {
                        Id =
                            group.Id,

                        Name =
                            group.Name,

                        IsActive =
                            group.IsActive
                    });
            }


            // -------------------------------------------------
            // RATES
            // -------------------------------------------------

            _rates.Clear();

            foreach (var rate in rates)
            {
                _rates.Add(
                    new RateMasterItem
                    {
                        Id =
                            rate.RateMasterId,

                        RateGroupId =
                            rate.RateGroupId,

                        RateGroupName =
                            rate.RateGroupName,

                        MilkType =
                            rate.MilkType,

                        Rate =
                            rate.Rate,

                        EffectiveDate =
                            rate.EffectiveDate
                    });
            }


            InitializeFilters();

            RefreshRates();
        }
        catch (UnauthorizedAccessException ex)
        {
            await DisplayAlertAsync(
                "Authorization Error",
                ex.Message,
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Unable to Load Rates",
                ex.Message,
                "OK");
        }
        finally
        {
            _isLoading = false;
        }
    }


    // =========================================================
    // FILTER INITIALIZATION
    // =========================================================

    private void InitializeFilters()
    {
        RateGroupFilterPicker.Items.Clear();

        RateGroupFilterPicker.Items.Add(
            "All Rate Groups");


        foreach (var group in _rateGroups
                     .Where(x => x.IsActive)
                     .OrderBy(x => x.Name))
        {
            RateGroupFilterPicker.Items.Add(
                group.Name);
        }


        MilkTypeFilterPicker.Items.Clear();

        MilkTypeFilterPicker.Items.Add(
            "All Milk Types");

        MilkTypeFilterPicker.Items.Add(
            "Cow");

        MilkTypeFilterPicker.Items.Add(
            "Buffalo");


        RateGroupFilterPicker.SelectedIndex =
            0;

        MilkTypeFilterPicker.SelectedIndex =
            0;


        _selectedRateGroupFilter =
            "All Rate Groups";

        _selectedMilkTypeFilter =
            "All Milk Types";
    }


    // =========================================================
    // REFRESH RATES
    // =========================================================

    private void RefreshRates()
    {
        RateList.Children.Clear();


        IEnumerable<RateMasterItem> filteredRates =
            _rates;


        // -----------------------------------------------------
        // RATE GROUP FILTER
        // -----------------------------------------------------

        if (!string.Equals(
                _selectedRateGroupFilter,
                "All Rate Groups",
                StringComparison.OrdinalIgnoreCase))
        {
            filteredRates =
                filteredRates.Where(
                    x =>
                        string.Equals(
                            x.RateGroupName,
                            _selectedRateGroupFilter,
                            StringComparison.OrdinalIgnoreCase));
        }


        // -----------------------------------------------------
        // MILK TYPE FILTER
        // -----------------------------------------------------

        if (!string.Equals(
                _selectedMilkTypeFilter,
                "All Milk Types",
                StringComparison.OrdinalIgnoreCase))
        {
            filteredRates =
                filteredRates.Where(
                    x =>
                        string.Equals(
                            x.MilkType,
                            _selectedMilkTypeFilter,
                            StringComparison.OrdinalIgnoreCase));
        }


        var rates =
            filteredRates
                .OrderBy(x => x.RateGroupName)
                .ThenBy(x => x.MilkType)
                .ThenByDescending(x => x.EffectiveDate)
                .ToList();


        RateCountLabel.Text =
            rates.Count == 1
                ? "1 rate"
                : $"{rates.Count} rates";


        EmptyState.IsVisible =
            rates.Count == 0;


        foreach (var rate in rates)
        {
            RateList.Children.Add(
                CreateRateCard(rate));
        }
    }


    // =========================================================
    // RATE CARD
    // =========================================================

    private View CreateRateCard(
        RateMasterItem rate)
    {
        bool isCow =
            string.Equals(
                rate.MilkType,
                "Cow",
                StringComparison.OrdinalIgnoreCase);


        bool isDark =
            Application.Current?.RequestedTheme ==
            AppTheme.Dark;


        var milkBackground =
            isCow
                ? Color.Parse("#EEF5EF")
                : Color.Parse("#FFF4DD");


        var milkBackgroundDark =
            isCow
                ? Color.Parse("#203125")
                : Color.Parse("#3A301C");


        var milkText =
            isCow
                ? Color.Parse("#004C22")
                : Color.Parse("#8A5A00");


        var milkTextDark =
            isCow
                ? Color.Parse("#8BD79B")
                : Color.Parse("#F3C96B");


        // -----------------------------------------------------
        // MILK BADGE
        // -----------------------------------------------------

        var milkBadge =
            new Border
            {
                WidthRequest = 44,

                HeightRequest = 44,

                StrokeThickness = 0,

                StrokeShape =
                    new RoundRectangle
                    {
                        CornerRadius = 22
                    },

                BackgroundColor =
                    isDark
                        ? milkBackgroundDark
                        : milkBackground
            };


        milkBadge.Content =
            new Label
            {
                Text =
                    isCow
                        ? "C"
                        : "B",

                FontSize = 14,

                FontAttributes =
                    FontAttributes.Bold,

                HorizontalTextAlignment =
                    TextAlignment.Center,

                VerticalTextAlignment =
                    TextAlignment.Center,

                TextColor =
                    isDark
                        ? milkTextDark
                        : milkText
            };


        // -----------------------------------------------------
        // RATE GROUP
        // -----------------------------------------------------

        var rateGroupLabel =
            new Label
            {
                Text =
                    rate.RateGroupName,

                FontFamily =
                    "OpenSansSemibold",

                FontAttributes =
                    FontAttributes.Bold,

                FontSize = 15,

                TextColor =
                    isDark
                        ? Color.Parse("#F2F5F1")
                        : Color.Parse("#141B2B")
            };


        // -----------------------------------------------------
        // MILK TYPE
        // -----------------------------------------------------

        var milkTypeLabel =
            new Label
            {
                Text =
                    isCow
                        ? "Cow Milk"
                        : "Buffalo Milk",

                FontSize = 11,

                TextColor =
                    isDark
                        ? Color.Parse("#C9D1CA")
                        : Color.Parse("#404940")
            };


        // -----------------------------------------------------
        // EFFECTIVE DATE
        // -----------------------------------------------------

        var effectiveDateLabel =
            new Label
            {
                Text =
                    $"Effective from {rate.EffectiveDate:dd MMM yyyy}",

                FontSize = 10,

                TextColor =
                    isDark
                        ? Color.Parse("#9CA89F")
                        : Color.Parse("#707A6F")
            };


        // -----------------------------------------------------
        // RATE
        // -----------------------------------------------------

        var rateLabel =
            new Label
            {
                Text =
                    $"₹{rate.Rate.ToString(
                        "0.00",
                        CultureInfo.InvariantCulture)}",

                FontFamily =
                    "OpenSansSemibold",

                FontAttributes =
                    FontAttributes.Bold,

                FontSize = 21,

                TextColor =
                    isDark
                        ? Color.Parse("#8BD79B")
                        : Color.Parse("#004C22")
            };


        var perLitreLabel =
            new Label
            {
                Text = "/ litre",

                FontSize = 9,

                TextColor =
                    isDark
                        ? Color.Parse("#9CA89F")
                        : Color.Parse("#707A6F")
            };


        // -----------------------------------------------------
        // EDIT BUTTON
        // -----------------------------------------------------

        var editButton =
            new Button
            {
                Text = "Edit",

                HeightRequest = 34,

                Padding =
                    new Thickness(10, 0),

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
                await EditRateAsync(rate);


        // -----------------------------------------------------
        // DELETE BUTTON
        // -----------------------------------------------------

        var deleteButton =
            new Button
            {
                Text = "Delete",

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
                    isDark
                        ? Color.Parse("#FFB4AB")
                        : Color.Parse("#BA1A1A")
            };


        deleteButton.Clicked +=
            async (_, _) =>
                await DeleteRateAsync(rate);


        var buttons =
            new HorizontalStackLayout
            {
                Spacing = 2,

                HorizontalOptions =
                    LayoutOptions.End
            };


        buttons.Children.Add(
            editButton);

        buttons.Children.Add(
            deleteButton);


        // -----------------------------------------------------
        // RATE AREA
        // -----------------------------------------------------

        var rateRow =
            new HorizontalStackLayout
            {
                Spacing = 3,

                HorizontalOptions =
                    LayoutOptions.End
            };


        rateRow.Children.Add(
            rateLabel);

        rateRow.Children.Add(
            perLitreLabel);


        var rateLayout =
            new VerticalStackLayout
            {
                Spacing = 2,

                HorizontalOptions =
                    LayoutOptions.End,

                VerticalOptions =
                    LayoutOptions.Center
            };


        rateLayout.Children.Add(
            rateRow);

        rateLayout.Children.Add(
            buttons);


        // -----------------------------------------------------
        // INFO
        // -----------------------------------------------------

        var infoLayout =
            new VerticalStackLayout
            {
                Spacing = 3,

                VerticalOptions =
                    LayoutOptions.Center
            };


        infoLayout.Children.Add(
            rateGroupLabel);

        infoLayout.Children.Add(
            milkTypeLabel);

        infoLayout.Children.Add(
            effectiveDateLabel);


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
                        : Color.Parse("#FFFFFF")
            };


        var grid =
            new Grid
            {
                ColumnDefinitions =
                    new ColumnDefinitionCollection
                    {
                        new ColumnDefinition(
                            GridLength.Auto),

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
            milkBadge,
            0);

        Grid.SetColumn(
            infoLayout,
            1);

        Grid.SetColumn(
            rateLayout,
            2);


        grid.Children.Add(
            milkBadge);

        grid.Children.Add(
            infoLayout);

        grid.Children.Add(
            rateLayout);


        card.Content = grid;


        return card;
    }


    // =========================================================
    // FILTER EVENTS
    // =========================================================

    private void RateGroupFilterChanged(
        object? sender,
        EventArgs e)
    {
        if (RateGroupFilterPicker.SelectedItem
            is string group)
        {
            _selectedRateGroupFilter =
                group;

            RefreshRates();
        }
    }


    private void MilkTypeFilterChanged(
        object? sender,
        EventArgs e)
    {
        if (MilkTypeFilterPicker.SelectedItem
            is string milkType)
        {
            _selectedMilkTypeFilter =
                milkType;

            RefreshRates();
        }
    }


    private void ClearFiltersClicked(
        object? sender,
        EventArgs e)
    {
        _selectedRateGroupFilter =
            "All Rate Groups";

        _selectedMilkTypeFilter =
            "All Milk Types";


        RateGroupFilterPicker.SelectedIndex =
            0;

        MilkTypeFilterPicker.SelectedIndex =
            0;


        RefreshRates();
    }


    // =========================================================
    // ADD RATE
    // =========================================================

    private async void AddRateTapped(
        object? sender,
        TappedEventArgs e)
    {
        await ShowRateFormAsync(null);
    }


    // =========================================================
    // EDIT RATE
    // =========================================================

    private async Task EditRateAsync(
    RateMasterItem rate)
    {
        bool isDark =
            Application.Current?.RequestedTheme ==
            AppTheme.Dark;

        var rateGroupPicker =
            new Picker
            {
                Title = "Select rate group",
                FontSize = 14,
                TextColor =
                    isDark
                        ? Color.Parse("#F2F5F1")
                        : Color.Parse("#141B2B")
            };

        foreach (var group in _rateGroups
                     .Where(x => x.IsActive)
                     .OrderBy(x => x.Name))
        {
            rateGroupPicker.Items.Add(group.Name);
        }

        var selectedGroupIndex =
            rateGroupPicker.Items.IndexOf(
                rate.RateGroupName);

        if (selectedGroupIndex >= 0)
        {
            rateGroupPicker.SelectedIndex =
                selectedGroupIndex;
        }


        var milkTypePicker =
            new Picker
            {
                Title = "Select milk type",
                FontSize = 14,
                TextColor =
                    isDark
                        ? Color.Parse("#F2F5F1")
                        : Color.Parse("#141B2B")
            };

        milkTypePicker.Items.Add("Cow");
        milkTypePicker.Items.Add("Buffalo");

        milkTypePicker.SelectedItem =
            string.Equals(
                rate.MilkType,
                "Buffalo",
                StringComparison.OrdinalIgnoreCase)
                ? "Buffalo"
                : "Cow";


        var rateEntry =
            new Entry
            {
                Text =
                    rate.Rate.ToString(
                        "0.00",
                        CultureInfo.InvariantCulture),

                Keyboard =
                    Keyboard.Numeric,

                FontSize = 14,

                TextColor =
                    isDark
                        ? Color.Parse("#F2F5F1")
                        : Color.Parse("#141B2B")
            };


        var effectiveDatePicker =
            new DatePicker
            {
                Date = rate.EffectiveDate,

                FontSize = 14,

                TextColor =
                    isDark
                        ? Color.Parse("#F2F5F1")
                        : Color.Parse("#141B2B")
            };


        var layout =
            new VerticalStackLayout
            {
                Padding = 22,
                Spacing = 8
            };

        layout.Children.Add(
            new Label
            {
                Text = "Edit Rate",

                FontSize = 22,

                FontAttributes =
                    FontAttributes.Bold,

                Margin =
                    new Thickness(0, 0, 0, 8),

                TextColor =
                    isDark
                        ? Color.Parse("#F2F5F1")
                        : Color.Parse("#141B2B")
            });

        layout.Children.Add(
            CreateFormLabel(
                "Rate Group",
                isDark));

        layout.Children.Add(
            rateGroupPicker);

        layout.Children.Add(
            CreateFormLabel(
                "Milk Type",
                isDark));

        layout.Children.Add(
            milkTypePicker);

        layout.Children.Add(
            CreateFormLabel(
                "Rate per Litre",
                isDark));

        layout.Children.Add(
            rateEntry);

        layout.Children.Add(
            CreateFormLabel(
                "Effective Date",
                isDark));

        layout.Children.Add(
            effectiveDatePicker);


        var saveButton =
            new Button
            {
                Text = "Save Changes",

                HeightRequest = 50,

                CornerRadius = 11,

                FontAttributes =
                    FontAttributes.Bold,

                Margin =
                    new Thickness(0, 12, 0, 0),

                BackgroundColor =
                    isDark
                        ? Color.Parse("#8BD79B")
                        : Color.Parse("#004C22"),

                TextColor =
                    isDark
                        ? Color.Parse("#06210F")
                        : Colors.White
            };


        var cancelButton =
            new Button
            {
                Text = "Cancel",

                HeightRequest = 48,

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
                var selectedGroupName =
                    rateGroupPicker.SelectedItem?
                        .ToString()
                    ?? string.Empty;

                var selectedMilkType =
                    milkTypePicker.SelectedItem?
                        .ToString()
                    ?? string.Empty;

                var rateText =
                    rateEntry.Text?.Trim()
                    ?? string.Empty;


                if (string.IsNullOrWhiteSpace(
                        selectedGroupName))
                {
                    await DisplayAlertAsync(
                        "Rate Group Required",
                        "Please select a rate group.",
                        "OK");

                    return;
                }


                var selectedGroup =
                    _rateGroups.FirstOrDefault(
                        x =>
                            string.Equals(
                                x.Name,
                                selectedGroupName,
                                StringComparison.OrdinalIgnoreCase) &&
                            x.IsActive);


                if (selectedGroup == null)
                {
                    await DisplayAlertAsync(
                        "Invalid Rate Group",
                        "The selected rate group is not available.",
                        "OK");

                    return;
                }


                if (string.IsNullOrWhiteSpace(
                        selectedMilkType))
                {
                    await DisplayAlertAsync(
                        "Milk Type Required",
                        "Please select Cow or Buffalo.",
                        "OK");

                    return;
                }


                if (!decimal.TryParse(
                        rateText,
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out var newRate))
                {
                    await DisplayAlertAsync(
                        "Invalid Rate",
                        "Please enter a valid rate.",
                        "OK");

                    rateEntry.Focus();

                    return;
                }


                if (newRate <= 0)
                {
                    await DisplayAlertAsync(
                        "Invalid Rate",
                        "Rate must be greater than zero.",
                        "OK");

                    rateEntry.Focus();

                    return;
                }


                newRate =
                    Math.Round(
                        newRate,
                        2,
                        MidpointRounding.AwayFromZero);


                DateTime effectiveDate =
                    effectiveDatePicker.Date ??
                    DateTime.Today;


                try
                {
                    saveButton.IsEnabled = false;

                    saveButton.Text = "Saving...";


                    await _rateMasterApiService
                        .UpdateRateAsync(
                            rate.Id,
                            selectedGroup.Id,
                            selectedMilkType,
                            newRate,
                            effectiveDate);


                    await Navigation.PopModalAsync();

                    await LoadDataAsync();

                    await DisplayAlertAsync(
                        "Success",
                        "Rate updated successfully.",
                        "OK");
                }
                catch (UnauthorizedAccessException ex)
                {
                    await DisplayAlertAsync(
                        "Authorization Error",
                        ex.Message,
                        "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlertAsync(
                        "Unable to Update Rate",
                        ex.Message,
                        "OK");
                }
                finally
                {
                    saveButton.IsEnabled = true;

                    saveButton.Text =
                        "Save Changes";
                }
            };


        cancelButton.Clicked +=
            async (_, _) =>
            {
                await Navigation.PopModalAsync();
            };


        layout.Children.Add(
            saveButton);

        layout.Children.Add(
            cancelButton);


        var page =
            new ContentPage
            {
                Title = "Edit Rate",

                BackgroundColor =
                    isDark
                        ? Color.Parse("#0F1411")
                        : Color.Parse("#F8FAF8"),

                Content =
                    new ScrollView
                    {
                        Content = layout
                    }
            };


        await Navigation.PushModalAsync(
            new NavigationPage(page));
    }


    // =========================================================
    // ADD RATE FORM
    // =========================================================

    private async Task ShowRateFormAsync(
        RateMasterItem? existingRate)
    {
        bool isDark =
            Application.Current?.RequestedTheme ==
            AppTheme.Dark;


        var rateGroupPicker =
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


        foreach (var group in _rateGroups
                     .Where(x => x.IsActive)
                     .OrderBy(x => x.Name))
        {
            rateGroupPicker.Items.Add(
                group.Name);
        }


        if (rateGroupPicker.Items.Count > 0)
        {
            rateGroupPicker.SelectedIndex =
                0;
        }


        var milkTypePicker =
            new Picker
            {
                Title =
                    "Select milk type",

                FontSize = 14,

                TextColor =
                    isDark
                        ? Color.Parse("#F2F5F1")
                        : Color.Parse("#141B2B")
            };


        milkTypePicker.Items.Add(
            "Cow");

        milkTypePicker.Items.Add(
            "Buffalo");

        milkTypePicker.SelectedIndex =
            0;


        var rateEntry =
            new Entry
            {
                Placeholder =
                    "0.00",

                Keyboard =
                    Keyboard.Numeric,

                FontSize = 14,

                TextColor =
                    isDark
                        ? Color.Parse("#F2F5F1")
                        : Color.Parse("#141B2B")
            };


        var effectiveDatePicker =
            new DatePicker
            {
                Date =
                    DateTime.Today,

                FontSize = 14,

                TextColor =
                    isDark
                        ? Color.Parse("#F2F5F1")
                        : Color.Parse("#141B2B")
            };


        var formLayout =
            new VerticalStackLayout
            {
                Spacing = 8
            };


        formLayout.Children.Add(
            CreateFormLabel(
                "Rate Group",
                isDark));

        formLayout.Children.Add(
            rateGroupPicker);

        formLayout.Children.Add(
            CreateFormLabel(
                "Milk Type",
                isDark));

        formLayout.Children.Add(
            milkTypePicker);

        formLayout.Children.Add(
            CreateFormLabel(
                "Rate per Litre",
                isDark));

        formLayout.Children.Add(
            rateEntry);

        formLayout.Children.Add(
            CreateFormLabel(
                "Effective Date",
                isDark));

        formLayout.Children.Add(
            effectiveDatePicker);


        var pageContent =
            new VerticalStackLayout
            {
                Padding = 22,

                Spacing = 16
            };


        pageContent.Children.Add(
            new Label
            {
                Text =
                    "Add Rate",

                FontFamily =
                    "OpenSansSemibold",

                FontAttributes =
                    FontAttributes.Bold,

                FontSize = 22,

                TextColor =
                    isDark
                        ? Color.Parse("#F2F5F1")
                        : Color.Parse("#141B2B")
            });


        pageContent.Children.Add(
            formLayout);


        var saveButton =
            new Button
            {
                Text =
                    "Save Rate",

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


        var cancelButton =
            new Button
            {
                Text =
                    "Cancel",

                HeightRequest = 48,

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
                await SaveRateAsync(
                    rateGroupPicker,
                    milkTypePicker,
                    rateEntry,
                    effectiveDatePicker,
                    saveButton);
            };


        cancelButton.Clicked +=
            async (_, _) =>
            {
                await Navigation.PopModalAsync();
            };


        pageContent.Children.Add(
            saveButton);

        pageContent.Children.Add(
            cancelButton);


        var dialog =
            new ContentPage
            {
                Title =
                    "Add Rate",

                BackgroundColor =
                    isDark
                        ? Color.Parse("#0F1411")
                        : Color.Parse("#F8FAF8"),

                Content =
                    new ScrollView
                    {
                        Content =
                            pageContent
                    }
            };


        await Navigation.PushModalAsync(
            new NavigationPage(dialog));
    }


    // =========================================================
    // FORM LABEL
    // =========================================================

    private static Label CreateFormLabel(
        string text,
        bool isDark)
    {
        return new Label
        {
            Text = text,

            FontSize = 11,

            FontAttributes =
                FontAttributes.Bold,

            Margin =
                new Thickness(
                    0,
                    4,
                    0,
                    0),

            TextColor =
                isDark
                    ? Color.Parse("#C9D1CA")
                    : Color.Parse("#404940")
        };
    }


    // =========================================================
    // SAVE RATE
    // =========================================================

    private async Task SaveRateAsync(
        Picker rateGroupPicker,
        Picker milkTypePicker,
        Entry rateEntry,
        DatePicker effectiveDatePicker,
        Button saveButton)
    {
        var selectedGroupName =
            rateGroupPicker.SelectedItem?
                .ToString()
            ?? string.Empty;


        var selectedMilkType =
            milkTypePicker.SelectedItem?
                .ToString()
            ?? string.Empty;


        var rateText =
            rateEntry.Text?.Trim()
            ?? string.Empty;


        // -----------------------------------------------------
        // RATE GROUP
        // -----------------------------------------------------

        if (string.IsNullOrWhiteSpace(
                selectedGroupName))
        {
            await DisplayAlertAsync(
                "Rate Group Required",
                "Please select a rate group.",
                "OK");

            return;
        }


        var selectedGroup =
            _rateGroups.FirstOrDefault(
                x =>
                    string.Equals(
                        x.Name,
                        selectedGroupName,
                        StringComparison.OrdinalIgnoreCase) &&
                    x.IsActive);


        if (selectedGroup == null)
        {
            await DisplayAlertAsync(
                "Invalid Rate Group",
                "The selected rate group is not available.",
                "OK");

            return;
        }


        // -----------------------------------------------------
        // MILK TYPE
        // -----------------------------------------------------

        if (string.IsNullOrWhiteSpace(
                selectedMilkType))
        {
            await DisplayAlertAsync(
                "Milk Type Required",
                "Please select Cow or Buffalo.",
                "OK");

            return;
        }


        // -----------------------------------------------------
        // RATE
        // -----------------------------------------------------

        if (!decimal.TryParse(
                rateText,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var rate))
        {
            await DisplayAlertAsync(
                "Invalid Rate",
                "Please enter a valid rate.",
                "OK");

            rateEntry.Focus();

            return;
        }


        if (rate <= 0)
        {
            await DisplayAlertAsync(
                "Invalid Rate",
                "Rate must be greater than zero.",
                "OK");

            rateEntry.Focus();

            return;
        }


        rate =
            Math.Round(
                rate,
                2,
                MidpointRounding.AwayFromZero);


        DateTime effectiveDate =
                effectiveDatePicker.Date ?? DateTime.Today;


        // -----------------------------------------------------
        // SAVE TO API
        // -----------------------------------------------------

        try
        {
            saveButton.IsEnabled =
                false;

            saveButton.Text =
                "Saving...";


            await _rateMasterApiService
                .CreateRateAsync(
                    selectedGroup.Id,
                    selectedMilkType,
                    rate,
                    effectiveDate);


            await Navigation.PopModalAsync();


            await LoadDataAsync();


            await DisplayAlertAsync(
                "Success",
                "Rate created successfully.",
                "OK");
        }
        catch (UnauthorizedAccessException ex)
        {
            await DisplayAlertAsync(
                "Authorization Error",
                ex.Message,
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Unable to Save Rate",
                ex.Message,
                "OK");
        }
        finally
        {
            saveButton.IsEnabled =
                true;

            saveButton.Text =
                "Save Rate";
        }
    }


    // =========================================================
    // DELETE
    // =========================================================

    private async Task DeleteRateAsync(
        RateMasterItem rate)
    {
        await DisplayAlertAsync(
            "Delete Rate",
            "Delete functionality will be connected after the delete API is added.",
            "OK");
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


    private async void CustomersTapped(
        object? sender,
        TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(
            "CustomersPage");
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


    private async void ReportsTapped(
        object? sender,
        TappedEventArgs e)
    {
        await DisplayAlertAsync(
            "Reports",
            "Reports will be connected later.",
            "OK");
    }


    private async void SettingsTapped(
        object? sender,
        TappedEventArgs e)
    {
        await DisplayAlertAsync(
            "Settings",
            "Settings will be connected later.",
            "OK");
    }


    private async void MoreClicked(
        object? sender,
        EventArgs e)
    {
        await DisplayAlertAsync(
            "Rate Master",
            "More options will be added later.",
            "OK");
    }
}