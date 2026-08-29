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
using SaraAgro.Api.Services.Sms;
using SaraAgro.Api.Interfaces.Reports;
using SaraAgro.Api.Services.Reports;
using System.Text;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.Configure<OtpSettings>(
    builder.Configuration.GetSection("OtpSettings"));
builder.Services.AddScoped<ISmsService, SmsService>();

builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IRateMasterService, RateMasterService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IMilkDistributionService, MilkDistributionService>();
builder.Services.AddScoped<IRateGroupService, RateGroupService>();
builder.Services.AddScoped<IBillingService, BillingService>();

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddDbContext<SaraAgroDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(
            builder.Configuration.GetConnectionString("DefaultConnection")
        )));

builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ReportPdfService>();
builder.Services.AddScoped<IReportsService,ReportsService>();

var jwtSettings = builder.Configuration
    .GetSection("JwtSettings")
    .Get<SaraAgro.Api.Authentication.JwtSettings>()
    ?? throw new InvalidOperationException(
        "JWT settings are not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),

            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapControllers();

app.Run();
