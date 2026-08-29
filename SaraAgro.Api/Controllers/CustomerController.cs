using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaraAgro.Api.DTOs.Customer;
using SaraAgro.Api.Extensions;
using SaraAgro.Api.Interfaces.Customer;

namespace SaraAgro.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(
        ICustomerService customerService)
    {
        _customerService = customerService;
    }


    // =========================================================
    // CREATE CUSTOMER
    // =========================================================

    [HttpPost]
    public async Task<IActionResult> CreateCustomer(
        [FromBody] CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var clientId = User.GetClientId();

        var customerId =
            await _customerService.CreateCustomerAsync(
                clientId,
                request,
                cancellationToken);

        return Ok(new
        {
            success = true,
            customerId
        });
    }


    // =========================================================
    // GET ALL CUSTOMERS
    // =========================================================

    [HttpGet]
    public async Task<ActionResult<List<CustomerResponse>>> GetCustomers(
        CancellationToken cancellationToken)
    {
        var clientId = User.GetClientId();

        var customers =
            await _customerService.GetCustomersAsync(
                clientId,
                cancellationToken);

        return Ok(customers);
    }


    // =========================================================
    // GET CUSTOMER BY ID
    // =========================================================

    [HttpGet("{customerId:int}")]
    public async Task<ActionResult<CustomerResponse>> GetCustomer(
        int customerId,
        CancellationToken cancellationToken)
    {
        var clientId = User.GetClientId();

        var customer =
            await _customerService.GetCustomerByIdAsync(
                clientId,
                customerId,
                cancellationToken);

        if (customer == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Customer not found."
            });
        }

        return Ok(customer);
    }


    // =========================================================
    // UPDATE CUSTOMER
    // =========================================================

    [HttpPut("{customerId:int}")]
    public async Task<IActionResult> UpdateCustomer(
        int customerId,
        [FromBody] UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var clientId = User.GetClientId();

        var updated =
            await _customerService.UpdateCustomerAsync(
                clientId,
                customerId,
                request,
                cancellationToken);

        if (!updated)
        {
            return NotFound(new
            {
                success = false,
                message = "Customer not found."
            });
        }

        return Ok(new
        {
            success = true,
            message = "Customer updated successfully."
        });
    }


    // =========================================================
    // UPDATE CUSTOMER STATUS
    // =========================================================

    [HttpPatch("{customerId:int}/status")]
    public async Task<IActionResult> UpdateCustomerStatus(
        int customerId,
        [FromBody] UpdateCustomerStatusRequest request,
        CancellationToken cancellationToken)
    {
        var clientId = User.GetClientId();

        var updated =
            await _customerService.UpdateCustomerStatusAsync(
                clientId,
                customerId,
                request,
                cancellationToken);

        if (!updated)
        {
            return NotFound(new
            {
                success = false,
                message = "Customer not found."
            });
        }

        return Ok(new
        {
            success = true,
            message = request.IsActive
                ? "Customer activated successfully."
                : "Customer deactivated successfully."
        });
    }


    // =========================================================
    // CUSTOMER ACCOUNT SUMMARY
    // =========================================================

    [HttpGet("{customerId:int}/account")]
    public async Task<ActionResult<CustomerAccountSummaryResponse>>
        GetCustomerAccount(
            int customerId,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            CancellationToken cancellationToken)
    {
        var clientId = User.GetClientId();

        var account =
            await _customerService.GetCustomerAccountSummaryAsync(
                clientId,
                customerId,
                fromDate,
                toDate,
                cancellationToken);

        if (account == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Customer not found."
            });
        }

        return Ok(account);
    }
}