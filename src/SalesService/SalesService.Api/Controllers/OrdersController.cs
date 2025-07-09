using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesService.Application.DTOs;
using SalesService.Application.Interfaces;

namespace SalesService.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Get all orders
    /// </summary>
    /// <returns>List of orders</returns>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Sales")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<OrderDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll()
    {
        try
        {
            var orders = await _orderService.GetAllAsync();
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all orders");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific order by ID
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order details</returns>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager,Sales")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderDto>> GetById(Guid id)
    {
        try
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order with ID: {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific order by order number
    /// </summary>
    /// <param name="orderNumber">Order number</param>
    /// <returns>Order details</returns>
    [HttpGet("by-number/{orderNumber}")]
    [Authorize(Roles = "Admin,Manager,Sales")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderDto>> GetByOrderNumber(string orderNumber)
    {
        try
        {
            var order = await _orderService.GetByOrderNumberAsync(orderNumber);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order with order number: {OrderNumber}", orderNumber);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    /// <param name="dto">Order creation data</param>
    /// <returns>Created order details</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Sales")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OrderResultDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderResultDto>> Create(CreateOrderDto dto)
    {
        try
        {
            var result = await _orderService.CreateAsync(dto);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }
            
            return CreatedAtAction(nameof(GetById), new { id = result.Order!.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update an existing order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="dto">Updated order data</param>
    /// <returns>Updated order details</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager,Sales")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderResultDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderResultDto>> Update(Guid id, UpdateOrderDto dto)
    {
        try
        {
            var result = await _orderService.UpdateAsync(id, dto);
            
            if (!result.Success)
            {
                if (result.ErrorCode == "ORDER_NOT_FOUND")
                {
                    return NotFound(result);
                }
                
                return BadRequest(result);
            }
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order with ID: {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete an order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Success or error message</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _orderService.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting order with ID: {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get orders for a specific customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>List of orders for the customer</returns>
    [HttpGet("by-customer/{customerId}")]
    [Authorize(Roles = "Admin,Manager,Sales")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<OrderDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetByCustomerId(Guid customerId)
    {
        try
        {
            var orders = await _orderService.GetByCustomerIdAsync(customerId);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders for customer with ID: {CustomerId}", customerId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get orders with a specific status
    /// </summary>
    /// <param name="status">Order status</param>
    /// <returns>List of orders with the specified status</returns>
    [HttpGet("by-status/{status}")]
    [Authorize(Roles = "Admin,Manager,Sales")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<OrderDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetByStatus(string status)
    {
        try
        {
            var orders = await _orderService.GetByStatusAsync(status);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders with status: {Status}", status);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get recent orders
    /// </summary>
    /// <param name="count">Number of orders to retrieve</param>
    /// <returns>List of recent orders</returns>
    [HttpGet("recent")]
    [Authorize(Roles = "Admin,Manager,Sales")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<OrderDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetRecent([FromQuery] int count = 10)
    {
        try
        {
            var orders = await _orderService.GetRecentOrdersAsync(count);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent orders");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Search for orders with advanced filtering and pagination
    /// </summary>
    /// <param name="dto">Search parameters</param>
    /// <returns>Paged list of orders</returns>
    [HttpPost("search")]
    [Authorize(Roles = "Admin,Manager,Sales")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderPagedResultDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderPagedResultDto>> Search(OrderSearchDto dto)
    {
        try
        {
            var result = await _orderService.SearchAsync(dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching orders");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Confirm an order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Updated order details</returns>
    [HttpPost("{id}/confirm")]
    [Authorize(Roles = "Admin,Manager,Sales")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderResultDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderResultDto>> ConfirmOrder(Guid id)
    {
        try
        {
            var result = await _orderService.ConfirmOrderAsync(id);
            
            if (!result.Success)
            {
                if (result.ErrorCode == "ORDER_NOT_FOUND")
                {
                    return NotFound(result);
                }
                
                return BadRequest(result);
            }
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming order with ID: {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Process an order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Updated order details</returns>
    [HttpPost("{id}/process")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderResultDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderResultDto>> ProcessOrder(Guid id)
    {
        try
        {
            var result = await _orderService.ProcessOrderAsync(id);
            
            if (!result.Success)
            {
                if (result.ErrorCode == "ORDER_NOT_FOUND")
                {
                    return NotFound(result);
                }
                
                return BadRequest(result);
            }
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order with ID: {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Mark an order as shipped
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="trackingInfo">Optional tracking information</param>
    /// <returns>Updated order details</returns>
    [HttpPost("{id}/ship")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderResultDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderResultDto>> ShipOrder(Guid id, [FromBody] string? trackingInfo = null)
    {
        try
        {
            var result = await _orderService.ShipOrderAsync(id, trackingInfo);
            
            if (!result.Success)
            {
                if (result.ErrorCode == "ORDER_NOT_FOUND")
                {
                    return NotFound(result);
                }
                
                return BadRequest(result);
            }
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error shipping order with ID: {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Mark an order as delivered
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Updated order details</returns>
    [HttpPost("{id}/deliver")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderResultDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderResultDto>> DeliverOrder(Guid id)
    {
        try
        {
            var result = await _orderService.DeliverOrderAsync(id);
            
            if (!result.Success)
            {
                if (result.ErrorCode == "ORDER_NOT_FOUND")
                {
                    return NotFound(result);
                }
                
                return BadRequest(result);
            }
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking order as delivered with ID: {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Cancel an order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="reason">Cancellation reason</param>
    /// <returns>Updated order details</returns>
    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderResultDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderResultDto>> CancelOrder(Guid id, [FromBody] string reason)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return BadRequest("Cancellation reason is required");
            }
            
            var result = await _orderService.CancelOrderAsync(id, reason);
            
            if (!result.Success)
            {
                if (result.ErrorCode == "ORDER_NOT_FOUND")
                {
                    return NotFound(result);
                }
                
                return BadRequest(result);
            }
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order with ID: {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Add an item to an order
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="dto">Order item data</param>
    /// <returns>Created order item details</returns>
    [HttpPost("{orderId}/items")]
    [Authorize(Roles = "Admin,Manager,Sales")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OrderItemDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderItemDto>> AddOrderItem(Guid orderId, AddOrderItemDto dto)
    {
        try
        {
            var orderItem = await _orderService.AddItemToOrderAsync(orderId, dto);
            return CreatedAtAction(nameof(GetById), new { id = orderId }, orderItem);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation error while adding item to order {OrderId}", orderId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to order with ID: {OrderId}", orderId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update an order item
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="itemId">Order item ID</param>
    /// <param name="dto">Updated order item data</param>
    /// <returns>Updated order item details</returns>
    [HttpPut("{orderId}/items/{itemId}")]
    [Authorize(Roles = "Admin,Manager,Sales")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderItemDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderItemDto>> UpdateOrderItem(Guid orderId, Guid itemId, UpdateOrderItemDto dto)
    {
        try
        {
            var orderItem = await _orderService.UpdateOrderItemAsync(itemId, dto);
            return Ok(orderItem);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation error while updating item {ItemId} in order {OrderId}", itemId, orderId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating item {ItemId} in order {OrderId}", itemId, orderId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Remove an item from an order
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="itemId">Order item ID</param>
    /// <returns>Success or error message</returns>
    [HttpDelete("{orderId}/items/{itemId}")]
    [Authorize(Roles = "Admin,Manager,Sales")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveOrderItem(Guid orderId, Guid itemId)
    {
        try
        {
            var result = await _orderService.RemoveItemFromOrderAsync(itemId);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing item {ItemId} from order {OrderId}", itemId, orderId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Validate an order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Validation result</returns>
    [HttpGet("{id}/validate")]
    [Authorize(Roles = "Admin,Manager,Sales")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> ValidateOrder(Guid id)
    {
        try
        {
            var isValid = await _orderService.ValidateOrderAsync(id);
            if (!isValid)
            {
                return Ok(false);
            }
            return Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating order with ID: {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Check inventory availability for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="quantity">Quantity needed</param>
    /// <param name="warehouseId">Optional warehouse ID</param>
    /// <returns>Availability status</returns>
    [HttpGet("check-inventory")]
    [Authorize(Roles = "Admin,Manager,Sales")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> CheckInventoryAvailability(
        [FromQuery] Guid productId, 
        [FromQuery] int quantity, 
        [FromQuery] Guid? warehouseId = null)
    {
        try
        {
            var isAvailable = await _orderService.CheckInventoryAvailabilityAsync(productId, quantity, warehouseId);
            return Ok(isAvailable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking inventory availability for ProductId: {ProductId}", productId);
            return StatusCode(500, "Internal server error");
        }
    }
}
