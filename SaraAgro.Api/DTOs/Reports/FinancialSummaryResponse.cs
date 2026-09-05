namespace SaraAgro.Api.DTOs.Reports;

public class FinancialSummaryResponse
{
    // =========================================================
    // SELECTED DATE
    // =========================================================

    public DateTime Date { get; set; }

    public decimal TodayRevenue { get; set; }

    public decimal TodayCollection { get; set; }

    public decimal TodayExpense { get; set; }

    public decimal TodayProfit { get; set; }


    // =========================================================
    // CURRENT MONTH
    // =========================================================

    public DateTime Month { get; set; }

    public decimal MonthlyRevenue { get; set; }

    public decimal MonthlyCollection { get; set; }

    public decimal MonthlyExpense { get; set; }

    public decimal MonthlyProfit { get; set; }
}