using Microsoft.Extensions.Options;
using SaraAgro.Api.Authentication;
using SaraAgro.Api.DTOs.Otp;
using SaraAgro.Api.Interfaces.Otp;
using SaraAgro.Api.Interfaces.Sms;
using SaraAgro.Api.Services.Sms;
using System.Security.Cryptography;
using System.Text;
namespace SaraAgro.Api.Services.Otp;

using Microsoft.EntityFrameworkCore;
using SaraAgro.Api.Data;
using SaraAgro.Api.Models;

public class OtpService : IOtpService
{
    private readonly OtpSettings _settings;
    private readonly ISmsService _smsService;
    private readonly SaraAgroDbContext _dbContext;

    public OtpService(IOptions<OtpSettings> options, ISmsService smsService, SaraAgroDbContext dbContext)
    {
        _settings = options.Value;
        _smsService = smsService;
        _dbContext = dbContext;
    }
    public async Task<SendOtpResponse> SendOtpAsync(
    SendOtpRequest request,
    CancellationToken cancellationToken = default)
    {
        var mobileNumber = request.MobileNumber.Trim();
        var purpose = request.Purpose.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(mobileNumber))
        {
            throw new ArgumentException(
                "Mobile number is required.");
        }

        if (purpose != OtpPurposes.Signup &&
            purpose != OtpPurposes.PasswordReset)
        {
            throw new ArgumentException(
                "Invalid OTP purpose.");
        }

        var latestOtp = await _dbContext.OtpVerifications
            .Where(x =>
                x.MobileNumber == mobileNumber &&
                x.Purpose == purpose)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestOtp != null)
        {
            var cooldownUntil = latestOtp.CreatedAt.AddSeconds(
                _settings.ResendCooldownSeconds);

            if (DateTime.UtcNow < cooldownUntil)
            {
                throw new InvalidOperationException(
                    "Please wait before requesting another OTP.");
            }
        }

        // Invalidate previous active OTPs
        var previousOtps = await _dbContext.OtpVerifications
            .Where(x =>
                x.MobileNumber == mobileNumber &&
                x.Purpose == purpose &&
                !x.IsVerified &&
                x.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var previousOtp in previousOtps)
        {
            previousOtp.ExpiresAt = DateTime.UtcNow;
        }

        var otp = GenerateOtp();
        var otpHash = HashOtp(otp);

        var expiresAt = DateTime.UtcNow.AddMinutes(
            _settings.ExpirationMinutes);

        var otpVerification = new OtpVerification
        {
            MobileNumber = mobileNumber,
            OtpHash = otpHash,
            Purpose = purpose,
            ExpiresAt = expiresAt,
            AttemptCount = 0,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.OtpVerifications.Add(otpVerification);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var message =
            $"Your SaraAgro verification OTP is {otp}. " +
            $"It expires in {_settings.ExpirationMinutes} minutes.";

        await _smsService.SendAsync(
            mobileNumber,
            message,
            cancellationToken);

        return new SendOtpResponse
        {
            Success = true,
            Message = "OTP sent successfully.",
            ExpiresAt = expiresAt
        };
    }

    public async Task<bool> VerifyOtpAsync(
    VerifyOtpRequest request,
    CancellationToken cancellationToken = default)
    {
        var mobileNumber = request.MobileNumber.Trim();
        var purpose = request.Purpose.Trim().ToUpperInvariant();

        var otpVerification = await _dbContext.OtpVerifications
            .Where(x =>
                x.MobileNumber == mobileNumber &&
                x.Purpose == purpose &&
                !x.IsVerified)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (otpVerification == null)
        {
            return false;
        }

        // OTP expired
        if (DateTime.UtcNow > otpVerification.ExpiresAt)
        {
            return false;
        }

        // Too many attempts
        if (otpVerification.AttemptCount >= _settings.MaxAttempts)
        {
            return false;
        }

        otpVerification.AttemptCount++;

        var enteredOtpHash = HashOtp(request.Otp.Trim());

        if (!string.Equals(
                enteredOtpHash,
                otpVerification.OtpHash,
                StringComparison.OrdinalIgnoreCase))
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            return false;
        }

        otpVerification.IsVerified = true;
        otpVerification.VerifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static string GenerateOtp()
    {
        return RandomNumberGenerator
            .GetInt32(100000, 1000000)
            .ToString();
    }

    private static string HashOtp(string otp)
    {
        var hash = SHA256.HashData(
            Encoding.UTF8.GetBytes(otp));

        return Convert.ToHexString(hash);
    }
}