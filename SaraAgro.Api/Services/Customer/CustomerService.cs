using Microsoft.EntityFrameworkCore;
using SaraAgro.Api.Data;
using SaraAgro.Api.DTOs.Customer;
using SaraAgro.Api.Interfaces.Customer;
using CustomerModel = SaraAgro.Api.Models.Customer;

namespace SaraAgro.Api.Services.Customer;

public class CustomerService : ICustomerService
{
    private readonly SaraAgroDbContext _dbContext;

    public CustomerService(SaraAgroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // =========================================================
    // UPDATE CUSTOMER STATUS
    // =========================================================

    public async Task<bool> UpdateCustomerStatusAsync(
        int clientId,
        int customerId,
        UpdateCustomerStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await _dbContext.Customers
            .FirstOrDefaultAsync(
                x => x.Id == customerId &&
                     x.ClientId == clientId,
                cancellationToken);

        if (customer == null)
        {
            return false;
        }

        customer.IsActive = request.IsActive;
        customer.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }


    // =========================================================
    // CUSTOMER ACCOUNT SUMMARY
    // =========================================================

    public async Task<CustomerAccountSummaryResponse?> GetCustomerAccountSummaryAsync(
        int clientId,
        int customerId,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        var from = fromDate.Date;
        var to = toDate.Date;

        if (to < from)
        {
            throw new ArgumentException(
                "To date cannot be earlier than from date.");
        }

        // -----------------------------------------------------
        // Get customer with Rate Group
        // -----------------------------------------------------

        var customer = await _dbContext.Customers
            .AsNoTracking()
            .Include(x => x.RateGroup)
            .FirstOrDefaultAsync(
                x => x.Id == customerId &&
                     x.ClientId == clientId,
                cancellationToken);

        if (customer == null)
        {
            return null;
        }

        // -----------------------------------------------------
        // Get actual milk distributions
        //
        // Milk type is NOT taken from Customer anymore.
        // It comes from each actual distribution.
        // -----------------------------------------------------

        var distributions = await _dbContext.MilkDistributions
            .AsNoTracking()
            .Where(x =>
                x.ClientId == clientId &&
                x.CustomerId == customerId &&
                x.DistributionDate >= from &&
                x.DistributionDate <= to)
            .ToListAsync(cancellationToken);

        // -----------------------------------------------------
        // Morning quantity
        // -----------------------------------------------------

        var morningQuantity = distributions
            .Where(x => x.Session == "M")
            .Sum(x => x.Quantity);

        // -----------------------------------------------------
        // Evening quantity
        // -----------------------------------------------------

        var eveningQuantity = distributions
            .Where(x => x.Session == "E")
            .Sum(x => x.Quantity);

        // -----------------------------------------------------
        // Total quantity
        // -----------------------------------------------------

        var totalQuantity =
            morningQuantity + eveningQuantity;

        // -----------------------------------------------------
        // Total amount
        // -----------------------------------------------------

        var totalAmount = distributions
            .Sum(x => x.Amount);

        return new CustomerAccountSummaryResponse
        {
            CustomerId = customer.Id,

            CustomerCode = customer.CustomerCode,

            CustomerName = customer.FullName,

            RateGroupId = customer.RateGroupId,

            RateGroupName =
                customer.RateGroup?.Name ?? string.Empty,

            FromDate = from,

            ToDate = to,

            MorningQuantity = morningQuantity,

            EveningQuantity = eveningQuantity,

            TotalQuantity = totalQuantity,

            TotalAmount = totalAmount
        };
    }


    // =========================================================
    // GET CUSTOMER BY ID
    // =========================================================

    public async Task<CustomerResponse?> GetCustomerByIdAsync(
        int clientId,
        int customerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Customers
            .AsNoTracking()
            .Where(x =>
                x.Id == customerId &&
                x.ClientId == clientId)
            .Select(x => new CustomerResponse
            {
                Id = x.Id,

                ClientId = x.ClientId,

                RateGroupId = x.RateGroupId,

                RateGroupName =
                    x.RateGroup != null
                        ? x.RateGroup.Name
                        : string.Empty,

                CustomerCode = x.CustomerCode,

                FullName = x.FullName,

                MobileNumber = x.MobileNumber,

                Address = x.Address,

                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);
    }


    // =========================================================
    // GET ALL CUSTOMERS
    // =========================================================

    public async Task<List<CustomerResponse>> GetCustomersAsync(
        int clientId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Customers
            .AsNoTracking()
            .Where(x => x.ClientId == clientId)
            .OrderBy(x => x.CustomerCode)
            .Select(x => new CustomerResponse
            {
                Id = x.Id,

                ClientId = x.ClientId,

                RateGroupId = x.RateGroupId,

                RateGroupName =
                    x.RateGroup != null
                        ? x.RateGroup.Name
                        : string.Empty,

                CustomerCode = x.CustomerCode,

                FullName = x.FullName,

                MobileNumber = x.MobileNumber,

                Address = x.Address,

                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }


    // =========================================================
    // UPDATE CUSTOMER
    // =========================================================

    public async Task<bool> UpdateCustomerAsync(
        int clientId,
        int customerId,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        // -----------------------------------------------------
        // 1. Find customer
        // -----------------------------------------------------

        var customer = await _dbContext.Customers
            .FirstOrDefaultAsync(
                x => x.Id == customerId &&
                     x.ClientId == clientId,
                cancellationToken);

        if (customer == null)
        {
            return false;
        }

        // -----------------------------------------------------
        // 2. Validate Rate Group
        // -----------------------------------------------------

        var rateGroupExists = await _dbContext.RateGroups
            .AnyAsync(
                x => x.Id == request.RateGroupId &&
                     x.ClientId == clientId &&
                     x.IsActive,
                cancellationToken);

        if (!rateGroupExists)
        {
            throw new InvalidOperationException(
                "Invalid rate group for this client.");
        }

        // -----------------------------------------------------
        // 3. Check duplicate mobile number
        // -----------------------------------------------------

        var mobileNumber = request.MobileNumber.Trim();

        var mobileExists = await _dbContext.Customers
            .AnyAsync(
                x => x.ClientId == clientId &&
                     x.MobileNumber == mobileNumber &&
                     x.Id != customerId,
                cancellationToken);

        if (mobileExists)
        {
            throw new InvalidOperationException(
                "Mobile number already exists for another customer.");
        }

        // -----------------------------------------------------
        // 4. Update customer
        // -----------------------------------------------------

        customer.RateGroupId = request.RateGroupId;

        customer.FullName =
            request.FullName.Trim();

        customer.MobileNumber =
            mobileNumber;

        customer.Address =
            request.Address?.Trim() ?? string.Empty;

        customer.IsActive =
            request.IsActive;

        customer.UpdatedAt =
            DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }


    // =========================================================
    // CREATE CUSTOMER
    // =========================================================

    public async Task<int> CreateCustomerAsync(
        int clientId,
        CreateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        var customerCode =
            request.CustomerCode.Trim();

        var fullName =
            request.FullName.Trim();

        var mobileNumber =
            request.MobileNumber.Trim();

        // -----------------------------------------------------
        // 1. Verify Client
        // -----------------------------------------------------

        var clientExists = await _dbContext.Clients
            .AnyAsync(
                x => x.Id == clientId &&
                     x.IsActive,
                cancellationToken);

        if (!clientExists)
        {
            throw new InvalidOperationException(
                "Invalid or inactive client.");
        }

        // -----------------------------------------------------
        // 2. Verify Rate Group
        // -----------------------------------------------------

        var rateGroupExists = await _dbContext.RateGroups
            .AnyAsync(
                x => x.Id == request.RateGroupId &&
                     x.ClientId == clientId &&
                     x.IsActive,
                cancellationToken);

        if (!rateGroupExists)
        {
            throw new InvalidOperationException(
                "Invalid rate group for this client.");
        }

        // -----------------------------------------------------
        // 3. Check duplicate Customer Code
        // -----------------------------------------------------

        var customerCodeExists = await _dbContext.Customers
            .AnyAsync(
                x => x.ClientId == clientId &&
                     x.CustomerCode == customerCode,
                cancellationToken);

        if (customerCodeExists)
        {
            throw new InvalidOperationException(
                "Customer code already exists for this client.");
        }

        // -----------------------------------------------------
        // 4. Check duplicate Mobile Number
        // -----------------------------------------------------

        var mobileExists = await _dbContext.Customers
            .AnyAsync(
                x => x.ClientId == clientId &&
                     x.MobileNumber == mobileNumber,
                cancellationToken);

        if (mobileExists)
        {
            throw new InvalidOperationException(
                "Mobile number already exists for this client.");
        }

        // -----------------------------------------------------
        // 5. Create Customer
        // -----------------------------------------------------

        var customer = new CustomerModel
        {
            ClientId = clientId,

            RateGroupId =
                request.RateGroupId,

            CustomerCode =
                customerCode,

            FullName =
                fullName,

            MobileNumber =
                mobileNumber,

            Address =
                request.Address?.Trim() ?? string.Empty,

            IsActive = true,

            CreatedAt =
                DateTime.UtcNow
        };

        _dbContext.Customers.Add(customer);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return customer.Id;
    }
}