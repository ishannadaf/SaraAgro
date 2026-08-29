using Microsoft.EntityFrameworkCore;
using SaraAgro.Api.Data;
using SaraAgro.Api.DTOs.RateGroup;
using SaraAgro.Api.Interfaces.RateGroup;
using RateGroupModel = SaraAgro.Api.Models.RateGroup;

namespace SaraAgro.Api.Services.RateGroup;

public class RateGroupService : IRateGroupService
{
    private readonly SaraAgroDbContext _dbContext;

    public RateGroupService(SaraAgroDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    // =========================================================
    // UPDATE RATE GROUP
    // =========================================================

    public async Task<bool> UpdateRateGroupAsync(
        int clientId,
        int rateGroupId,
        UpdateRateGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        var name =
            request.Name?.Trim()
            ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Rate group name is required.");
        }

        var rateGroup =
            await _dbContext.RateGroups
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == rateGroupId &&
                        x.ClientId == clientId,
                    cancellationToken);

        if (rateGroup == null)
        {
            return false;
        }

        var duplicateExists =
            await _dbContext.RateGroups
                .AnyAsync(
                    x =>
                        x.ClientId == clientId &&
                        x.Id != rateGroupId &&
                        x.Name.ToLower() == name.ToLower(),
                    cancellationToken);

        if (duplicateExists)
        {
            throw new InvalidOperationException(
                "A rate group with this name already exists.");
        }

        rateGroup.Name = name;

        rateGroup.UpdatedAt =
            DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
    }
    // =========================================================
    // GET ALL RATE GROUPS
    // =========================================================

    public async Task<List<RateGroupResponse>> GetRateGroupsAsync(
        int clientId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RateGroups
            .AsNoTracking()
            .Where(x =>
                x.ClientId == clientId &&
                x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new RateGroupResponse
            {
                Id = x.Id,

                ClientId = x.ClientId,

                Name = x.Name,

                IsActive = x.IsActive,

                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }


    // =========================================================
    // CREATE RATE GROUP
    // =========================================================

    public async Task<int> CreateRateGroupAsync(
        int clientId,
        CreateRateGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Rate group name is required.");
        }

        // -----------------------------------------------------
        // Verify client
        // -----------------------------------------------------

        var clientExists =
            await _dbContext.Clients
                .AnyAsync(
                    x =>
                        x.Id == clientId &&
                        x.IsActive,
                    cancellationToken);

        if (!clientExists)
        {
            throw new InvalidOperationException(
                "Invalid or inactive client.");
        }

        // -----------------------------------------------------
        // Prevent duplicate rate group name
        // -----------------------------------------------------

        var duplicateExists =
            await _dbContext.RateGroups
                .AnyAsync(
                    x =>
                        x.ClientId == clientId &&
                        x.Name.ToLower() == name.ToLower(),
                    cancellationToken);

        if (duplicateExists)
        {
            throw new InvalidOperationException(
                "A rate group with this name already exists.");
        }

        // -----------------------------------------------------
        // Create rate group
        // -----------------------------------------------------

        var rateGroup = new RateGroupModel
        {
            ClientId = clientId,

            Name = name,

            IsActive = true,

            CreatedAt = DateTime.UtcNow
        };

        _dbContext.RateGroups.Add(rateGroup);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return rateGroup.Id;
    }


    // =========================================================
    // UPDATE RATE GROUP STATUS
    // =========================================================

    public async Task<bool> UpdateRateGroupStatusAsync(
        int clientId,
        int rateGroupId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var rateGroup =
            await _dbContext.RateGroups
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == rateGroupId &&
                        x.ClientId == clientId,
                    cancellationToken);

        if (rateGroup == null)
        {
            return false;
        }

        rateGroup.IsActive = isActive;

        rateGroup.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}