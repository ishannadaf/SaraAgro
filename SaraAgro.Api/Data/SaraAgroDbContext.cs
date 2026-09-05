using Microsoft.EntityFrameworkCore;
using SaraAgro.Api.Models;

namespace SaraAgro.Api.Data;

public class SaraAgroDbContext : DbContext
{
    public SaraAgroDbContext(DbContextOptions<SaraAgroDbContext> options)
        : base(options)
    {
    }

    public DbSet<Client> Clients => Set<Client>();

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();
    public DbSet<RateMaster> RateMasters => Set<RateMaster>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<MilkDistribution> MilkDistributions => Set<MilkDistribution>();
    public DbSet<RateGroup> RateGroups { get; set; }

    public DbSet<Bill> Bills => Set<Bill>();

    public DbSet<BillPayment> BillPayments => Set<BillPayment>();
    public DbSet<ExpenseAccount> ExpenseAccounts => Set<ExpenseAccount>();
    public DbSet<Expense> Expenses => Set<Expense>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Bill>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.BillNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.MilkAmount)
                .HasPrecision(12, 2);

            entity.Property(x => x.PreviousOutstanding)
                .HasPrecision(12, 2);

            entity.Property(x => x.TotalPayable)
                .HasPrecision(12, 2);

            entity.Property(x => x.PaidAmount)
                .HasPrecision(12, 2);

            entity.Property(x => x.BalanceAmount)
                .HasPrecision(12, 2);

            entity.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(30);

            entity.HasIndex(x => new
            {
                x.ClientId,
                x.BillNumber
            })
            .IsUnique();

            entity.HasIndex(x => new
            {
                x.ClientId,
                x.CustomerId,
                x.FromDate,
                x.ToDate
            })
            .IsUnique();

            entity.HasOne(x => x.Client)
                .WithMany()
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        modelBuilder.Entity<BillPayment>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Amount)
                .HasPrecision(12, 2);

            entity.Property(x => x.PaymentMode)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(x => x.ReferenceNumber)
                .HasMaxLength(100);

            entity.Property(x => x.Notes)
                .HasMaxLength(500);

            entity.HasOne(x => x.Client)
                .WithMany()
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Bill)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.BillId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new
            {
                x.ClientId,
                x.BillId
            });
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(x => x.Code)
                .HasMaxLength(50);

            entity.Property(x => x.PhoneNumber)
                .HasMaxLength(20);

            entity.Property(x => x.Email)
                .HasMaxLength(150);

            entity.HasIndex(x => x.Code)
                .IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.FullName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(x => x.MobileNumber)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(x => x.Email)
                .HasMaxLength(150);

            entity.Property(x => x.PasswordHash)
                .IsRequired();

            entity.Property(x => x.Role)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(x => x.MobileNumber)
                .IsUnique();

            entity.HasOne(x => x.Client)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.TokenHash)
                .IsRequired()
                .HasMaxLength(128);

            entity.HasIndex(x => x.TokenHash)
                .IsUnique();

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OtpVerification>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.MobileNumber)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(x => x.OtpHash)
                .IsRequired()
                .HasMaxLength(128);

            entity.Property(x => x.Purpose)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(x => new
            {
                x.MobileNumber,
                x.Purpose
            });

            entity.HasIndex(x => x.ExpiresAt);
        });

        modelBuilder.Entity<RateMaster>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.RateGroupId)
                .IsRequired();

            entity.Property(x => x.MilkType)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(x => x.Rate)
                .HasPrecision(10, 2);

            entity.Property(x => x.EffectiveDate)
                .IsRequired();

            entity.HasIndex(x => new
            {
                x.ClientId,
                x.RateGroupId,
                x.MilkType,
                x.EffectiveDate
            });

            entity.HasOne(x => x.Client)
                .WithMany()
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RateGroup)
                .WithMany(x => x.RateMasters)
                .HasForeignKey(x => x.RateGroupId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CustomerCode)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.FullName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(x => x.MobileNumber)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(x => x.Address)
                .HasMaxLength(500);

            // Customer belongs to a Client
            entity.HasOne(x => x.Client)
                .WithMany()
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Customer uses a Rate Master
            entity.HasOne(x => x.RateGroup)
                .WithMany()
                .HasForeignKey(x => x.RateGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // Customer code must be unique within a Client
            entity.HasIndex(x => new
            {
                x.ClientId,
                x.CustomerCode
            })
            .IsUnique();

            // Mobile number should also be unique within a Client
            entity.HasIndex(x => new
            {
                x.ClientId,
                x.MobileNumber
            })
            .IsUnique();
        });

        modelBuilder.Entity<MilkDistribution>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Session)
                .IsRequired()
                .HasMaxLength(1);

            entity.Property(x => x.MilkType)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(x => x.Quantity)
                .HasPrecision(10, 2);

            entity.Property(x => x.Rate)
                .HasPrecision(10, 2);

            entity.Property(x => x.Amount)
                .HasPrecision(12, 2);

            entity.Property(x => x.DistributionDate)
                .IsRequired();

            // Client
            entity.HasOne(x => x.Client)
                .WithMany()
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Customer
            entity.HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Rate Master
            entity.HasOne(x => x.RateMaster)
                .WithMany()
                .HasForeignKey(x => x.RateMasterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Useful for daily distribution queries
            entity.HasIndex(x => new
            {
                x.ClientId,
                x.CustomerId,
                x.DistributionDate,
                x.Session
            })
            .IsUnique();

            entity.HasIndex(x => new
            {
                x.ClientId,
                x.DistributionDate,
                x.Session
            });
        });

        modelBuilder.Entity<ExpenseAccount>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Description)
                .HasMaxLength(500);

            entity.Property(x => x.IsActive)
                .IsRequired();

            entity.HasOne(x => x.Client)
                .WithMany()
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.ClientId,
                x.Name
            })
            .IsUnique();
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ExpenseDate)
                .IsRequired();

            entity.Property(x => x.Amount)
                .HasPrecision(12, 2)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(500);

            entity.Property(x => x.PaymentMode)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(x => x.ReferenceNumber)
                .HasMaxLength(100);

            entity.HasOne(x => x.Client)
                .WithMany()
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ExpenseAccount)
                .WithMany(x => x.Expenses)
                .HasForeignKey(x => x.ExpenseAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.ClientId,
                x.ExpenseDate
            });

            entity.HasIndex(x => new
            {
                x.ClientId,
                x.ExpenseAccountId,
                x.ExpenseDate
            });
        });

        modelBuilder.Entity<RateGroup>()
            .HasOne(x => x.Client)
            .WithMany()
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RateMaster>()
    .HasOne(x => x.RateGroup)
    .WithMany(x => x.RateMasters)
    .HasForeignKey(x => x.RateGroupId)
    .OnDelete(DeleteBehavior.Restrict);

    }
}