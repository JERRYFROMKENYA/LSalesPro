using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesService.Application.DTOs;
using SalesService.Application.Interfaces;

namespace SalesService.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    /// <summary>
    /// Get all customers
    /// </summary>
    /// <returns>List of customers</returns>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Sales")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CustomerDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll()
    {
        try
        {
            var customers = await _customerService.GetAllAsync();
            return Ok(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all customers");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific customer by ID
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Customer details</returns>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager,Sales")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CustomerDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CustomerDto>> GetById(Guid id)
    {
        try
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return Ok(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer with ID: {CustomerId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    /// <param name="dto">Customer creation data</param>
    /// <returns>Created customer details</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CustomerDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CustomerDto>> Create(CreateCustomerDto dto)
    {
        try
        {
            var createdCustomer = await _customerService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdCustomer.Id }, createdCustomer);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation error during customer creation");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update an existing customer
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="dto">Updated customer data</param>
    /// <returns>Updated customer details</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CustomerDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CustomerDto>> Update(Guid id, UpdateCustomerDto dto)
    {
        try
        {
            var updatedCustomer = await _customerService.UpdateAsync(id, dto);
            if (updatedCustomer == null)
            {
                return NotFound();
            }
            return Ok(updatedCustomer);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation error during customer update for ID: {CustomerId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer with ID: {CustomerId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a customer
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Success or error message</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _customerService.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer with ID: {CustomerId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Search for customers
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <returns>List of matching customers</returns>
    [HttpGet("search")]
    [Authorize(Roles = "Admin,Manager,Sales")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CustomerDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> Search([FromQuery] string searchTerm)
    {
        try
        {
            var customers = await _customerService.SearchAsync(searchTerm);
            return Ok(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching customers with term: {SearchTerm}", searchTerm);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get customers with advanced filtering and pagination
    /// </summary>
    /// <param name="dto">Search parameters</param>
    /// <returns>Paged list of customers</returns>
    [HttpPost("search")]
    [Authorize(Roles = "Admin,Manager,Sales")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CustomerPagedResultDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CustomerPagedResultDto>> SearchPaged(CustomerSearchDto dto)
    {
        try
        {
            var result = await _customerService.SearchPagedAsync(dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching customers with paging");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get customers by type
    /// </summary>
    /// <param name="type">Customer type</param>
    /// <returns>List of customers of specified type</returns>
    [HttpGet("by-type/{type}")]
    [Authorize(Roles = "Admin,Manager,Sales")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CustomerDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetByType(string type)
    {
        try
        {
            var customers = await _customerService.GetByTypeAsync(type);
            return Ok(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customers by type: {Type}", type);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get customers by category
    /// </summary>
    /// <param name="category">Customer category</param>
    /// <returns>List of customers of specified category</returns>
    [HttpGet("by-category/{category}")]
    [Authorize(Roles = "Admin,Manager,Sales")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CustomerDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetByCategory(string category)
    {
        try
        {
            var customers = await _customerService.GetByCategoryAsync(category);
            return Ok(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customers by category: {Category}", category);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get active customers
    /// </summary>
    /// <returns>List of active customers</returns>
    [HttpGet("active")]
    [Authorize(Roles = "Admin,Manager,Sales")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CustomerDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetActive()
    {
        try
        {
            var customers = await _customerService.GetActiveCustomersAsync();
            return Ok(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active customers");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update a customer's credit limit
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="creditLimit">New credit limit</param>
    /// <returns>Success status</returns>
    [HttpPut("{id}/credit-limit")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateCreditLimit(Guid id, [FromBody] decimal creditLimit)
    {
        try
        {
            var result = await _customerService.UpdateCreditLimitAsync(id, creditLimit);
            if (!result)
            {
                return NotFound();
            }
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating credit limit for customer with ID: {CustomerId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Activate a customer
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Success status</returns>
    [HttpPut("{id}/activate")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Activate(Guid id)
    {
        try
        {
            var result = await _customerService.ActivateCustomerAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating customer with ID: {CustomerId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Deactivate a customer
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Success status</returns>
    [HttpPut("{id}/deactivate")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        try
        {
            var result = await _customerService.DeactivateCustomerAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating customer with ID: {CustomerId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all customer types
    /// </summary>
    /// <returns>List of customer types</returns>
    [HttpGet("types")]
    [Authorize(Roles = "Admin,Manager,Sales")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<string>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<string>>> GetCustomerTypes()
    {
        try
        {
            var types = await _customerService.GetCustomerTypesAsync();
            return Ok(types);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer types");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all customer categories
    /// </summary>
    /// <returns>List of customer categories</returns>
    [HttpGet("categories")]
    [Authorize(Roles = "Admin,Manager,Sales")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<string>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<string>>> GetCustomerCategories()
    {
        try
        {
            var categories = await _customerService.GetCustomerCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer categories");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Check if an email is already in use
    /// </summary>
    /// <param name="email">Email to check</param>
    /// <param name="excludeId">Optional customer ID to exclude from check</param>
    /// <returns>True if email is available</returns>
    [HttpGet("validate-email")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> ValidateEmail([FromQuery] string email, [FromQuery] Guid? excludeId = null)
    {
        try
        {
            var isValid = await _customerService.ValidateEmailAsync(email, excludeId);
            return Ok(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating email: {Email}", email);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Check if a tax ID is already in use
    /// </summary>
    /// <param name="taxId">Tax ID to check</param>
    /// <param name="excludeId">Optional customer ID to exclude from check</param>
    /// <returns>True if tax ID is available</returns>
    [HttpGet("validate-taxid")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> ValidateTaxId([FromQuery] string taxId, [FromQuery] Guid? excludeId = null)
    {
        try
        {
            var isValid = await _customerService.ValidateTaxIdAsync(taxId, excludeId);
            return Ok(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating tax ID: {TaxId}", taxId);
            return StatusCode(500, "Internal server error");
        }
    }
}
