using Microsoft.EntityFrameworkCore;
using SaraAgro.Api.Authentication;
using SaraAgro.Api.Data;
using SaraAgro.Api.DTOs.Authentication;
using SaraAgro.Api.Interfaces.Authentication;
using SaraAgro.Api.Interfaces.Otp;
using SaraAgro.Api.Models;
using ClientModel = SaraAgro.Api.Models.Client;
namespace SaraAgro.Api.Services.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly SaraAgroDbContext _dbContext;
    private readonly PasswordService _passwordService;
    private readonly IOtpService _otpService;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthenticationService(
        SaraAgroDbContext dbContext,
        PasswordService passwordService,
        IOtpService otpService,
        IJwtTokenService jwtTokenService)
    {
        _dbContext = dbContext;
        _passwordService = passwordService;
        _otpService = otpService;
        _jwtTokenService = jwtTokenService;
    }


    // =========================================================
    // REGISTER
    // =========================================================

    public async Task<RegisterResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var mobileNumber =
            request.MobileNumber.Trim();

        var businessName =
            request.BusinessName.Trim();

        var ownerName =
            request.OwnerName.Trim();

        var email =
            string.IsNullOrWhiteSpace(request.Email)
                ? null
                : request.Email.Trim();

        // -----------------------------------------------------
        // BASIC VALIDATION
        // -----------------------------------------------------

        if (string.IsNullOrWhiteSpace(businessName))
        {
            throw new ArgumentException(
                "Business name is required.");
        }

        if (string.IsNullOrWhiteSpace(ownerName))
        {
            throw new ArgumentException(
                "Owner name is required.");
        }

        if (mobileNumber.Length != 10 ||
            !mobileNumber.All(char.IsDigit))
        {
            throw new ArgumentException(
                "Please enter a valid 10-digit mobile number.");
        }


        // -----------------------------------------------------
        // CHECK MOBILE
        // -----------------------------------------------------

        var mobileExists =
            await _dbContext.Users
                .AnyAsync(
                    x => x.MobileNumber == mobileNumber,
                    cancellationToken);

        if (mobileExists)
        {
            throw new InvalidOperationException(
                "An account with this mobile number already exists.");
        }


        // -----------------------------------------------------
        // VERIFY OTP
        // -----------------------------------------------------

        var otpVerified =
            await _otpService.VerifyOtpAsync(
                new DTOs.Otp.VerifyOtpRequest
                {
                    MobileNumber = mobileNumber,
                    Otp = request.Otp,
                    Purpose = OtpPurposes.Signup
                },
                cancellationToken);



        //if (!otpVerified)
        //{
        //    throw new InvalidOperationException(
        //        "Invalid or expired OTP.");
        //}


        // -----------------------------------------------------
        // HASH PASSWORD
        // -----------------------------------------------------

        var passwordHash =
            _passwordService.HashPassword(
                request.Password);


        // =====================================================
        // CREATE CLIENT + USER
        // =====================================================

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            // -------------------------------------------------
            // CREATE CLIENT
            // -------------------------------------------------

            var client =
                new ClientModel
                {
                    Name =
                        businessName,

                    PhoneNumber =
                        mobileNumber,

                    Email =
                        email,

                    IsActive =
                        true,

                    City =
                        request.City?.Trim(),

                    CreatedAt =
                        DateTime.UtcNow
                };


            _dbContext.Clients.Add(client);

            await _dbContext.SaveChangesAsync(
                cancellationToken);


            // -------------------------------------------------
            // CREATE OWNER USER
            // -------------------------------------------------

            var user =
                new User
                {
                    ClientId =
                        client.Id,

                    FullName =
                        ownerName,

                    MobileNumber =
                        mobileNumber,

                    Email =
                        email,

                    PasswordHash =
                        passwordHash,

                    Role =
                        "Owner",

                    IsActive =
                        true,

                    CreatedAt =
                        DateTime.UtcNow
                };


            _dbContext.Users.Add(user);

            await _dbContext.SaveChangesAsync(
                cancellationToken);


            // -------------------------------------------------
            // COMMIT
            // -------------------------------------------------

            await transaction.CommitAsync(
                cancellationToken);


            return new RegisterResponse
            {
                Success = true,

                Message =
                    "Registration successful.",

                UserId =
                    user.Id
            };
        }
        catch
        {
            await transaction.RollbackAsync(
                cancellationToken);

            throw;
        }
    }


    // =========================================================
    // LOGIN
    // =========================================================

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var mobileNumber =
            request.MobileNumber.Trim();


        // -----------------------------------------------------
        // FIND USER
        // -----------------------------------------------------

        var user =
            await _dbContext.Users
                .Include(x => x.Client)
                .FirstOrDefaultAsync(
                    x => x.MobileNumber == mobileNumber,
                    cancellationToken);


        if (user == null)
        {
            throw new UnauthorizedAccessException(
                "Invalid mobile number or password.");
        }


        // -----------------------------------------------------
        // USER ACTIVE
        // -----------------------------------------------------

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException(
                "Your account is inactive. Please contact your administrator.");
        }


        // -----------------------------------------------------
        // PASSWORD
        // -----------------------------------------------------

        var passwordValid =
            _passwordService.VerifyPassword(
                request.Password,
                user.PasswordHash);


        if (!passwordValid)
        {
            throw new UnauthorizedAccessException(
                "Invalid mobile number or password.");
        }


        // -----------------------------------------------------
        // CLIENT ACTIVE
        // -----------------------------------------------------

        if (user.Client == null ||
            !user.Client.IsActive)
        {
            throw new UnauthorizedAccessException(
                "Your client account is inactive.");
        }


        // -----------------------------------------------------
        // ACCESS TOKEN
        // -----------------------------------------------------

        var accessToken =
            _jwtTokenService.GenerateAccessToken(
                user,
                user.Client.Name);


        var accessTokenExpiresAt =
            _jwtTokenService.GetAccessTokenExpiration();


        // -----------------------------------------------------
        // REFRESH TOKEN
        // -----------------------------------------------------

        var refreshToken =
            _jwtTokenService.GenerateRefreshToken();


        var refreshTokenHash =
            _jwtTokenService.HashRefreshToken(
                refreshToken);


        var refreshTokenEntity =
            new RefreshToken
            {
                UserId =
                    user.Id,

                TokenHash =
                    refreshTokenHash,

                ExpiresAt =
                    DateTime.UtcNow.AddDays(7),

                CreatedAt =
                    DateTime.UtcNow
            };


        _dbContext.RefreshTokens.Add(
            refreshTokenEntity);


        await _dbContext.SaveChangesAsync(
            cancellationToken);


        // -----------------------------------------------------
        // RESPONSE
        // -----------------------------------------------------

        return new LoginResponse
        {
            AccessToken =
                accessToken,

            RefreshToken =
                refreshToken,

            AccessTokenExpiresAt =
                accessTokenExpiresAt,

            UserId =
                user.Id,

            FullName =
                user.FullName,

            MobileNumber =
                user.MobileNumber,

            Role =
                user.Role,

            ClientId =
                user.ClientId,

            ClientName =
                user.Client.Name
        };
    }
}