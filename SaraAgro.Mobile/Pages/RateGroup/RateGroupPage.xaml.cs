using Microsoft.Maui.Controls.Shapes;
using SaraAgro.Mobile.Services.Api.RateGroup;

namespace SaraAgro.Mobile.Pages.RateGroup;

public partial class RateGroupPage : ContentPage
{
    private sealed class RateGroupItem
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }


    private readonly RateGroupApiService _rateGroupApiService;

    private readonly List<RateGroupItem> _rateGroups = new();

    private bool _isLoading;


    public RateGroupPage(
        RateGroupApiService rateGroupApiService)
    {
        InitializeComponent();

        _rateGroupApiService =
            rateGroupApiService;
    }


    // =========================================================
    // PAGE APPEARING
    // =========================================================

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadRateGroupsAsync();
    }


    // =========================================================
    // LOAD RATE GROUPS
    // =========================================================

    private async Task LoadRateGroupsAsync()
    {
        if (_isLoading)
            return;

        try
        {
            _isLoading = true;

            EmptyState.IsVisible = false;

            var groups =
                await _rateGroupApiService
                    .GetRateGroupsAsync();

            _rateGroups.Clear();

            foreach (var group in groups)
            {
                _rateGroups.Add(
                    new RateGroupItem
                    {
                        Id = group.Id,

                        Name = group.Name,

                        IsActive = group.IsActive
                    });
            }

            RefreshRateGroups(
                SearchEntry.Text);
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
                "Unable to Load Rate Groups",
                ex.Message,
                "OK");
        }
        finally
        {
            _isLoading = false;
        }
    }


    // =========================================================
    // REFRESH LIST
    // =========================================================

    private void RefreshRateGroups(
        string? searchText = null)
    {
        RateGroupList.Children.Clear();

        var groups =
            _rateGroups.AsEnumerable();


        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var search =
                searchText.Trim();

            groups =
                groups.Where(
                    x =>
                        x.Name.Contains(
                            search,
                            StringComparison.OrdinalIgnoreCase));
        }


        var result =
            groups
                .OrderByDescending(x => x.IsActive)
                .ThenBy(x => x.Name)
                .ToList();


        RateGroupCountLabel.Text =
            result.Count == 1
                ? "1 group"
                : $"{result.Count} groups";


        EmptyState.IsVisible =
            result.Count == 0;


        foreach (var group in result)
        {
            RateGroupList.Children.Add(
                CreateRateGroupCard(group));
        }
    }


    // =========================================================
    // CARD
    // =========================================================

    private View CreateRateGroupCard(
        RateGroupItem group)
    {
        bool isDark =
            Application.Current?.RequestedTheme ==
            AppTheme.Dark;


        var card =
            new Border
            {
                Padding = 16,

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


        var nameLabel =
            new Label
            {
                Text = group.Name,

                FontSize = 17,

                FontAttributes =
                    FontAttributes.Bold,

                TextColor =
                    isDark
                        ? Color.Parse("#F2F5F1")
                        : Color.Parse("#141B2B")
            };


        var statusLabel =
            new Label
            {
                Text =
                    group.IsActive
                        ? "Active"
                        : "Inactive",

                FontSize = 11,

                FontAttributes =
                    FontAttributes.Bold,

                TextColor =
                    group.IsActive
                        ? (
                            isDark
                                ? Color.Parse("#8BD79B")
                                : Color.Parse("#004C22")
                          )
                        : (
                            isDark
                                ? Color.Parse("#FFB4AB")
                                : Color.Parse("#BA1A1A")
                          )
            };


        var subtitleLabel =
            new Label
            {
                Text =
                    group.IsActive
                        ? "Available for customers and rates"
                        : "Currently unavailable",

                FontSize = 11,

                TextColor =
                    isDark
                        ? Color.Parse("#9CA89F")
                        : Color.Parse("#707A6F")
            };


        // ---------------------------------------------------------
        // EDIT
        // ---------------------------------------------------------

        var editButton =
            new Button
            {
                Text = "Edit",

                HeightRequest = 36,

                Padding =
                    new Thickness(12, 0),

                CornerRadius = 9,

                BackgroundColor =
                    Colors.Transparent,

                FontSize = 12,

                FontAttributes =
                    FontAttributes.Bold,

                TextColor =
                    isDark
                        ? Color.Parse("#8BD79B")
                        : Color.Parse("#004C22")
            };


        editButton.Clicked +=
            async (_, _) =>
                await EditRateGroupAsync(group);


        // ---------------------------------------------------------
        // STATUS
        // ---------------------------------------------------------

        var statusButton =
            new Button
            {
                Text =
                    group.IsActive
                        ? "Deactivate"
                        : "Activate",

                HeightRequest = 36,

                Padding =
                    new Thickness(10, 0),

                CornerRadius = 9,

                BackgroundColor =
                    Colors.Transparent,

                FontSize = 12,

                FontAttributes =
                    FontAttributes.Bold,

                TextColor =
                    group.IsActive
                        ? (
                            isDark
                                ? Color.Parse("#FFB4AB")
                                : Color.Parse("#BA1A1A")
                          )
                        : (
                            isDark
                                ? Color.Parse("#8BD79B")
                                : Color.Parse("#004C22")
                          )
            };


        statusButton.Clicked +=
            async (_, _) =>
                await ToggleStatusAsync(group);


        var buttons =
            new HorizontalStackLayout
            {
                Spacing = 4,

                HorizontalOptions =
                    LayoutOptions.End
            };


        buttons.Children.Add(editButton);
        buttons.Children.Add(statusButton);


        var info =
            new VerticalStackLayout
            {
                Spacing = 3,

                VerticalOptions =
                    LayoutOptions.Center
            };

        info.Children.Add(nameLabel);
        info.Children.Add(statusLabel);
        info.Children.Add(subtitleLabel);


        var grid =
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
                    },

                ColumnSpacing = 10
            };


        Grid.SetColumn(info, 0);
        Grid.SetColumn(buttons, 1);


        grid.Children.Add(info);
        grid.Children.Add(buttons);


        card.Content = grid;


        return card;
    }


    // =========================================================
    // SEARCH
    // =========================================================

    private void SearchTextChanged(
        object? sender,
        TextChangedEventArgs e)
    {
        RefreshRateGroups(
            e.NewTextValue);
    }


    // =========================================================
    // ADD
    // =========================================================

    private async void AddRateGroupTapped(
        object? sender,
        TappedEventArgs e)
    {
        await ShowRateGroupFormAsync(null);
    }


    // =========================================================
    // EDIT
    // =========================================================

    private async Task EditRateGroupAsync(
    RateGroupItem group)
    {
        bool isDark =
            Application.Current?.RequestedTheme ==
            AppTheme.Dark;

        var nameEntry =
            new Entry
            {
                Text = group.Name,

                Placeholder =
                    "Enter rate group name",

                FontSize = 14,

                TextColor =
                    isDark
                        ? Color.Parse("#F2F5F1")
                        : Color.Parse("#141B2B"),

                PlaceholderColor =
                    isDark
                        ? Color.Parse("#78847B")
                        : Color.Parse("#8A938A")
            };

        var saveButton =
            new Button
            {
                Text = "Save Changes",

                HeightRequest = 50,

                CornerRadius = 12,

                BackgroundColor =
                    isDark
                        ? Color.Parse("#8BD79B")
                        : Color.Parse("#004C22"),

                TextColor =
                    isDark
                        ? Color.Parse("#06210F")
                        : Colors.White,

                FontAttributes =
                    FontAttributes.Bold
            };

        var cancelButton =
            new Button
            {
                Text = "Cancel",

                HeightRequest = 46,

                CornerRadius = 12,

                BackgroundColor =
                    Colors.Transparent,

                TextColor =
                    isDark
                        ? Color.Parse("#8BD79B")
                        : Color.Parse("#004C22"),

                FontAttributes =
                    FontAttributes.Bold
            };

        var page =
            new ContentPage
            {
                BackgroundColor =
                    isDark
                        ? Color.Parse("#0F1411")
                        : Color.Parse("#F7FAF7")
            };

        var layout =
            new VerticalStackLayout
            {
                Padding = 22,

                Spacing = 18
            };

        layout.Children.Add(
            new Label
            {
                Text = "Edit Rate Group",

                FontSize = 24,

                FontAttributes =
                    FontAttributes.Bold,

                TextColor =
                    isDark
                        ? Color.Parse("#F2F5F1")
                        : Color.Parse("#141B2B")
            });

        layout.Children.Add(
            new Label
            {
                Text = "Rate Group Name",

                FontSize = 12,

                FontAttributes =
                    FontAttributes.Bold,

                TextColor =
                    isDark
                        ? Color.Parse("#C9D1CA")
                        : Color.Parse("#404940")
            });

        layout.Children.Add(nameEntry);
        layout.Children.Add(saveButton);
        layout.Children.Add(cancelButton);

        page.Content = layout;

        cancelButton.Clicked +=
            async (_, _) =>
            {
                await Navigation.PopModalAsync();
            };

        saveButton.Clicked +=
            async (_, _) =>
            {
                var name =
                    nameEntry.Text?.Trim()
                    ?? string.Empty;

                if (string.IsNullOrWhiteSpace(name))
                {
                    await DisplayAlertAsync(
                        "Name Required",
                        "Please enter a rate group name.",
                        "OK");

                    nameEntry.Focus();

                    return;
                }

                if (string.Equals(
                        name,
                        group.Name,
                        StringComparison.OrdinalIgnoreCase))
                {
                    await Navigation.PopModalAsync();

                    return;
                }

                try
                {
                    saveButton.IsEnabled = false;

                    saveButton.Text =
                        "Saving...";

                    await _rateGroupApiService
                        .UpdateRateGroupAsync(
                            group.Id,
                            name);

                    await Navigation.PopModalAsync();

                    await LoadRateGroupsAsync();

                    await DisplayAlertAsync(
                        "Success",
                        "Rate group updated successfully.",
                        "OK");
                }
                catch (UnauthorizedAccessException)
                {
                    await Navigation.PopModalAsync();

                    await DisplayAlertAsync(
                        "Session Expired",
                        "Your session has expired. Please login again.",
                        "OK");

                    await Shell.Current.GoToAsync(
                        "//LoginPage");
                }
                catch (Exception ex)
                {
                    await DisplayAlertAsync(
                        "Unable to Update Rate Group",
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

        await Navigation.PushModalAsync(
            new NavigationPage(page));
    }


    // =========================================================
    // ADD FORM
    // =========================================================

    private async Task ShowRateGroupFormAsync(
        RateGroupItem? existingGroup)
    {
        bool isDark =
            Application.Current?.RequestedTheme ==
            AppTheme.Dark;


        var nameEntry =
            new Entry
            {
                Placeholder =
                    "Enter rate group name",

                FontSize = 14,

                TextColor =
                    isDark
                        ? Color.Parse("#F2F5F1")
                        : Color.Parse("#141B2B"),

                PlaceholderColor =
                    isDark
                        ? Color.Parse("#78847B")
                        : Color.Parse("#8A938A")
            };


        var form =
            new VerticalStackLayout
            {
                Spacing = 8,

                Children =
                {
                    new Label
                    {
                        Text = "Rate Group Name",

                        FontSize = 12,

                        FontAttributes =
                            FontAttributes.Bold,

                        TextColor =
                            isDark
                                ? Color.Parse("#C9D1CA")
                                : Color.Parse("#404940")
                    },

                    nameEntry
                }
            };


        var dialog =
            new ContentPage
            {
                BackgroundColor =
                    isDark
                        ? Color.Parse("#0F1411")
                        : Color.Parse("#F7FAF7"),

                Content =
                    new VerticalStackLayout
                    {
                        Padding = 22,

                        Spacing = 18,

                        Children =
                        {
                            new Label
                            {
                                Text =
                                    "Add Rate Group",

                                FontSize = 24,

                                FontAttributes =
                                    FontAttributes.Bold,

                                TextColor =
                                    isDark
                                        ? Color.Parse("#F2F5F1")
                                        : Color.Parse("#141B2B")
                            },

                            form
                        }
                    }
            };


        var saveButton =
            new Button
            {
                Text =
                    "Save Rate Group",

                HeightRequest = 50,

                CornerRadius = 12,

                BackgroundColor =
                    isDark
                        ? Color.Parse("#8BD79B")
                        : Color.Parse("#004C22"),

                TextColor =
                    isDark
                        ? Color.Parse("#06210F")
                        : Colors.White,

                FontAttributes =
                    FontAttributes.Bold
            };


        var cancelButton =
            new Button
            {
                Text = "Cancel",

                HeightRequest = 46,

                CornerRadius = 12,

                BackgroundColor =
                    Colors.Transparent,

                TextColor =
                    isDark
                        ? Color.Parse("#8BD79B")
                        : Color.Parse("#004C22"),

                FontAttributes =
                    FontAttributes.Bold
            };


        saveButton.Clicked +=
            async (_, _) =>
            {
                var name =
                    nameEntry.Text?.Trim()
                    ?? string.Empty;


                if (string.IsNullOrWhiteSpace(name))
                {
                    await DisplayAlertAsync(
                        "Name Required",
                        "Please enter a rate group name.",
                        "OK");

                    nameEntry.Focus();

                    return;
                }


                try
                {
                    saveButton.IsEnabled = false;

                    saveButton.Text =
                        "Saving...";


                    var rateGroupId =
                        await _rateGroupApiService
                            .CreateRateGroupAsync(
                                name);


                    await Navigation.PopModalAsync();


                    await DisplayAlertAsync(
                        "Success",
                        "Rate group created successfully.",
                        "OK");


                    await LoadRateGroupsAsync();
                }
                catch (UnauthorizedAccessException)
                {
                    await Navigation.PopModalAsync();

                    await DisplayAlertAsync(
                        "Session Expired",
                        "Your session has expired. Please login again.",
                        "OK");

                    await Shell.Current.GoToAsync(
                        "//LoginPage");
                }
                catch (Exception ex)
                {
                    await DisplayAlertAsync(
                        "Unable to Create Rate Group",
                        ex.Message,
                        "OK");
                }
                finally
                {
                    saveButton.IsEnabled = true;

                    saveButton.Text =
                        "Save Rate Group";
                }
            };


        cancelButton.Clicked +=
            async (_, _) =>
            {
                await Navigation.PopModalAsync();
            };


        var buttons =
            new VerticalStackLayout
            {
                Spacing = 8,

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


        var pageLayout =
            (VerticalStackLayout)dialog.Content;

        pageLayout.Children.Add(
            buttons);


        await Navigation.PushModalAsync(
            new NavigationPage(dialog));
    }


    // =========================================================
    // ACTIVATE / DEACTIVATE
    // =========================================================

    private async Task ToggleStatusAsync(
        RateGroupItem group)
    {
        var action =
            group.IsActive
                ? "deactivate"
                : "activate";


        bool confirmed =
            await DisplayAlertAsync(
                group.IsActive
                    ? "Deactivate Rate Group"
                    : "Activate Rate Group",

                $"Do you want to {action} '{group.Name}'?",

                group.IsActive
                    ? "Deactivate"
                    : "Activate",

                "Cancel");


        if (!confirmed)
            return;


        try
        {
            await _rateGroupApiService
                .UpdateRateGroupStatusAsync(
                    group.Id,
                    !group.IsActive);


            await LoadRateGroupsAsync();
        }
        catch (UnauthorizedAccessException)
        {
            await DisplayAlertAsync(
                "Session Expired",
                "Your session has expired. Please login again.",
                "OK");

            await Shell.Current.GoToAsync(
                "//LoginPage");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Unable to Update Rate Group",
                ex.Message,
                "OK");
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


    private async void CustomersTapped(
        object? sender,
        TappedEventArgs e)
    {
        await DisplayAlertAsync(
            "Customers",
            "Customer Management will be connected next.",
            "OK");
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
            "Rate Groups",
            "More options will be added later.",
            "OK");
    }
}