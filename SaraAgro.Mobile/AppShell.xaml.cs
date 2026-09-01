using SaraAgro.Mobile.Pages.Billing;
using SaraAgro.Mobile.Pages.MilkDistribution;
using SaraAgro.Mobile.Pages.RateGroup;
using SaraAgro.Mobile.Pages.Reports;
using SaraAgro.Mobile.Pages.Settings;
namespace SaraAgro.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();


        // =====================================================
        // AUTHENTICATION
        // =====================================================

        Routing.RegisterRoute(
            "SignupPage",
            typeof(Pages.Authentication.SignupPage));


        // =====================================================
        // DASHBOARD
        // =====================================================

        Routing.RegisterRoute(
            "DashboardPage",
            typeof(Pages.Dashboard.DashboardPage));


        // =====================================================
        // RATE MASTER
        // =====================================================

        Routing.RegisterRoute(
            "RateMasterPage",
            typeof(Pages.RateMaster.RateMasterPage));


        // =====================================================
        // CUSTOMERS
        // =====================================================

        Routing.RegisterRoute(
            "CustomersPage",
            typeof(Pages.Customers.CustomersPage));


        // =====================================================
        // RATE GROUP
        // =====================================================

        Routing.RegisterRoute(
            "RateGroupPage",
            typeof(RateGroupPage));


        // =====================================================
        // MILK DISTRIBUTION
        // =====================================================

        Routing.RegisterRoute(
            nameof(MilkDistributionPage),
            typeof(MilkDistributionPage));


        // =====================================================
        // BILLING
        // =====================================================

        Routing.RegisterRoute(
            nameof(BillingPage),
            typeof(BillingPage));


        // =====================================================
        // REPORTS
        // =====================================================

        Routing.RegisterRoute(
            nameof(ReportsPage),
            typeof(ReportsPage));

        // =====================================================
        // SETTINGS
        // =====================================================

        Routing.RegisterRoute(
            "SettingsPage",
            typeof(SettingsPage));
    }
}