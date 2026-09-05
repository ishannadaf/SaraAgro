using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SaraAgro.Api.DTOs.Reports;

namespace SaraAgro.Api.Services.Reports;

public sealed class ReportPdfService
{
    public ReportPdfService()
    {
        QuestPDF.Settings.License =
            LicenseType.Community;
    }


    // =========================================================
    // CUSTOMER LEDGER PDF
    // =========================================================

    public byte[] GenerateCustomerLedgerPdf(
        CustomerLedgerResponse report)
    {
        var document =
            Document.Create(
                container =>
                {
                    container.Page(
                        page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(25);
                            page.DefaultTextStyle(
                                x => x.FontSize(8));

                            page.Header()
                                .Column(
                                    header =>
                                    {
                                        header.Item()
                                            .AlignCenter()
                                            .Text("SARA AGRO")
                                            .Bold()
                                            .FontSize(18);

                                        header.Item()
                                            .AlignCenter()
                                            .Text("Customer Ledger")
                                            .Bold()
                                            .FontSize(13);

                                        header.Item()
                                            .PaddingTop(4)
                                            .AlignCenter()
                                            .Text(
                                                $"{report.CustomerCode}  •  {report.CustomerName}")
                                            .FontSize(10);

                                        header.Item()
                                            .AlignCenter()
                                            .Text(
                                                $"{report.FromDate:dd MMM yyyy} - {report.ToDate:dd MMM yyyy}")
                                            .FontSize(8);
                                    });

                            page.Content()
                                .PaddingTop(15)
                                .Column(
                                    content =>
                                    {
                                        content.Item()
                                            .Table(
                                                table =>
                                                {
                                                    table.ColumnsDefinition(
                                                        columns =>
                                                        {
                                                            columns.RelativeColumn();
                                                            columns.RelativeColumn();
                                                            columns.RelativeColumn();
                                                            columns.RelativeColumn();
                                                            columns.RelativeColumn();
                                                        });

                                                    AddSummaryCell(
                                                        table,
                                                        "Opening",
                                                        Currency(report.OpeningBalance));

                                                    AddSummaryCell(
                                                        table,
                                                        "Milk",
                                                        Currency(report.TotalMilkAmount));

                                                    AddSummaryCell(
                                                        table,
                                                        "Milk Qty",
                                                        $"{report.TotalMilkQuantity:0.00} L");

                                                    AddSummaryCell(
                                                        table,
                                                        "Payments",
                                                        Currency(report.TotalPayments));

                                                    AddSummaryCell(
                                                        table,
                                                        "Closing",
                                                        Currency(report.ClosingBalance));
                                                });

                                        content.Item()
                                            .PaddingTop(15)
                                            .Text("TRANSACTIONS")
                                            .Bold()
                                            .FontSize(10);

                                        content.Item()
                                            .PaddingTop(5)
                                            .Table(
                                                table =>
                                                {
                                                    table.ColumnsDefinition(
                                                        columns =>
                                                        {
                                                            columns.ConstantColumn(55);
                                                            columns.ConstantColumn(45);
                                                            columns.RelativeColumn(2.2f);
                                                            columns.ConstantColumn(45);
                                                            columns.ConstantColumn(45);
                                                            columns.ConstantColumn(60);
                                                            columns.ConstantColumn(60);
                                                            columns.ConstantColumn(65);
                                                        });

                                                    AddHeader(table, "Date");
                                                    AddHeader(table, "Type");
                                                    AddHeader(table, "Description");
                                                    AddHeader(table, "Qty");
                                                    AddHeader(table, "Rate");
                                                    AddHeader(table, "Debit");
                                                    AddHeader(table, "Credit");
                                                    AddHeader(table, "Balance");

                                                    foreach (var entry in report.Entries)
                                                    {
                                                        AddCell(
                                                            table,
                                                            entry.Date.ToString("dd/MM/yy"));

                                                        AddCell(
                                                            table,
                                                            entry.Type);

                                                        AddCell(
                                                            table,
                                                            BuildLedgerDescription(entry));

                                                        AddCell(
                                                            table,
                                                            entry.Quantity > 0
                                                                ? $"{entry.Quantity:0.00}"
                                                                : "-");

                                                        AddCell(
                                                            table,
                                                            entry.Rate > 0
                                                                ? Currency(entry.Rate)
                                                                : "-");

                                                        AddCell(
                                                            table,
                                                            entry.Debit > 0
                                                                ? Currency(entry.Debit)
                                                                : "-");

                                                        AddCell(
                                                            table,
                                                            entry.Credit > 0
                                                                ? Currency(entry.Credit)
                                                                : "-");

                                                        AddCell(
                                                            table,
                                                            Currency(entry.RunningBalance));
                                                    }
                                                });

                                        content.Item()
                                            .PaddingTop(15)
                                            .AlignRight()
                                            .Column(
                                                totals =>
                                                {
                                                    AddTotal(
                                                        totals,
                                                        "Total Milk",
                                                        $"{report.TotalMilkQuantity:0.00} L");

                                                    AddTotal(
                                                        totals,
                                                        "Milk Amount",
                                                        Currency(report.TotalMilkAmount));

                                                    AddTotal(
                                                        totals,
                                                        "Payments",
                                                        Currency(report.TotalPayments));

                                                    totals.Item()
                                                        .PaddingTop(5)
                                                        .Text(
                                                            $"Closing Balance: {Currency(report.ClosingBalance)}")
                                                        .Bold()
                                                        .FontSize(11);
                                                });
                                    });

                            page.Footer()
                                .AlignCenter()
                                .Text(
                                    text =>
                                    {
                                        text.Span(
                                            "Sara Agro  •  Customer Ledger  •  Page ");

                                        text.CurrentPageNumber();

                                        text.Span(" / ");

                                        text.TotalPages();
                                    });
                        });
                });

        return document.GeneratePdf();
    }


    // =========================================================
    // DAILY REPORT PDF
    // =========================================================

    // =========================================================
    // DAILY REPORT PDF
    // =========================================================

    public byte[] GenerateDailyReportPdf(
        DailyReportResponse report,
        FinancialSummaryResponse financial)
    {
        var document =
            Document.Create(
                container =>
                {
                    container.Page(
                        page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(25);

                            page.DefaultTextStyle(
                                x => x.FontSize(8));

                            page.Header()
                                .Column(
                                    header =>
                                    {
                                        header.Item()
                                            .AlignCenter()
                                            .Text("SARA AGRO")
                                            .Bold()
                                            .FontSize(18);

                                        header.Item()
                                            .AlignCenter()
                                            .Text("Daily Financial & Milk Collection Report")
                                            .Bold()
                                            .FontSize(13);

                                        header.Item()
                                            .AlignCenter()
                                            .PaddingTop(4)
                                            .Text(
                                                $"Date: {report.Date:dd MMM yyyy}")
                                            .FontSize(9);
                                    });

                            page.Content()
                                .PaddingTop(15)
                                .Column(
                                    content =>
                                    {
                                        // ========================================
                                        // FINANCIAL SUMMARY
                                        // ========================================

                                        content.Item()
                                            .Text("FINANCIAL SUMMARY")
                                            .Bold()
                                            .FontSize(10);

                                        content.Item()
                                            .PaddingTop(5)
                                            .Table(
                                                table =>
                                                {
                                                    table.ColumnsDefinition(
                                                        columns =>
                                                        {
                                                            columns.RelativeColumn();
                                                            columns.RelativeColumn();
                                                            columns.RelativeColumn();
                                                            columns.RelativeColumn();
                                                        });

                                                    AddSummaryCell(
                                                        table,
                                                        "Revenue",
                                                        Currency(
                                                            financial.TodayRevenue));

                                                    AddSummaryCell(
                                                        table,
                                                        "Collection",
                                                        Currency(
                                                            financial.TodayCollection));

                                                    AddSummaryCell(
                                                        table,
                                                        "Expense",
                                                        Currency(
                                                            financial.TodayExpense));

                                                    AddSummaryCell(
                                                        table,
                                                        "Profit",
                                                        Currency(
                                                            financial.TodayProfit));
                                                });


                                        // ========================================
                                        // CUSTOMER-WISE COLLECTION
                                        // ========================================

                                        content.Item()
                                            .PaddingTop(18)
                                            .Text("CUSTOMER-WISE COLLECTION")
                                            .Bold()
                                            .FontSize(10);

                                        content.Item()
                                            .PaddingTop(5)
                                            .Table(
                                                table =>
                                                {
                                                    table.ColumnsDefinition(
                                                        columns =>
                                                        {
                                                            columns.ConstantColumn(50);
                                                            columns.RelativeColumn(2);
                                                            columns.ConstantColumn(65);
                                                            columns.ConstantColumn(75);
                                                            columns.ConstantColumn(65);
                                                            columns.ConstantColumn(80);
                                                        });

                                                    AddHeader(table, "Code");
                                                    AddHeader(table, "Customer");
                                                    AddHeader(table, "Cow L");
                                                    AddHeader(table, "Buffalo L");
                                                    AddHeader(table, "Total L");
                                                    AddHeader(table, "Amount");

                                                    foreach (
                                                        var customer
                                                        in report.Customers)
                                                    {
                                                        AddCell(
                                                            table,
                                                            customer.CustomerCode);

                                                        AddCell(
                                                            table,
                                                            customer.CustomerName);

                                                        AddCell(
                                                            table,
                                                            $"{customer.CowQuantity:0.00}");

                                                        AddCell(
                                                            table,
                                                            $"{customer.BuffaloQuantity:0.00}");

                                                        AddCell(
                                                            table,
                                                            $"{customer.TotalQuantity:0.00}");

                                                        AddCell(
                                                            table,
                                                            Currency(
                                                                customer.TotalAmount));
                                                    }

                                                    AddTotalRow(
                                                        table,
                                                        "TOTAL",
                                                        $"{report.TotalCowQuantity:0.00}",
                                                        $"{report.TotalBuffaloQuantity:0.00}",
                                                        $"{report.TotalQuantity:0.00}",
                                                        Currency(
                                                            report.TotalAmount));
                                                });


                                        // ========================================
                                        // SESSION SUMMARY
                                        // ========================================

                                        content.Item()
                                            .PaddingTop(18)
                                            .Text("SESSION SUMMARY")
                                            .Bold()
                                            .FontSize(10);

                                        content.Item()
                                            .PaddingTop(5)
                                            .Table(
                                                table =>
                                                {
                                                    table.ColumnsDefinition(
                                                        columns =>
                                                        {
                                                            columns.RelativeColumn(2);
                                                            columns.RelativeColumn();
                                                            columns.RelativeColumn();
                                                            columns.RelativeColumn();
                                                            columns.RelativeColumn();
                                                        });

                                                    AddHeader(table, "Session");
                                                    AddHeader(table, "Cow L");
                                                    AddHeader(table, "Buffalo L");
                                                    AddHeader(table, "Total L");
                                                    AddHeader(table, "Amount");

                                                    AddCell(
                                                        table,
                                                        "Morning");

                                                    AddCell(
                                                        table,
                                                        $"{report.MorningCowQuantity:0.00}");

                                                    AddCell(
                                                        table,
                                                        $"{report.MorningBuffaloQuantity:0.00}");

                                                    AddCell(
                                                        table,
                                                        $"{report.MorningTotalQuantity:0.00}");

                                                    AddCell(
                                                        table,
                                                        Currency(
                                                            report.MorningAmount));

                                                    AddCell(
                                                        table,
                                                        "Evening");

                                                    AddCell(
                                                        table,
                                                        $"{report.EveningCowQuantity:0.00}");

                                                    AddCell(
                                                        table,
                                                        $"{report.EveningBuffaloQuantity:0.00}");

                                                    AddCell(
                                                        table,
                                                        $"{report.EveningTotalQuantity:0.00}");

                                                    AddCell(
                                                        table,
                                                        Currency(
                                                            report.EveningAmount));

                                                    AddTotalRow(
                                                        table,
                                                        "TOTAL",
                                                        $"{report.TotalCowQuantity:0.00}",
                                                        $"{report.TotalBuffaloQuantity:0.00}",
                                                        $"{report.TotalQuantity:0.00}",
                                                        Currency(
                                                            report.TotalAmount));
                                                });


                                        // ========================================
                                        // CUSTOMER COUNT
                                        // ========================================

                                        content.Item()
                                            .PaddingTop(15)
                                            .AlignRight()
                                            .Text(
                                                $"Customers: {report.CustomerCount}")
                                            .Bold();
                                    });

                            page.Footer()
                                .AlignCenter()
                                .Text(
                                    text =>
                                    {
                                        text.Span(
                                            "Sara Agro  •  Daily Report  •  Page ");

                                        text.CurrentPageNumber();

                                        text.Span(" / ");

                                        text.TotalPages();
                                    });
                        });
                });

        return document.GeneratePdf();
    }


    // =========================================================
    // MONTHLY REPORT PDF
    // =========================================================

    // =========================================================
    // MONTHLY REPORT PDF
    // =========================================================

    public byte[] GenerateMonthlyReportPdf(
        MonthlyReportResponse report,
        FinancialSummaryResponse financial)
    {
        var document =
            Document.Create(
                container =>
                {
                    container.Page(
                        page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(25);

                            page.DefaultTextStyle(
                                x => x.FontSize(8));

                            page.Header()
                                .Column(
                                    header =>
                                    {
                                        header.Item()
                                            .AlignCenter()
                                            .Text("SARA AGRO")
                                            .Bold()
                                            .FontSize(18);

                                        header.Item()
                                            .AlignCenter()
                                            .Text("Monthly Financial & Milk Collection Report")
                                            .Bold()
                                            .FontSize(13);

                                        header.Item()
                                            .AlignCenter()
                                            .PaddingTop(4)
                                            .Text(
                                                report.Month.ToString("MMMM yyyy"))
                                            .FontSize(10);
                                    });

                            page.Content()
                                .PaddingTop(15)
                                .Column(
                                    content =>
                                    {
                                        // ========================================
                                        // FINANCIAL SUMMARY
                                        // ========================================

                                        content.Item()
                                            .Text("FINANCIAL SUMMARY")
                                            .Bold()
                                            .FontSize(10);

                                        content.Item()
                                            .PaddingTop(5)
                                            .Table(
                                                table =>
                                                {
                                                    table.ColumnsDefinition(
                                                        columns =>
                                                        {
                                                            columns.RelativeColumn();
                                                            columns.RelativeColumn();
                                                            columns.RelativeColumn();
                                                            columns.RelativeColumn();
                                                        });

                                                    AddSummaryCell(
                                                        table,
                                                        "Revenue",
                                                        Currency(
                                                            financial.MonthlyRevenue));

                                                    AddSummaryCell(
                                                        table,
                                                        "Collection",
                                                        Currency(
                                                            financial.MonthlyCollection));

                                                    AddSummaryCell(
                                                        table,
                                                        "Expense",
                                                        Currency(
                                                            financial.MonthlyExpense));

                                                    AddSummaryCell(
                                                        table,
                                                        "Profit",
                                                        Currency(
                                                            financial.MonthlyProfit));
                                                });


                                        // ========================================
                                        // CUSTOMER-WISE MILK COLLECTION
                                        // ========================================

                                        content.Item()
                                            .PaddingTop(18)
                                            .Text("CUSTOMER-WISE MILK COLLECTION")
                                            .Bold()
                                            .FontSize(10);

                                        content.Item()
                                            .PaddingTop(5)
                                            .Table(
                                                table =>
                                                {
                                                    table.ColumnsDefinition(
                                                        columns =>
                                                        {
                                                            columns.ConstantColumn(50);
                                                            columns.RelativeColumn(2);
                                                            columns.ConstantColumn(70);
                                                            columns.ConstantColumn(75);
                                                            columns.ConstantColumn(70);
                                                            columns.ConstantColumn(85);
                                                        });

                                                    AddHeader(table, "Code");
                                                    AddHeader(table, "Customer");
                                                    AddHeader(table, "Cow L");
                                                    AddHeader(table, "Buffalo L");
                                                    AddHeader(table, "Total L");
                                                    AddHeader(table, "Amount");

                                                    foreach (
                                                        var customer
                                                        in report.Customers)
                                                    {
                                                        AddCell(
                                                            table,
                                                            customer.CustomerCode);

                                                        AddCell(
                                                            table,
                                                            customer.CustomerName);

                                                        AddCell(
                                                            table,
                                                            $"{customer.CowQuantity:0.00}");

                                                        AddCell(
                                                            table,
                                                            $"{customer.BuffaloQuantity:0.00}");

                                                        AddCell(
                                                            table,
                                                            $"{customer.TotalQuantity:0.00}");

                                                        AddCell(
                                                            table,
                                                            Currency(
                                                                customer.TotalAmount));
                                                    }

                                                    AddTotalRow(
                                                        table,
                                                        "TOTAL",
                                                        $"{report.CowQuantity:0.00}",
                                                        $"{report.BuffaloQuantity:0.00}",
                                                        $"{report.TotalQuantity:0.00}",
                                                        Currency(
                                                            report.TotalAmount));
                                                });


                                        // ========================================
                                        // MONTHLY COLLECTION SUMMARY
                                        // ========================================

                                        content.Item()
                                            .PaddingTop(18)
                                            .Text("MONTHLY COLLECTION SUMMARY")
                                            .Bold()
                                            .FontSize(10);

                                        content.Item()
                                            .PaddingTop(5)
                                            .Table(
                                                table =>
                                                {
                                                    table.ColumnsDefinition(
                                                        columns =>
                                                        {
                                                            columns.RelativeColumn();
                                                            columns.RelativeColumn();
                                                        });

                                                    AddSummaryCell(
                                                        table,
                                                        "Customers",
                                                        report.CustomerCount.ToString());

                                                    AddSummaryCell(
                                                        table,
                                                        "Days Recorded",
                                                        report.DaysRecorded.ToString());

                                                    AddSummaryCell(
                                                        table,
                                                        "Cow Quantity",
                                                        $"{report.CowQuantity:0.00} L");

                                                    AddSummaryCell(
                                                        table,
                                                        "Buffalo Quantity",
                                                        $"{report.BuffaloQuantity:0.00} L");

                                                    AddSummaryCell(
                                                        table,
                                                        "Total Quantity",
                                                        $"{report.TotalQuantity:0.00} L");

                                                    AddSummaryCell(
                                                        table,
                                                        "Total Amount",
                                                        Currency(
                                                            report.TotalAmount));
                                                });
                                    });

                            page.Footer()
                                .AlignCenter()
                                .Text(
                                    text =>
                                    {
                                        text.Span(
                                            "Sara Agro  •  Monthly Report  •  Page ");

                                        text.CurrentPageNumber();

                                        text.Span(" / ");

                                        text.TotalPages();
                                    });
                        });
                });

        return document.GeneratePdf();
    }


    // =========================================================
    // PENDING AMOUNT PDF
    // =========================================================

    public byte[] GeneratePendingAmountReportPdf(
        PendingAmountReportResponse report)
    {
        var document =
            Document.Create(
                container =>
                {
                    container.Page(
                        page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(25);

                            page.DefaultTextStyle(
                                x => x.FontSize(8));

                            page.Header()
                                .Column(
                                    header =>
                                    {
                                        header.Item()
                                            .AlignCenter()
                                            .Text("SARA AGRO")
                                            .Bold()
                                            .FontSize(18);

                                        header.Item()
                                            .AlignCenter()
                                            .Text("Pending Amount Report")
                                            .Bold()
                                            .FontSize(13);

                                        header.Item()
                                            .AlignCenter()
                                            .PaddingTop(4)
                                            .Text(
                                                $"As of {report.AsOfDate:dd MMM yyyy}")
                                            .FontSize(9);
                                    });

                            page.Content()
                                .PaddingTop(15)
                                .Column(
                                    content =>
                                    {
                                        content.Item()
                                            .Table(
                                                table =>
                                                {
                                                    table.ColumnsDefinition(
                                                        columns =>
                                                        {
                                                            columns.ConstantColumn(45);
                                                            columns.RelativeColumn(1.7f);
                                                            columns.ConstantColumn(75);
                                                            columns.ConstantColumn(65);
                                                            columns.ConstantColumn(70);
                                                            columns.ConstantColumn(70);
                                                            columns.ConstantColumn(75);
                                                        });

                                                    AddHeader(table, "Code");
                                                    AddHeader(table, "Customer");
                                                    AddHeader(table, "Bill No.");
                                                    AddHeader(table, "Bill Date");
                                                    AddHeader(table, "Payable");
                                                    AddHeader(table, "Paid");
                                                    AddHeader(table, "Pending");

                                                    foreach (
                                                        var customer
                                                        in report.Customers)
                                                    {
                                                        AddCell(
                                                            table,
                                                            customer.CustomerCode);

                                                        AddCell(
                                                            table,
                                                            customer.CustomerName);

                                                        AddCell(
                                                            table,
                                                            customer.BillNumber);

                                                        AddCell(
                                                            table,
                                                            customer.BillDate.ToString("dd/MM/yy"));

                                                        AddCell(
                                                            table,
                                                            Currency(customer.TotalPayable));

                                                        AddCell(
                                                            table,
                                                            Currency(customer.PaidAmount));

                                                        AddCell(
                                                            table,
                                                            Currency(customer.PendingAmount));
                                                    }

                                                    AddTotalRow(
                                                        table,
                                                        "TOTAL",
                                                        "",
                                                        "",
                                                        "",
                                                        Currency(report.TotalPendingAmount));
                                                });

                                        content.Item()
                                            .PaddingTop(18)
                                            .AlignRight()
                                            .Text(
                                                $"Customers Pending: {report.CustomerCount}")
                                            .Bold();

                                        content.Item()
                                            .PaddingTop(5)
                                            .AlignRight()
                                            .Text(
                                                $"TOTAL PENDING: {Currency(report.TotalPendingAmount)}")
                                            .Bold()
                                            .FontSize(12);
                                    });

                            page.Footer()
                                .AlignCenter()
                                .Text(
                                    text =>
                                    {
                                        text.Span("Sara Agro  •  Pending Report  •  Page ");
                                        text.CurrentPageNumber();
                                        text.Span(" / ");
                                        text.TotalPages();
                                    });
                        });
                });

        return document.GeneratePdf();
    }


    // =========================================================
    // ALL PAYMENTS PDF
    // =========================================================

    public byte[] GenerateAllPaymentReportPdf(
        AllPaymentReportResponse report)
    {
        var document =
            Document.Create(
                container =>
                {
                    container.Page(
                        page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(25);

                            page.DefaultTextStyle(
                                x => x.FontSize(8));

                            page.Header()
                                .Column(
                                    header =>
                                    {
                                        header.Item()
                                            .AlignCenter()
                                            .Text("SARA AGRO")
                                            .Bold()
                                            .FontSize(18);

                                        header.Item()
                                            .AlignCenter()
                                            .Text("All Payment Report")
                                            .Bold()
                                            .FontSize(13);

                                        header.Item()
                                            .AlignCenter()
                                            .PaddingTop(4)
                                            .Text(
                                                $"{report.FromDate:dd MMM yyyy} - {report.ToDate:dd MMM yyyy}")
                                            .FontSize(9);
                                    });

                            page.Content()
                                .PaddingTop(15)
                                .Column(
                                    content =>
                                    {
                                        content.Item()
                                            .Table(
                                                table =>
                                                {
                                                    table.ColumnsDefinition(
                                                        columns =>
                                                        {
                                                            columns.ConstantColumn(55);
                                                            columns.ConstantColumn(45);
                                                            columns.RelativeColumn(1.6f);
                                                            columns.ConstantColumn(75);
                                                            columns.ConstantColumn(55);
                                                            columns.RelativeColumn(1.2f);
                                                            columns.ConstantColumn(75);
                                                        });

                                                    AddHeader(table, "Date");
                                                    AddHeader(table, "Code");
                                                    AddHeader(table, "Customer");
                                                    AddHeader(table, "Bill No.");
                                                    AddHeader(table, "Mode");
                                                    AddHeader(table, "Reference");
                                                    AddHeader(table, "Amount");

                                                    foreach (
                                                        var payment
                                                        in report.Payments)
                                                    {
                                                        AddCell(
                                                            table,
                                                            payment.PaymentDate.ToString("dd/MM/yy"));

                                                        AddCell(
                                                            table,
                                                            payment.CustomerCode);

                                                        AddCell(
                                                            table,
                                                            payment.CustomerName);

                                                        AddCell(
                                                            table,
                                                            payment.BillNumber);

                                                        AddCell(
                                                            table,
                                                            payment.PaymentMode);

                                                        AddCell(
                                                            table,
                                                            string.IsNullOrWhiteSpace(
                                                                payment.ReferenceNumber)
                                                                ? "-"
                                                                : payment.ReferenceNumber);

                                                        AddCell(
                                                            table,
                                                            Currency(payment.Amount));
                                                    }

                                                    AddPaymentTotalRow(
                                                        table,
                                                        report.TotalPaymentAmount);
                                                });

                                        content.Item()
                                            .PaddingTop(18)
                                            .AlignRight()
                                            .Text(
                                                $"Payment Count: {report.PaymentCount}")
                                            .Bold();

                                        content.Item()
                                            .PaddingTop(5)
                                            .AlignRight()
                                            .Text(
                                                $"TOTAL PAYMENTS: {Currency(report.TotalPaymentAmount)}")
                                            .Bold()
                                            .FontSize(12);
                                    });

                            page.Footer()
                                .AlignCenter()
                                .Text(
                                    text =>
                                    {
                                        text.Span("Sara Agro  •  Payment Report  •  Page ");
                                        text.CurrentPageNumber();
                                        text.Span(" / ");
                                        text.TotalPages();
                                    });
                        });
                });

        return document.GeneratePdf();
    }


    // =========================================================
    // COMMON HELPERS
    // =========================================================

    private static string Currency(decimal value)
    {
        return $"₹{value:0.00}";
    }


    private static void AddHeader(
        TableDescriptor table,
        string text)
    {
        table.Cell()
            .Background("#193522")
            .Padding(5)
            .Text(text)
            .FontColor("#FFFFFF")
            .Bold()
            .FontSize(7);
    }


    private static void AddCell(
        TableDescriptor table,
        string text)
    {
        table.Cell()
            .BorderBottom(0.5f)
            .BorderColor("#D5D5D5")
            .Padding(4)
            .Text(text)
            .FontSize(7);
    }


    private static void AddSummaryCell(
        TableDescriptor table,
        string title,
        string value)
    {
        table.Cell()
            .Border(0.5f)
            .BorderColor("#D5D5D5")
            .Padding(6)
            .Column(
                column =>
                {
                    column.Item()
                        .Text(title)
                        .FontSize(7);

                    column.Item()
                        .Text(value)
                        .Bold()
                        .FontSize(9);
                });
    }


    private static void AddTotal(
        ColumnDescriptor column,
        string label,
        string value)
    {
        column.Item()
            .Text($"{label}: {value}")
            .FontSize(8);
    }


    private static void AddTotalRow(
        TableDescriptor table,
        string label,
        string value1,
        string value2,
        string value3,
        string value4)
    {
        AddCell(table, label);
        AddCell(table, "");

        AddCell(table, value1);
        AddCell(table, value2);
        AddCell(table, value3);
        AddCell(table, value4);
    }


    private static void AddPaymentTotalRow(
        TableDescriptor table,
        decimal amount)
    {
        AddCell(table, "TOTAL");
        AddCell(table, "");
        AddCell(table, "");
        AddCell(table, "");
        AddCell(table, "");
        AddCell(table, "");
        AddCell(table, Currency(amount));
    }


    private static string BuildLedgerDescription(
        CustomerLedgerEntry entry)
    {
        if (entry.Type == "Bill")
        {
            return
                $"{entry.Description}\n" +
                $"Payable {Currency(entry.TotalPayable)}  " +
                $"Paid {Currency(entry.PaidAmount)}  " +
                $"Balance {Currency(entry.BillBalance)}";
        }


        if (entry.Type == "Payment")
        {
            var text =
                entry.Description;


            if (!string.IsNullOrWhiteSpace(
                    entry.PaymentMode))
            {
                text +=
                    $" • {entry.PaymentMode}";
            }


            if (!string.IsNullOrWhiteSpace(
                    entry.ReferenceNumber))
            {
                text +=
                    $" • {entry.ReferenceNumber}";
            }


            return text;
        }


        return entry.Description;
    }
}