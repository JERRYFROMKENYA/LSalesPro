using Microsoft.AspNetCore.Mvc;
using SalesService.Application.DTOs;
using SalesService.Application.Interfaces;

namespace SalesService.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        /// <summary>
        /// Get a list of customers with optional pagination.
        /// </summary>
        /// <param name="pageNumber">The page number for pagination (default: 1).</param>
        /// <param name="pageSize">The number of items per page (default: 10).</param>
        /// <returns>A paginated list of customers.</returns>
        [HttpGet]
        public async Task<ActionResult<CustomerPagedResultDto>> GetCustomers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var searchDto = new CustomerSearchDto { PageNumber = pageNumber, PageSize = pageSize };
            var customers = await _customerService.SearchPagedAsync(searchDto);
            return Ok(customers);
        }

        /// <summary>
        /// Get customer details by ID.
        /// </summary>
        /// <param name="id">The ID of the customer.</param>
        /// <returns>Customer details.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> GetCustomerById(Guid id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return Ok(customer);
        }

        /// <summary>
        /// Create a new customer.
        /// </summary>
        /// <param name="createCustomerDto">The customer data.</param>
        /// <returns>The created customer details.</returns>
        [HttpPost]
        public async Task<ActionResult<CustomerDto>> CreateCustomer(CreateCustomerDto createCustomerDto)
        {
            var customer = await _customerService.CreateAsync(createCustomerDto);
            return CreatedAtAction(nameof(GetCustomerById), new { id = customer.Id }, customer);
        }

        /// <summary>
        /// Update an existing customer.
        /// </summary>
        /// <param name="id">The ID of the customer to update.</param>
        /// <param name="updateCustomerDto">The updated customer data.</param>
        /// <returns>No content if successful.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(Guid id, UpdateCustomerDto updateCustomerDto)
        {
            await _customerService.UpdateAsync(id, updateCustomerDto);
            return NoContent();
        }

        /// <summary>
        /// Soft delete a customer by ID.
        /// </summary>
        /// <param name="id">The ID of the customer to soft delete.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(Guid id)
        {
            await _customerService.DeleteAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Get customer order history.
        /// </summary>
        /// <param name="id">The ID of the customer.</param>
        /// <returns>A list of customer orders.</returns>
        [HttpGet("{id}/orders")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetCustomerOrders(Guid id)
        {
            var orders = await _customerService.GetOrderHistoryAsync(id);
            return Ok(orders);
        }

        /// <summary>
        /// Get customer credit status.
        /// </summary>
        /// <param name="id">The ID of the customer.</param>
        /// <returns>The customer's credit status.</returns>
        [HttpGet("{id}/credit-status")]
        public async Task<ActionResult<CustomerCreditStatusDto>> GetCustomerCreditStatus(Guid id)
        {
            var creditStatus = await _customerService.GetCreditStatusAsync(id);
            if (creditStatus == null)
            {
                return NotFound();
            }
            return Ok(creditStatus);
        }

        /// <summary>
        /// Get location data for mapping customers.
        /// </summary>
        /// <returns>A list of customer location data.</returns>
        [HttpGet("map-data")]
        public async Task<ActionResult<IEnumerable<CustomerMapDataDto>>> GetCustomerMapData()
        {
            var mapData = await _customerService.GetMapDataAsync();
            return Ok(mapData);
        }
    }
}