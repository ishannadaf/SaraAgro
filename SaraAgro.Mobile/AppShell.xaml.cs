using SaraAgro.Mobile.Pages.Billing;
using SaraAgro.Mobile.Pages.MilkDistribution;
using SaraAgro.Mobile.Pages.RateGroup;
using SaraAgro.Mobile.Pages.Reports;

namespace SaraAgro.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(
            "SignupPage",
            typeof(Pages.Authentication.SignupPage));

        Routing.RegisterRoute(
            "DashboardPage",
            typeof(Pages.Dashboard.DashboardPage));

        Routing.RegisterRoute(
            "RateMasterPage",
            typeof(Pages.RateMaster.RateMasterPage));

        Routing.RegisterRoute(
            "CustomersPage",
            typeof(Pages.Customers.CustomersPage));

        Routing.RegisterRoute(
            "RateGroupPage",
            typeof(RateGroupPage));

        Routing.RegisterRoute(
            nameof(MilkDistributionPage),
            typeof(MilkDistributionPage));

        Routing.RegisterRoute(
            nameof(BillingPage),
            typeof(BillingPage));

        Routing.RegisterRoute(
            nameof(ReportsPage),
            typeof(ReportsPage));
    }
}