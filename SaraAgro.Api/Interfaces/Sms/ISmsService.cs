namespace SaraAgro.Api.Interfaces.Sms;

public interface ISmsService
{
    Task SendAsync(
        string mobileNumber,
        string message,
        CancellationToken cancellationToken = default);
}