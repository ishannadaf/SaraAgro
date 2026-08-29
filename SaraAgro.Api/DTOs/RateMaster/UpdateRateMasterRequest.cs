namespace SaraAgro.Api.DTOs.RateMaster;

public class UpdateRateMasterRequest
{
    public int RateGroupId { get; set; }

    public string MilkType { get; set; } = string.Empty;

    public decimal Rate { get; set; }

    public DateTime EffectiveDate { get; set; }
}