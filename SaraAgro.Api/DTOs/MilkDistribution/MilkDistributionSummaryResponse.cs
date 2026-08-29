namespace SaraAgro.Api.DTOs.MilkDistribution;

public class MilkDistributionSummaryResponse
{
    public DateTime Date { get; set; }

    public decimal MorningCowQuantity { get; set; }

    public decimal MorningBuffaloQuantity { get; set; }

    public decimal EveningCowQuantity { get; set; }

    public decimal EveningBuffaloQuantity { get; set; }

    public decimal MorningTotalQuantity { get; set; }

    public decimal EveningTotalQuantity { get; set; }

    public decimal TotalQuantity { get; set; }

    public decimal MorningAmount { get; set; }

    public decimal EveningAmount { get; set; }

    public decimal TotalAmount { get; set; }
}