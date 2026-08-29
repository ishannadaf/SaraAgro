namespace SaraAgro.Api.DTOs.MilkDistribution;

public class MilkDistributionMonthlySummaryResponse
{
    public DateTime Month { get; set; }

    public int ActiveCustomerCount { get; set; }

    public decimal CowQuantity { get; set; }

    public decimal BuffaloQuantity { get; set; }

    public decimal TotalQuantity { get; set; }

    public decimal TotalAmount { get; set; }

    public int DaysRecorded { get; set; }
}