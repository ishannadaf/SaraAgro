namespace SaraAgro.Api.DTOs.Customer;

public class CustomerAccountSummaryResponse
{
    public int CustomerId { get; set; }

    public string CustomerCode { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public int RateGroupId { get; set; }

    public string RateGroupName { get; set; } = string.Empty;

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public decimal MorningQuantity { get; set; }

    public decimal EveningQuantity { get; set; }

    public decimal TotalQuantity { get; set; }

    public decimal TotalAmount { get; set; }
}