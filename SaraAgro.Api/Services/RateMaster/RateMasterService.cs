using Microsoft.EntityFrameworkCore;
using SaraAgro.Api.Data;
using SaraAgro.Api.DTOs.RateMaster;
using SaraAgro.Api.Interfaces.RateMaster;
using RateMasterModel = SaraAgro.Api.Models.RateMaster;

namespace SaraAgro.Api.Services.RateMaster;

public class RateMasterService : IRateMasterService
{
    private readonly SaraAgroDbContext _dbContext;

    public RateMasterService(SaraAgroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // =========================================================
    // UPDATE RATE
    // =========================================================

    public async Task<bool> UpdateRateAsync(
        int clientId,
        int rateMasterId,
        UpdateRateMasterRequest request,
        CancellationToken cancellationToken = default)
    {
        var milkType =
            request.MilkType.Trim();

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

        var effectiveDate =
            request.EffectiveDate.Date;


        // -----------------------------------------------------
        // FIND EXISTING RATE
        // -----------------------------------------------------

        var rateMaster =
            await _dbContext.RateMasters
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == rateMasterId &&
                        x.ClientId == clientId,
                    cancellationToken);

        if (rateMaster == null)
        {
            return false;
        }


        // -----------------------------------------------------
        // VALIDATE RATE GROUP
        // -----------------------------------------------------

        var rateGroup =
            await _dbContext.RateGroups
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == request.RateGroupId &&
                        x.ClientId == clientId &&
                        x.IsActive,
                    cancellationToken);

        if (rateGroup == null)
        {
            throw new InvalidOperationException(
                "Invalid or inactive rate group.");
        }


        // -----------------------------------------------------
        // DUPLICATE VALIDATION
        // -----------------------------------------------------

        var duplicateExists =
            await _dbContext.RateMasters
                .AnyAsync(
                    x =>
                        x.Id != rateMasterId &&

                        x.ClientId == clientId &&

                        x.RateGroupId ==
                            request.RateGroupId &&

                        x.MilkType ==
                            milkType &&

                        x.EffectiveDate ==
                            effectiveDate,
                    cancellationToken);

        if (duplicateExists)
        {
            throw new InvalidOperationException(
                "A rate already exists for this rate group, milk type and effective date.");
        }


        // -----------------------------------------------------
        // UPDATE
        // -----------------------------------------------------

        rateMaster.RateGroupId =
            request.RateGroupId;

        rateMaster.MilkType =
            milkType;

        rateMaster.Rate =
            request.Rate;

        rateMaster.EffectiveDate =
            effectiveDate;


        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
    }


    // =========================================================
    // DELETE RATE
    // =========================================================

    public async Task<bool> DeleteRateAsync(
        int clientId,
        int rateMasterId,
        CancellationToken cancellationToken = default)
    {
        var rateMaster =
            await _dbContext.RateMasters
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == rateMasterId &&
                        x.ClientId == clientId,
                    cancellationToken);

        if (rateMaster == null)
        {
            return false;
        }


        _dbContext.RateMasters.Remove(
            rateMaster);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    // =========================================================
    // GET ALL RATES
    // =========================================================

    public async Task<List<RateLookupResponse>> GetRatesAsync(
        int clientId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RateMasters
            .AsNoTracking()
            .Include(x => x.RateGroup)
            .Where(x =>
                x.ClientId == clientId)
            .OrderBy(x => x.RateGroup!.Name)
            .ThenBy(x => x.MilkType)
            .ThenByDescending(x => x.EffectiveDate)
            .Select(x => new RateLookupResponse
            {
                RateMasterId =
                    x.Id,

                RateGroupId =
                    x.RateGroupId,

                RateGroupName =
                    x.RateGroup!.Name,

                MilkType =
                    x.MilkType,

                Rate =
                    x.Rate,

                EffectiveDate =
                    x.EffectiveDate
            })
            .ToListAsync(
                cancellationToken);
    }


    // =========================================================
    // GET APPLICABLE RATE
    // =========================================================

    public async Task<RateLookupResponse?> GetApplicableRateAsync(
        int clientId,
        int rateGroupId,
        string milkType,
        DateTime distributionDate,
        CancellationToken cancellationToken = default)
    {
        var normalizedMilkType =
            milkType.Trim();


        normalizedMilkType =
            normalizedMilkType.Equals(
                "cow",
                StringComparison.OrdinalIgnoreCase)
                ? "Cow"
                : normalizedMilkType.Equals(
                    "buffalo",
                    StringComparison.OrdinalIgnoreCase)
                    ? "Buffalo"
                    : throw new ArgumentException(
                        "Milk type must be Cow or Buffalo.");


        var date =
            distributionDate.Date;


        return await _dbContext.RateMasters
            .AsNoTracking()
            .Include(x => x.RateGroup)
            .Where(
                x =>
                    x.ClientId == clientId &&

                    x.RateGroupId ==
                        rateGroupId &&

                    x.MilkType ==
                        normalizedMilkType &&

                    x.IsActive &&

                    x.EffectiveDate <=
                        date)
            .OrderByDescending(
                x => x.EffectiveDate)
            .Select(
                x => new RateLookupResponse
                {
                    RateMasterId =
                        x.Id,

                    RateGroupId =
                        x.RateGroupId,

                    RateGroupName =
                        x.RateGroup!.Name,

                    MilkType =
                        x.MilkType,

                    Rate =
                        x.Rate,

                    EffectiveDate =
                        x.EffectiveDate
                })
            .FirstOrDefaultAsync(
                cancellationToken);
    }


    // =========================================================
    // CREATE RATE
    // =========================================================

    public async Task<int> CreateRateAsync(
        CreateRateMasterRequest request,
        CancellationToken cancellationToken = default)
    {
        var milkType =
            request.MilkType.Trim();


        // -----------------------------------------------------
        // Normalize milk type
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
        // Normalize effective date
        // -----------------------------------------------------

        var effectiveDate =
            request.EffectiveDate.Date;


        // -----------------------------------------------------
        // Verify client
        // -----------------------------------------------------

        var clientExists =
            await _dbContext.Clients
                .AnyAsync(
                    x =>
                        x.Id ==
                            request.ClientId &&

                        x.IsActive,
                    cancellationToken);


        if (!clientExists)
        {
            throw new InvalidOperationException(
                "Invalid or inactive client.");
        }


        // -----------------------------------------------------
        // Verify Rate Group
        // -----------------------------------------------------

        var rateGroup =
            await _dbContext.RateGroups
                .FirstOrDefaultAsync(
                    x =>
                        x.Id ==
                            request.RateGroupId &&

                        x.ClientId ==
                            request.ClientId &&

                        x.IsActive,
                    cancellationToken);


        if (rateGroup == null)
        {
            throw new InvalidOperationException(
                "Invalid or inactive rate group.");
        }


        // -----------------------------------------------------
        // Prevent duplicate rate
        //
        // Same:
        // Client + RateGroup + MilkType + EffectiveDate
        // -----------------------------------------------------

        var duplicateExists =
            await _dbContext.RateMasters
                .AnyAsync(
                    x =>
                        x.ClientId ==
                            request.ClientId &&

                        x.RateGroupId ==
                            request.RateGroupId &&

                        x.MilkType ==
                            milkType &&

                        x.EffectiveDate ==
                            effectiveDate,
                    cancellationToken);


        if (duplicateExists)
        {
            throw new InvalidOperationException(
                "A rate already exists for this rate group, milk type and effective date.");
        }


        // -----------------------------------------------------
        // Create Rate Master
        // -----------------------------------------------------

        var rateMaster =
            new RateMasterModel
            {
                ClientId =
                    request.ClientId,

                RateGroupId =
                    request.RateGroupId,

                MilkType =
                    milkType,

                Rate =
                    request.Rate,

                EffectiveDate =
                    effectiveDate,

                IsActive =
                    true,

                CreatedAt =
                    DateTime.UtcNow
            };


        _dbContext.RateMasters.Add(
            rateMaster);


        await _dbContext.SaveChangesAsync(
            cancellationToken);


        return rateMaster.Id;
    }
}