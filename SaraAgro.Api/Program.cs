using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SaraAgro.Api.Authentication;
using SaraAgro.Api.Data;
using SaraAgro.Api.Interfaces.Authentication;
using SaraAgro.Api.Interfaces.Billing;
using SaraAgro.Api.Interfaces.Client;
using SaraAgro.Api.Interfaces.Customer;
using SaraAgro.Api.Interfaces.MilkDistribution;
using SaraAgro.Api.Interfaces.Otp;
using SaraAgro.Api.Interfaces.RateGroup;
using SaraAgro.Api.Interfaces.RateMaster;
using SaraAgro.Api.Interfaces.Reports;
using SaraAgro.Api.Interfaces.Sms;
using SaraAgro.Api.Services;
using SaraAgro.Api.Services.Authentication;
using SaraAgro.Api.Services.Billing;
using SaraAgro.Api.Services.Client;
using SaraAgro.Api.Services.Customer;
using SaraAgro.Api.Services.MilkDistribution;
using SaraAgro.Api.Services.Otp;
using SaraAgro.Api.Services.RateGroup;
using SaraAgro.Api.Services.RateMaster;
using SaraAgro.Api.Services.Reports;
using SaraAgro.Api.Services.Sms;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// =========================================================
// SERVICES
// =========================================================

// Authentication
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<PasswordService>();

// JWT
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

// Client
builder.Services.AddScoped<IClientService, ClientService>();

// OTP / SMS
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<ISmsService, SmsService>();

builder.Services.Configure<OtpSettings>(
    builder.Configuration.GetSection("OtpSettings"));

// Rate Master
builder.Services.AddScoped<IRateMasterService, RateMasterService>();

// Rate Group
builder.Services.AddScoped<IRateGroupService, RateGroupService>();

// Customers
builder.Services.AddScoped<ICustomerService, CustomerService>();

// Milk Distribution
builder.Services.AddScoped<IMilkDistributionService, MilkDistributionService>();

// Billing
builder.Services.AddScoped<IBillingService, BillingService>();

// Reports
builder.Services.AddScoped<IReportsService, ReportsService>();
builder.Services.AddScoped<ReportPdfService>();


// =========================================================
// DATABASE
// =========================================================

var connectionString =
    builder.Configuration.GetConnectionString(
        "DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Database connection string 'DefaultConnection' is not configured.");
}

builder.Services.AddDbContext<SaraAgroDbContext>(
    options =>
        options.UseMySql(
            connectionString,
            ServerVersion.AutoDetect(connectionString)));


// =========================================================
// JWT CONFIGURATION
// =========================================================

var jwtSettings =
    builder.Configuration
        .GetSection("JwtSettings")
        .Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "JWT settings are not configured.");

if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
{
    throw new InvalidOperationException(
        "JWT SecretKey is not configured.");
}

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    jwtSettings.Issuer,

                ValidAudience =
                    jwtSettings.Audience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtSettings.SecretKey)),

                ClockSkew =
                    TimeSpan.FromSeconds(30)
            };
    });

builder.Services.AddAuthorization();


// =========================================================
// MVC / API CONTROLLERS
// =========================================================

builder.Services.AddControllers();


// =========================================================
// SWAGGER
// =========================================================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// =========================================================
// OPENAPI
// =========================================================

builder.Services.AddOpenApi();


// =========================================================
// BUILD
// =========================================================

var app = builder.Build();


// =========================================================
// SWAGGER
// =========================================================

// Keep Swagger enabled so we can test the
// production Railway deployment.
app.UseSwagger();

app.UseSwaggerUI();


// =========================================================
// AUTHENTICATION / AUTHORIZATION
// =========================================================

app.UseAuthentication();

app.UseAuthorization();


// =========================================================
// API CONTROLLERS
// =========================================================

// IMPORTANT:
// This was missing in your previous Program.cs.
// Without this, controller endpoints return 404.
app.MapControllers();


// =========================================================
// HEALTH CHECK
// =========================================================

app.MapGet(
    "/health",
    () => Results.Ok(
        new
        {
            status = "healthy",
            service = "SaraAgro.Api"
        }));


// =========================================================
// START APPLICATION
// =========================================================

app.Run();