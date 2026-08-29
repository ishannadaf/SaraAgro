namespace SaraAgro.Api.DTOs.Billing;

public class GenerateBillRequest
{
    public int CustomerId { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }
}