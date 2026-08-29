using Microsoft.EntityFrameworkCore;
using SaraAgro.Api.Data;
using SaraAgro.Api.DTOs.MilkDistribution;
using SaraAgro.Api.Interfaces.MilkDistribution;
using MilkDistributionModel = SaraAgro.Api.Models.MilkDistribution;

namespace SaraAgro.Api.Services.MilkDistribution;

public class MilkDistributionService : IMilkDistributionService
{
    private readonly SaraAgroDbContext _dbContext;

    public MilkDistributionService(
        SaraAgroDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    // =========================================================
    // CREATE DISTRIBUTION
    // =========================================================

    public async Task<MilkDistributionResponse> CreateDistributionAsync(
        int clientId,
        CreateMilkDistributionRequest request,
        CancellationToken cancellationToken = default)
    {
        var session =
            request.Session.Trim().ToUpperInvariant();

        var milkType =
            request.MilkType.Trim();


        // -----------------------------------------------------
        // 1. Validate session
        // -----------------------------------------------------

        if (session != "M" && session != "E")
        {
            throw new ArgumentException(
                "Session must be M or E.");
        }


        // -----------------------------------------------------
        // 2. Normalize milk type
        // -----------------------------------------------------

        milkType =
            milkType.Equals(
                "cow",
                StringComparison.OrdinalIgnoreCase)
                ? "Cow"
                : milkType.Equals(
                    "buffalo",
                    StringComparison.OrdinalIgnoreCase)
                    ? "Buffalo"
                    : throw new ArgumentException(
                        "Milk type must be Cow or Buffalo.");


        // -----------------------------------------------------
        // 3. Validate quantity
        // -----------------------------------------------------

        if (request.Quantity <= 0)
        {
            throw new ArgumentException(
                "Quantity must be greater than zero.");
        }


        if (decimal.Round(
                request.Quantity,
                2) != request.Quantity)
        {
            throw new ArgumentException(
                "Quantity can have maximum 2 decimal places.");
        }


        var distributionDate =
            request.DistributionDate.Date;


        // -----------------------------------------------------
        // 4. Find customer + Rate Group
        // -----------------------------------------------------

        var customer =
            await _dbContext.Customers
                .Include(x => x.RateGroup)
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == request.CustomerId &&
                        x.ClientId == clientId &&
                        x.IsActive,
                    cancellationToken);


        if (customer == null)
        {
            throw new InvalidOperationException(
                "Customer not found or inactive.");
        }


        // -----------------------------------------------------
        // 5. Validate customer's Rate Group
        // -----------------------------------------------------

        var rateGroup =
            customer.RateGroup;


        if (rateGroup == null ||
            !rateGroup.IsActive ||
            rateGroup.ClientId != clientId)
        {
            throw new InvalidOperationException(
                "Customer does not have a valid rate group.");
        }


        // -----------------------------------------------------
        // 6. Prevent duplicate distribution
        // -----------------------------------------------------

        var duplicateExists =
            await _dbContext.MilkDistributions
                .AnyAsync(
                    x =>
                        x.ClientId == clientId &&
                        x.CustomerId == request.CustomerId &&
                        x.DistributionDate ==
                            distributionDate &&
                        x.Session == session,
                    cancellationToken);


        if (duplicateExists)
        {
            throw new InvalidOperationException(
                "Milk distribution already exists for this customer, date and session.");
        }


        // -----------------------------------------------------
        // 7. Find applicable rate
        //
        // Customer gives us:
        //     RateGroup
        //
        // Distribution gives us:
        //     MilkType
        //
        // Therefore:
        //     RateGroup + MilkType + EffectiveDate
        // -----------------------------------------------------

        var applicableRate =
            await _dbContext.RateMasters
                .AsNoTracking()
                .Where(
                    x =>
                        x.ClientId == clientId &&
                        x.RateGroupId ==
                            customer.RateGroupId &&
                        x.MilkType == milkType &&
                        x.IsActive &&
                        x.EffectiveDate <=
                            distributionDate)
                .OrderByDescending(
                    x => x.EffectiveDate)
                .FirstOrDefaultAsync(
                    cancellationToken);


        if (applicableRate == null)
        {
            throw new InvalidOperationException(
                $"No applicable {milkType} milk rate found for " +
                $"rate group '{rateGroup.Name}' " +
                $"and distribution date.");
        }


        // -----------------------------------------------------
        // 8. Calculate amount
        // -----------------------------------------------------

        var amount =
            decimal.Round(
                request.Quantity *
                applicableRate.Rate,
                2,
                MidpointRounding.AwayFromZero);


        // -----------------------------------------------------
        // 9. Create distribution
        // -----------------------------------------------------

        var distribution =
            new MilkDistributionModel
            {
                ClientId =
                    clientId,

                CustomerId =
                    customer.Id,

                RateMasterId =
                    applicableRate.Id,

                DistributionDate =
                    distributionDate,

                Session =
                    session,

                MilkType =
                    milkType,

                Quantity =
                    decimal.Round(
                        request.Quantity,
                        2),

                Rate =
                    applicableRate.Rate,

                Amount =
                    amount,

                CreatedAt =
                    DateTime.UtcNow
            };


        _dbContext.MilkDistributions.Add(
            distribution);


        await _dbContext.SaveChangesAsync(
            cancellationToken);


        // -----------------------------------------------------
        // 10. Return created distribution
        // -----------------------------------------------------

        return new MilkDistributionResponse
        {
            Id =
                distribution.Id,

            CustomerId =
                customer.Id,

            CustomerCode =
                customer.CustomerCode,

            CustomerName =
                customer.FullName,

            DistributionDate =
                distribution.DistributionDate,

            Session =
                distribution.Session,

            MilkType =
                distribution.MilkType,

            Quantity =
                distribution.Quantity,

            Rate =
                distribution.Rate,

            Amount =
                distribution.Amount,

            RateMasterId =
                applicableRate.Id,

            RateGroupId =
                rateGroup.Id,

            RateGroupName =
                rateGroup.Name
        };
    }


    // =========================================================
    // GET DISTRIBUTIONS
    // =========================================================

    public async Task<List<MilkDistributionResponse>>
        GetDistributionsAsync(
            int clientId,
            DateTime? date,
            string? session,
            string? search,
            CancellationToken cancellationToken = default)
    {
        var query =
            _dbContext.MilkDistributions
                .AsNoTracking()
                .Where(
                    x => x.ClientId == clientId)
                .AsQueryable();


        if (date.HasValue)
        {
            var distributionDate =
                date.Value.Date;

            query =
                query.Where(
                    x =>
                        x.DistributionDate ==
                        distributionDate);
        }


        if (!string.IsNullOrWhiteSpace(session))
        {
            var normalizedSession =
                session.Trim().ToUpperInvariant();


            if (normalizedSession != "M" &&
                normalizedSession != "E")
            {
                throw new ArgumentException(
                    "Session must be M or E.");
            }


            query =
                query.Where(
                    x =>
                        x.Session ==
                        normalizedSession);
        }


        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm =
                search.Trim();

            query =
                query.Where(
                    x =>
                        x.Customer != null &&
                        (
                            x.Customer.CustomerCode
                                .Contains(searchTerm) ||

                            x.Customer.FullName
                                .Contains(searchTerm) ||

                            x.Customer.MobileNumber
                                .Contains(searchTerm)
                        ));
        }


        return await query
            .OrderBy(
                x => x.DistributionDate)
            .ThenBy(
                x =>
                    x.Session == "M"
                        ? 0
                        : 1)
            .ThenBy(
                x =>
                    x.Customer!.CustomerCode)
            .Select(
                x => new MilkDistributionResponse
                {
                    Id =
                        x.Id,

                    CustomerId =
                        x.CustomerId,

                    CustomerCode =
                        x.Customer!.CustomerCode,

                    CustomerName =
                        x.Customer.FullName,

                    DistributionDate =
                        x.DistributionDate,

                    Session =
                        x.Session,

                    MilkType =
                        x.MilkType,

                    Quantity =
                        x.Quantity,

                    Rate =
                        x.Rate,

                    Amount =
                        x.Amount,

                    RateMasterId =
                        x.RateMasterId,

                    RateGroupId =
                        x.RateMaster!.RateGroupId,

                    RateGroupName =
                        x.RateMaster!.RateGroup!.Name
                })
            .ToListAsync(
                cancellationToken);
    }


    // =========================================================
    // GET CUSTOMER DISTRIBUTIONS
    // =========================================================

    public async Task<List<MilkDistributionResponse>>
        GetCustomerDistributionsAsync(
            int clientId,
            int customerId,
            DateTime? fromDate,
            DateTime? toDate,
            CancellationToken cancellationToken = default)
    {
        var customerExists =
            await _dbContext.Customers
                .AnyAsync(
                    x =>
                        x.Id == customerId &&
                        x.ClientId == clientId,
                    cancellationToken);


        if (!customerExists)
        {
            throw new InvalidOperationException(
                "Customer not found.");
        }


        var query =
            _dbContext.MilkDistributions
                .AsNoTracking()
                .Where(
                    x =>
                        x.ClientId == clientId &&
                        x.CustomerId == customerId)
                .AsQueryable();


        if (fromDate.HasValue)
        {
            var from =
                fromDate.Value.Date;

            query =
                query.Where(
                    x =>
                        x.DistributionDate >= from);
        }


        if (toDate.HasValue)
        {
            var to =
                toDate.Value.Date;

            query =
                query.Where(
                    x =>
                        x.DistributionDate <= to);
        }


        return await query
            .OrderByDescending(
                x => x.DistributionDate)
            .ThenBy(
                x =>
                    x.Session == "M"
                        ? 0
                        : 1)
            .Select(
                x => new MilkDistributionResponse
                {
                    Id =
                        x.Id,

                    CustomerId =
                        x.CustomerId,

                    CustomerCode =
                        x.Customer!.CustomerCode,

                    CustomerName =
                        x.Customer.FullName,

                    DistributionDate =
                        x.DistributionDate,

                    Session =
                        x.Session,

                    MilkType =
                        x.MilkType,

                    Quantity =
                        x.Quantity,

                    Rate =
                        x.Rate,

                    Amount =
                        x.Amount,

                    RateMasterId =
                        x.RateMasterId,

                    RateGroupId =
                        x.RateMaster!.RateGroupId,

                    RateGroupName =
                        x.RateMaster!.RateGroup!.Name
                })
            .ToListAsync(
                cancellationToken);
    }


    // =========================================================
    // DAILY SUMMARY
    // =========================================================

    public async Task<MilkDistributionSummaryResponse>
        GetDailySummaryAsync(
            int clientId,
            DateTime date,
            CancellationToken cancellationToken = default)
    {
        var distributionDate =
            date.Date;


        var distributions =
            await _dbContext.MilkDistributions
                .AsNoTracking()
                .Where(
                    x =>
                        x.ClientId == clientId &&
                        x.DistributionDate ==
                        distributionDate)
                .ToListAsync(
                    cancellationToken);


        var morningCowQuantity =
            distributions
                .Where(
                    x =>
                        x.Session == "M" &&
                        x.MilkType == "Cow")
                .Sum(
                    x => x.Quantity);


        var morningBuffaloQuantity =
            distributions
                .Where(
                    x =>
                        x.Session == "M" &&
                        x.MilkType == "Buffalo")
                .Sum(
                    x => x.Quantity);


        var eveningCowQuantity =
            distributions
                .Where(
                    x =>
                        x.Session == "E" &&
                        x.MilkType == "Cow")
                .Sum(
                    x => x.Quantity);


        var eveningBuffaloQuantity =
            distributions
                .Where(
                    x =>
                        x.Session == "E" &&
                        x.MilkType == "Buffalo")
                .Sum(
                    x => x.Quantity);


        var morningTotalQuantity =
            morningCowQuantity +
            morningBuffaloQuantity;


        var eveningTotalQuantity =
            eveningCowQuantity +
            eveningBuffaloQuantity;


        var totalQuantity =
            morningTotalQuantity +
            eveningTotalQuantity;


        var morningAmount =
            distributions
                .Where(
                    x => x.Session == "M")
                .Sum(
                    x => x.Amount);


        var eveningAmount =
            distributions
                .Where(
                    x => x.Session == "E")
                .Sum(
                    x => x.Amount);


        var totalAmount =
            morningAmount +
            eveningAmount;


        return new MilkDistributionSummaryResponse
        {
            Date =
                distributionDate,

            MorningCowQuantity =
                morningCowQuantity,

            MorningBuffaloQuantity =
                morningBuffaloQuantity,

            EveningCowQuantity =
                eveningCowQuantity,

            EveningBuffaloQuantity =
                eveningBuffaloQuantity,

            MorningTotalQuantity =
                morningTotalQuantity,

            EveningTotalQuantity =
                eveningTotalQuantity,

            TotalQuantity =
                totalQuantity,

            MorningAmount =
                morningAmount,

            EveningAmount =
                eveningAmount,

            TotalAmount =
                totalAmount
        };
    }


    // =========================================================
    // UPDATE DISTRIBUTION
    // =========================================================

    public async Task<MilkDistributionResponse?>
        UpdateDistributionAsync(
            int clientId,
            int distributionId,
            UpdateMilkDistributionRequest request,
            CancellationToken cancellationToken = default)
    {
        // -----------------------------------------------------
        // 1. Validate quantity
        // -----------------------------------------------------

        if (request.Quantity <= 0)
        {
            throw new ArgumentException(
                "Quantity must be greater than zero.");
        }


        if (decimal.Round(
                request.Quantity,
                2) != request.Quantity)
        {
            throw new ArgumentException(
                "Quantity can have maximum 2 decimal places.");
        }


        // -----------------------------------------------------
        // 2. Find distribution
        // -----------------------------------------------------

        var distribution =
            await _dbContext.MilkDistributions
                .Include(x => x.Customer)
                .Include(x => x.RateMaster)
                    .ThenInclude(x => x.RateGroup)
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == distributionId &&
                        x.ClientId == clientId,
                    cancellationToken);


        if (distribution == null)
        {
            return null;
        }


        // -----------------------------------------------------
        // 3. Preserve historical rate
        // -----------------------------------------------------

        var historicalRate =
            distribution.Rate;


        // -----------------------------------------------------
        // 4. Recalculate amount only
        // -----------------------------------------------------

        var amount =
            decimal.Round(
                request.Quantity *
                historicalRate,
                2,
                MidpointRounding.AwayFromZero);


        distribution.Quantity =
            decimal.Round(
                request.Quantity,
                2);


        distribution.Amount =
            amount;


        await _dbContext.SaveChangesAsync(
            cancellationToken);


        // -----------------------------------------------------
        // 5. Return updated distribution
        // -----------------------------------------------------

        return new MilkDistributionResponse
        {
            Id =
                distribution.Id,

            CustomerId =
                distribution.CustomerId,

            CustomerCode =
                distribution.Customer!.CustomerCode,

            CustomerName =
                distribution.Customer.FullName,

            DistributionDate =
                distribution.DistributionDate,

            Session =
                distribution.Session,

            MilkType =
                distribution.MilkType,

            Quantity =
                distribution.Quantity,

            Rate =
                distribution.Rate,

            Amount =
                distribution.Amount,

            RateMasterId =
                distribution.RateMasterId,

            RateGroupId =
                distribution.RateMaster!.RateGroupId,

            RateGroupName =
                distribution.RateMaster!.RateGroup!.Name
        };
    }

    // =========================================================
    // MONTHLY SUMMARY
    // =========================================================

    public async Task<MilkDistributionMonthlySummaryResponse>
        GetMonthlySummaryAsync(
            int clientId,
            DateTime month,
            CancellationToken cancellationToken = default)
    {
        var monthStart =
            new DateTime(
                month.Year,
                month.Month,
                1);

        var monthEnd =
            monthStart.AddMonths(1);


        // ---------------------------------------------------------
        // DISTRIBUTIONS FOR THIS MONTH
        // ---------------------------------------------------------

        var distributions =
            await _dbContext.MilkDistributions
                .AsNoTracking()
                .Where(
                    x =>
                        x.ClientId == clientId &&
                        x.DistributionDate >= monthStart &&
                        x.DistributionDate < monthEnd)
                .ToListAsync(
                    cancellationToken);


        // ---------------------------------------------------------
        // CUSTOMER COUNT
        // ---------------------------------------------------------

        var activeCustomerCount =
            await _dbContext.Customers
                .AsNoTracking()
                .CountAsync(
                    x =>
                        x.ClientId == clientId &&
                        x.IsActive,
                    cancellationToken);


        // ---------------------------------------------------------
        // MILK TOTALS
        // ---------------------------------------------------------

        var cowQuantity =
            distributions
                .Where(
                    x =>
                        string.Equals(
                            x.MilkType,
                            "Cow",
                            StringComparison.OrdinalIgnoreCase))
                .Sum(
                    x => x.Quantity);


        var buffaloQuantity =
            distributions
                .Where(
                    x =>
                        string.Equals(
                            x.MilkType,
                            "Buffalo",
                            StringComparison.OrdinalIgnoreCase))
                .Sum(
                    x => x.Quantity);


        var totalQuantity =
            cowQuantity +
            buffaloQuantity;


        var totalAmount =
            distributions
                .Sum(
                    x => x.Amount);


        // ---------------------------------------------------------
        // DAYS WITH DISTRIBUTION
        // ---------------------------------------------------------

        var daysRecorded =
            distributions
                .Select(
                    x => x.DistributionDate.Date)
                .Distinct()
                .Count();


        // ---------------------------------------------------------
        // RESPONSE
        // ---------------------------------------------------------

        return new MilkDistributionMonthlySummaryResponse
        {
            Month =
                monthStart,

            ActiveCustomerCount =
                activeCustomerCount,

            CowQuantity =
                cowQuantity,

            BuffaloQuantity =
                buffaloQuantity,

            TotalQuantity =
                totalQuantity,

            TotalAmount =
                totalAmount,

            DaysRecorded =
                daysRecorded
        };
    }
}