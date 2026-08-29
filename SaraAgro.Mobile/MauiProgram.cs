using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SaraAgro.Mobile.Services.Api;
using SaraAgro.Mobile.Services.Api.Authentication;
using SaraAgro.Mobile.Services.Api.Billing;
using SaraAgro.Mobile.Services.Api.Customer;
using SaraAgro.Mobile.Services.Api.MilkDistribution;
using SaraAgro.Mobile.Services.Api.RateGroup;
using SaraAgro.Mobile.Services.Api.RateMaster;
using SaraAgro.Mobile.Pages.Billing;
using SaraAgro.Mobile.Services.Api.Reports;
namespace SaraAgro.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont(
                        "OpenSans-Regular.ttf",
                        "OpenSansRegular");

                    fonts.AddFont(
                        "OpenSans-Semibold.ttf",
                        "OpenSansSemibold");
                });

            // =====================================================
            // API BASE URL
            // =====================================================

            const string apiBaseUrl =
                "http://192.168.1.12:5000/";

            Console.WriteLine(
                $"API Base URL: {apiBaseUrl}");

            // =====================================================
            // API SERVICES
            // =====================================================

            builder.Services.AddHttpClient<AuthenticationApiService>(
                client =>
                {
                    client.BaseAddress =
                        new Uri(apiBaseUrl);
                });

            builder.Services.AddHttpClient<RateGroupApiService>(
                client =>
                {
                    client.BaseAddress =
                        new Uri(apiBaseUrl);
                });

            builder.Services.AddHttpClient<RateMasterApiService>(
                client =>
                {
                    client.BaseAddress =
                        new Uri(apiBaseUrl);
                });

            builder.Services.AddHttpClient<CustomerApiService>(
                client =>
                {
                    client.BaseAddress =
                        new Uri(apiBaseUrl);
                });

            builder.Services.AddHttpClient<MilkDistributionApiService>(
                client =>
                {
                    client.BaseAddress =
                        new Uri(apiBaseUrl);
                });

            builder.Services.AddHttpClient<BillingApiService>(
                client =>
                {
                    client.BaseAddress =
                        new Uri(apiBaseUrl);
                });

            builder.Services.AddHttpClient<ReportsApiService>(
                client =>
                {
                    client.BaseAddress =
                        new Uri(apiBaseUrl);
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}