using SaraAgro.Api.Interfaces.Sms;

namespace SaraAgro.Api.Services.Sms;

public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;

    public SmsService(ILogger<SmsService> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(
        string mobileNumber,
        string message,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Development SMS to {MobileNumber}: {Message}",
            mobileNumber,
            message);

        return Task.CompletedTask;
    }
}