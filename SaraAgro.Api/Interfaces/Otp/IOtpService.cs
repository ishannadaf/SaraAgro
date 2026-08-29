using SaraAgro.Api.DTOs.Otp;

namespace SaraAgro.Api.Interfaces.Otp;

public interface IOtpService
{
    Task<SendOtpResponse> SendOtpAsync(
        SendOtpRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> VerifyOtpAsync(
        VerifyOtpRequest request,
        CancellationToken cancellationToken = default);
}