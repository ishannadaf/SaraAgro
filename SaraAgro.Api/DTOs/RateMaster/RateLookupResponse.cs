namespace SaraAgro.Api.DTOs.RateMaster;

public class RateLookupResponse
{
    public int RateMasterId { get; set; }

    public int RateGroupId { get; set; }

    public string RateGroupName { get; set; } = string.Empty;

    public string MilkType { get; set; } = string.Empty;

    public decimal Rate { get; set; }

    public DateTime EffectiveDate { get; set; }
}