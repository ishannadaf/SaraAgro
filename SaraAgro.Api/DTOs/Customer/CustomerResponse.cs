namespace SaraAgro.Api.DTOs.Customer;

public class CustomerResponse
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    // Customer belongs to a Route / Rate Group
    // and is not tied to Cow or Buffalo.
    public int RateGroupId { get; set; }

    public string RateGroupName { get; set; } = string.Empty;

    public string CustomerCode { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string MobileNumber { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}