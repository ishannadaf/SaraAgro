using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace SaraAgro.Mobile.Services.Api.Reports;

public sealed class ReportsApiService
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };


    public ReportsApiService(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    // =========================================================
    // CUSTOMER LEDGER
    // =========================================================

    // =========================================================
    // CUSTOMER LEDGER PDF
    // =========================================================

    public async Task<byte[]> DownloadCustomerLedgerPdfAsync(
        int customerId,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        var url =
            "api/Reports/customer-ledger/pdf" +
            $"?customerId={customerId}" +
            $"&fromDate={Uri.EscapeDataString(fromDate.Date.ToString("yyyy-MM-dd"))}" +
            $"&toDate={Uri.EscapeDataString(toDate.Date.ToString("yyyy-MM-dd"))}";


        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                url);


        // =====================================================
        // IMPORTANT:
        // PDF request must also send login JWT token.
        // =====================================================

        await AddAuthorizationHeaderAsync(
            request,
            cancellationToken);


        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);


        // =====================================================
        // SESSION EXPIRED
        // =====================================================

        if (response.StatusCode ==
            HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException(
                "Your session has expired. Please login again.");
        }


        // =====================================================
        // OTHER API ERRORS
        // =====================================================

        if (!response.IsSuccessStatusCode)
        {
            throw await CreateApiExceptionAsync(
                response,
                cancellationToken);
        }


        // =====================================================
        // PDF
        // =====================================================

        var pdfBytes =
            await response.Content.ReadAsByteArrayAsync(
                cancellationToken);


        if (pdfBytes == null ||
            pdfBytes.Length == 0)
        {
            throw new InvalidOperationException(
                "The server returned an empty PDF.");
        }


        return pdfBytes;
    }

    // =========================================================
    // DAILY REPORT PDF
    // =========================================================

    public async Task<byte[]> DownloadDailyReportPdfAsync(
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var url =
            "api/Reports/daily/pdf" +
            $"?date={Uri.EscapeDataString(date.Date.ToString("yyyy-MM-dd"))}";

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                url);

        await AddAuthorizationHeaderAsync(
            request,
            cancellationToken);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.StatusCode ==
            HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException(
                "Your session has expired. Please login again.");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw await CreateApiExceptionAsync(
                response,
                cancellationToken);
        }

        var pdfBytes =
            await response.Content.ReadAsByteArrayAsync(
                cancellationToken);

        if (pdfBytes.Length == 0)
        {
            throw new InvalidOperationException(
                "The server returned an empty PDF.");
        }

        return pdfBytes;
    }


    // =========================================================
    // MONTHLY REPORT PDF
    // =========================================================

    public async Task<byte[]> DownloadMonthlyReportPdfAsync(
        DateTime month,
        CancellationToken cancellationToken = default)
    {
        var url =
            "api/Reports/monthly/pdf" +
            $"?month={Uri.EscapeDataString(month.Date.ToString("yyyy-MM-dd"))}";

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                url);

        await AddAuthorizationHeaderAsync(
            request,
            cancellationToken);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.StatusCode ==
            HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException(
                "Your session has expired. Please login again.");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw await CreateApiExceptionAsync(
                response,
                cancellationToken);
        }

        var pdfBytes =
            await response.Content.ReadAsByteArrayAsync(
                cancellationToken);

        if (pdfBytes.Length == 0)
        {
            throw new InvalidOperationException(
                "The server returned an empty PDF.");
        }

        return pdfBytes;
    }


    // =========================================================
    // PENDING AMOUNT REPORT PDF
    // =========================================================

    public async Task<byte[]> DownloadPendingAmountReportPdfAsync(
        DateTime asOfDate,
        CancellationToken cancellationToken = default)
    {
        var url =
            "api/Reports/pending/pdf" +
            $"?asOfDate={Uri.EscapeDataString(asOfDate.Date.ToString("yyyy-MM-dd"))}";

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                url);

        await AddAuthorizationHeaderAsync(
            request,
            cancellationToken);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.StatusCode ==
            HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException(
                "Your session has expired. Please login again.");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw await CreateApiExceptionAsync(
                response,
                cancellationToken);
        }

        var pdfBytes =
            await response.Content.ReadAsByteArrayAsync(
                cancellationToken);

        if (pdfBytes.Length == 0)
        {
            throw new InvalidOperationException(
                "The server returned an empty PDF.");
        }

        return pdfBytes;
    }

    // =========================================================
    // FINANCIAL SUMMARY
    // =========================================================

    public async Task<FinancialSummaryResponse>
        GetFinancialSummaryAsync(
            DateTime date,
            CancellationToken cancellationToken = default)
    {
        var url =
            "api/Reports/financial-summary" +
            $"?date={Uri.EscapeDataString(date.Date.ToString("yyyy-MM-dd"))}";

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                url);

        await AddAuthorizationHeaderAsync(
            request,
            cancellationToken);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content
                .ReadFromJsonAsync<FinancialSummaryResponse>(
                    JsonOptions,
                    cancellationToken)
                ?? throw new InvalidOperationException(
                    "Invalid financial summary received from server.");
        }

        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }

    // =========================================================
    // ALL PAYMENTS REPORT PDF
    // =========================================================

    public async Task<byte[]> DownloadAllPaymentReportPdfAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        var url =
            "api/Reports/payments/pdf" +
            $"?fromDate={Uri.EscapeDataString(fromDate.Date.ToString("yyyy-MM-dd"))}" +
            $"&toDate={Uri.EscapeDataString(toDate.Date.ToString("yyyy-MM-dd"))}";

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                url);

        await AddAuthorizationHeaderAsync(
            request,
            cancellationToken);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.StatusCode ==
            HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException(
                "Your session has expired. Please login again.");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw await CreateApiExceptionAsync(
                response,
                cancellationToken);
        }

        var pdfBytes =
            await response.Content.ReadAsByteArrayAsync(
                cancellationToken);

        if (pdfBytes.Length == 0)
        {
            throw new InvalidOperationException(
                "The server returned an empty PDF.");
        }

        return pdfBytes;
    }

    // =========================================================
    // DAILY REPORT
    // =========================================================

    public async Task<DailyReportResponse>
        GetDailyReportAsync(
            DateTime date,
            CancellationToken cancellationToken = default)
    {
        var url =
            "api/Reports/daily" +
            $"?date={Uri.EscapeDataString(date.Date.ToString("O"))}";

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                url);

        await AddAuthorizationHeaderAsync(
            request,
            cancellationToken);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content
                .ReadFromJsonAsync<DailyReportResponse>(
                    JsonOptions,
                    cancellationToken)
                ?? throw new InvalidOperationException(
                    "Invalid daily report received from server.");
        }

        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }


    // =========================================================
    // MONTHLY REPORT
    // =========================================================

    public async Task<MonthlyReportResponse>
        GetMonthlyReportAsync(
            DateTime month,
            CancellationToken cancellationToken = default)
    {
        var monthStart =
            new DateTime(
                month.Year,
                month.Month,
                1);

        var url =
            "api/Reports/monthly" +
            $"?month={Uri.EscapeDataString(monthStart.ToString("O"))}";

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                url);

        await AddAuthorizationHeaderAsync(
            request,
            cancellationToken);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content
                .ReadFromJsonAsync<MonthlyReportResponse>(
                    JsonOptions,
                    cancellationToken)
                ?? throw new InvalidOperationException(
                    "Invalid monthly report received from server.");
        }

        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }


    // =========================================================
    // PENDING AMOUNT REPORT
    // =========================================================

    public async Task<PendingAmountReportResponse>
        GetPendingAmountReportAsync(
            DateTime? asOfDate = null,
            CancellationToken cancellationToken = default)
    {
        var date =
            (asOfDate ?? DateTime.Today).Date;

        var url =
            "api/Reports/pending" +
            $"?asOfDate={Uri.EscapeDataString(date.ToString("O"))}";

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                url);

        await AddAuthorizationHeaderAsync(
            request,
            cancellationToken);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content
                .ReadFromJsonAsync<PendingAmountReportResponse>(
                    JsonOptions,
                    cancellationToken)
                ?? throw new InvalidOperationException(
                    "Invalid pending amount report received from server.");
        }

        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }


    // =========================================================
    // ALL PAYMENTS
    // =========================================================

    public async Task<AllPaymentReportResponse>
        GetAllPaymentReportAsync(
            DateTime fromDate,
            DateTime toDate,
            CancellationToken cancellationToken = default)
    {
        var url =
            "api/Reports/payments" +
            $"?fromDate={Uri.EscapeDataString(fromDate.Date.ToString("O"))}" +
            $"&toDate={Uri.EscapeDataString(toDate.Date.ToString("O"))}";

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                url);

        await AddAuthorizationHeaderAsync(
            request,
            cancellationToken);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content
                .ReadFromJsonAsync<AllPaymentReportResponse>(
                    JsonOptions,
                    cancellationToken)
                ?? throw new InvalidOperationException(
                    "Invalid payment report received from server.");
        }

        throw await CreateApiExceptionAsync(
            response,
            cancellationToken);
    }


    // =========================================================
    // AUTHORIZATION
    // =========================================================

    private static async Task AddAuthorizationHeaderAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token =
            await SecureStorage.Default.GetAsync(
                "access_token");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException(
                "Access token is missing. Please login again.");
        }

        request.Headers.Remove(
            "Authorization");

        request.Headers.TryAddWithoutValidation(
            "Authorization",
            $"Bearer {token}");
    }


    // =========================================================
    // API ERROR
    // =========================================================

    private static async Task<Exception>
        CreateApiExceptionAsync(
            HttpResponseMessage response,
            CancellationToken cancellationToken)
    {
        var responseText =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        if (!string.IsNullOrWhiteSpace(responseText))
        {
            try
            {
                using var document =
                    JsonDocument.Parse(responseText);

                var root =
                    document.RootElement;


                // -------------------------------------------------
                // ASP.NET VALIDATION ERRORS
                // -------------------------------------------------

                if (root.TryGetProperty(
                        "errors",
                        out var errors))
                {
                    var messages =
                        new List<string>();

                    foreach (var property
                             in errors.EnumerateObject())
                    {
                        foreach (var error
                                 in property.Value.EnumerateArray())
                        {
                            if (error.ValueKind ==
                                JsonValueKind.String)
                            {
                                messages.Add(
                                    $"{property.Name}: {error.GetString()}");
                            }
                        }
                    }

                    if (messages.Count > 0)
                    {
                        return new InvalidOperationException(
                            string.Join(
                                Environment.NewLine,
                                messages));
                    }
                }


                // -------------------------------------------------
                // NORMAL API MESSAGE
                // -------------------------------------------------

                if (root.TryGetProperty(
                        "message",
                        out var messageProperty))
                {
                    var message =
                        messageProperty.GetString();

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        if (response.StatusCode ==
                            HttpStatusCode.Unauthorized)
                        {
                            return new UnauthorizedAccessException(
                                message);
                        }

                        return new InvalidOperationException(
                            message);
                    }
                }


                // -------------------------------------------------
                // ERROR PROPERTY
                // -------------------------------------------------

                if (root.TryGetProperty(
                        "error",
                        out var errorProperty))
                {
                    var error =
                        errorProperty.GetString();

                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        if (response.StatusCode ==
                            HttpStatusCode.Unauthorized)
                        {
                            return new UnauthorizedAccessException(
                                error);
                        }

                        return new InvalidOperationException(
                            error);
                    }
                }
            }
            catch (JsonException)
            {
                // Response was not JSON.
            }
        }


        // -----------------------------------------------------
        // STATUS CODE FALLBACK
        // -----------------------------------------------------

        if (response.StatusCode ==
            HttpStatusCode.Unauthorized)
        {
            return new UnauthorizedAccessException(
                "Your session has expired. Please login again.");
        }

        if (response.StatusCode ==
            HttpStatusCode.Forbidden)
        {
            return new UnauthorizedAccessException(
                "You are not authorized to access this report.");
        }


        return new HttpRequestException(
            $"Report API request failed " +
            $"with status code {(int)response.StatusCode} " +
            $"({response.StatusCode}).");
    }
}


// =============================================================
// CUSTOMER LEDGER RESPONSE
// =============================================================

public sealed class CustomerLedgerResponse
{
    public int CustomerId { get; set; }

    public string CustomerCode { get; set; } =
        string.Empty;

    public string CustomerName { get; set; } =
        string.Empty;

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public decimal OpeningBalance { get; set; }

    public decimal TotalMilkQuantity { get; set; }

    public decimal TotalMilkAmount { get; set; }

    public decimal TotalPayments { get; set; }

    public decimal ClosingBalance { get; set; }

    public List<CustomerLedgerEntry> Entries { get; set; } =
        new();
}


public sealed class CustomerLedgerEntry
{
    public DateTime Date { get; set; }

    public string Type { get; set; } =
        string.Empty;

    public string Description { get; set; } =
        string.Empty;

    public string Session { get; set; } =
        string.Empty;

    public string MilkType { get; set; } =
        string.Empty;

    public decimal Quantity { get; set; }

    public decimal Rate { get; set; }

    public decimal Debit { get; set; }

    public decimal Credit { get; set; }

    public decimal RunningBalance { get; set; }

    public string PaymentMode { get; set; } =
        string.Empty;

    public string ReferenceNumber { get; set; } =
        string.Empty;
}


// =============================================================
// DAILY REPORT
// =============================================================

public sealed class DailyReportResponse
{
    public DateTime Date { get; set; }

    public decimal MorningCowQuantity { get; set; }

    public decimal MorningBuffaloQuantity { get; set; }

    public decimal MorningTotalQuantity { get; set; }

    public decimal MorningAmount { get; set; }

    public decimal EveningCowQuantity { get; set; }

    public decimal EveningBuffaloQuantity { get; set; }

    public decimal EveningTotalQuantity { get; set; }

    public decimal EveningAmount { get; set; }

    public decimal TotalCowQuantity { get; set; }

    public decimal TotalBuffaloQuantity { get; set; }

    public decimal TotalQuantity { get; set; }

    public decimal TotalAmount { get; set; }

    public int CustomerCount { get; set; }

    public List<DailyCustomerReportRow> Customers { get; set; } =
        new();
}


public sealed class DailyCustomerReportRow
{
    public int CustomerId { get; set; }

    public string CustomerCode { get; set; } =
        string.Empty;

    public string CustomerName { get; set; } =
        string.Empty;

    public decimal CowQuantity { get; set; }

    public decimal BuffaloQuantity { get; set; }

    public decimal TotalQuantity { get; set; }

    public decimal TotalAmount { get; set; }
}


// =============================================================
// MONTHLY REPORT
// =============================================================

public sealed class MonthlyReportResponse
{
    public DateTime Month { get; set; }

    public decimal CowQuantity { get; set; }

    public decimal BuffaloQuantity { get; set; }

    public decimal TotalQuantity { get; set; }

    public decimal TotalAmount { get; set; }

    public int CustomerCount { get; set; }

    public int DaysRecorded { get; set; }

    public List<MonthlyCustomerReportRow> Customers { get; set; } =
        new();
}


public sealed class MonthlyCustomerReportRow
{
    public int CustomerId { get; set; }

    public string CustomerCode { get; set; } =
        string.Empty;

    public string CustomerName { get; set; } =
        string.Empty;

    public decimal CowQuantity { get; set; }

    public decimal BuffaloQuantity { get; set; }

    public decimal TotalQuantity { get; set; }

    public decimal TotalAmount { get; set; }
}


// =============================================================
// PENDING AMOUNT REPORT
// =============================================================

public sealed class PendingAmountReportResponse
{
    public DateTime AsOfDate { get; set; }

    public int CustomerCount { get; set; }

    public decimal TotalPendingAmount { get; set; }

    public List<PendingCustomerReportRow> Customers { get; set; } =
        new();
}


public sealed class PendingCustomerReportRow
{
    public int CustomerId { get; set; }

    public string CustomerCode { get; set; } =
        string.Empty;

    public string CustomerName { get; set; } =
        string.Empty;

    public string BillNumber { get; set; } =
        string.Empty;

    public DateTime BillDate { get; set; }

    public decimal TotalPayable { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal PendingAmount { get; set; }

    public string Status { get; set; } =
        string.Empty;
}


// =============================================================
// ALL PAYMENT REPORT
// =============================================================

public sealed class AllPaymentReportResponse
{
    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public decimal TotalPaymentAmount { get; set; }

    public int PaymentCount { get; set; }

    public List<PaymentReportRow> Payments { get; set; } =
        new();
}


public sealed class PaymentReportRow
{
    public int PaymentId { get; set; }

    public int BillId { get; set; }

    public string BillNumber { get; set; } =
        string.Empty;

    public int CustomerId { get; set; }

    public string CustomerCode { get; set; } =
        string.Empty;

    public string CustomerName { get; set; } =
        string.Empty;

    public DateTime PaymentDate { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMode { get; set; } =
        string.Empty;

    public string ReferenceNumber { get; set; } =
        string.Empty;

    public string Notes { get; set; } =
        string.Empty;
}

// =============================================================
// FINANCIAL SUMMARY
// =============================================================

public sealed class FinancialSummaryResponse
{
    public DateTime Date { get; set; }

    public decimal TodayRevenue { get; set; }

    public decimal TodayCollection { get; set; }

    public decimal TodayExpense { get; set; }

    public decimal TodayProfit { get; set; }

    public DateTime Month { get; set; }

    public decimal MonthlyRevenue { get; set; }

    public decimal MonthlyCollection { get; set; }

    public decimal MonthlyExpense { get; set; }

    public decimal MonthlyProfit { get; set; }
}