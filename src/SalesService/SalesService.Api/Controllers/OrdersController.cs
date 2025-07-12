using Microsoft.AspNetCore.Mvc;
using SalesService.Application.DTOs;
using SalesService.Application.Interfaces;

namespace SalesService.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Get a list of orders with optional filters.
        /// </summary>
        /// <param name="status">Optional. Filter by order status.</param>
        /// <param name="customerId">Optional. Filter by customer ID.</param>
        /// <returns>A list of orders.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders([FromQuery] string? status = null, [FromQuery] Guid? customerId = null)
        {
            IEnumerable<OrderDto> orders;

            if (customerId.HasValue)
            {
                orders = await _orderService.GetByCustomerIdAsync(customerId.Value);
                if (!string.IsNullOrEmpty(status))
                {
                    orders = orders.Where(o => o.Status == status);
                }
            }
            else if (!string.IsNullOrEmpty(status))
            {
                orders = await _orderService.GetByStatusAsync(status);
            }
            else
            {
                orders = await _orderService.GetAllAsync();
            }
            return Ok(orders);
        }

        /// <summary>
        /// Get order details by ID.
        /// </summary>
        /// <param name="id">The ID of the order.</param>
        /// <returns>Order details.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrderById(Guid id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }

        /// <summary>
        /// Create a new order.
        /// </summary>
        /// <param name="createOrderDto">The order data.</param>
        /// <returns>The created order details.</returns>
        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderDto createOrderDto)
        {
            var orderResult = await _orderService.CreateAsync(createOrderDto);
            if (!orderResult.Success || orderResult.Order == null)
            {
                return BadRequest(orderResult.Message);
            }
            return CreatedAtAction(nameof(GetOrderById), new { id = orderResult.Order.Id }, orderResult.Order);
        }

        /// <summary>
        /// Update the status of an order.
        /// </summary>
        /// <param name="id">The ID of the order to update.</param>
        /// <param name="status">The new status.</param>
        /// <returns>No content if successful.</returns>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] string status)
        {
            var updateStatusDto = new UpdateOrderStatusDto { OrderId = id.ToString(), Status = status };
            var result = await _orderService.UpdateStatusAsync(updateStatusDto);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return NoContent();
        }

        /// <summary>
        /// Preview order calculations.
        /// </summary>
        /// <param name="calculateOrderDto">The order data for calculation.</param>
        /// <returns>The calculated order totals.</returns>
        [HttpPost("calculate-total")]
        public async Task<ActionResult<OrderCalculationResultDto>> CalculateOrderTotal(CalculateOrderTotalDto calculateOrderDto)
        {
            var result = await _orderService.CalculateTotalAsync(calculateOrderDto);
            return Ok(result);
        }
    }
}