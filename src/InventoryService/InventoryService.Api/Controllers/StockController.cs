using Microsoft.AspNetCore.Mvc;
using InventoryService.Application.Interfaces;
using InventoryService.Application.DTOs;

namespace InventoryService.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class StockController : ControllerBase
{
    private readonly IStockReservationService _stockReservationService;
    private readonly IMultiWarehouseStockService _multiWarehouseStockService;
    private readonly ILowStockMonitoringService _lowStockMonitoringService;
    private readonly ILogger<StockController> _logger;

    public StockController(
        IStockReservationService stockReservationService,
        IMultiWarehouseStockService multiWarehouseStockService,
        ILowStockMonitoringService lowStockMonitoringService,
        ILogger<StockController> logger)
    {
        _stockReservationService = stockReservationService;
        _multiWarehouseStockService = multiWarehouseStockService;
        _lowStockMonitoringService = lowStockMonitoringService;
        _logger = logger;
    }

    /// <summary>
    /// Reserve stock for a product
    /// </summary>
    [HttpPost("reserve")]
    public async Task<ActionResult<StockReservationResultDto>> ReserveStock([FromBody] CreateStockReservationDto request)
    {
        try
        {
            var result = await _stockReservationService.ReserveStockAsync(request);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reserving stock for product {ProductId}", request.ProductId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Release a stock reservation
    /// </summary>
    [HttpPost("release/{reservationId}")]
    public async Task<ActionResult<StockReservationResultDto>> ReleaseReservation(string reservationId)
    {
        try
        {
            var result = await _stockReservationService.ReleaseReservationAsync(reservationId);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing reservation {ReservationId}", reservationId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get stock reservation details
    /// </summary>
    [HttpGet("reservations/{reservationId}")]
    public async Task<ActionResult<StockReservationDto>> GetReservation(string reservationId)
    {
        try
        {
            var reservation = await _stockReservationService.GetReservationAsync(reservationId);
            
            if (reservation == null)
            {
                return NotFound(new { message = "Reservation not found" });
            }
            
            return Ok(reservation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reservation {ReservationId}", reservationId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Check stock availability across warehouses
    /// </summary>
    [HttpGet("availability/{productId}")]
    public async Task<ActionResult<MultiWarehouseStockAvailabilityDto>> CheckAvailability(Guid productId, [FromQuery] int quantity = 1)
    {
        try
        {
            var availability = await _multiWarehouseStockService.CheckStockAvailabilityAsync(productId, quantity);
            return Ok(availability);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking availability for product {ProductId}", productId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Transfer stock between warehouses
    /// </summary>
    [HttpPost("transfer")]
    public async Task<ActionResult<StockTransferResultDto>> TransferStock([FromBody] CreateStockTransferDto request)
    {
        try
        {
            var result = await _multiWarehouseStockService.TransferStockAsync(request);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transferring stock from {FromWarehouse} to {ToWarehouse}", 
                request.FromWarehouseId, request.ToWarehouseId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get low stock alerts
    /// </summary>
    [HttpGet("alerts/low-stock")]
    public async Task<ActionResult<IEnumerable<LowStockAlertDto>>> GetLowStockAlerts()
    {
        try
        {
            var alerts = await _lowStockMonitoringService.GetLowStockAlertsAsync();
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting low stock alerts");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get low stock alerts for a specific warehouse
    /// </summary>
    [HttpGet("alerts/low-stock/warehouse/{warehouseId}")]
    public async Task<ActionResult<IEnumerable<LowStockAlertDto>>> GetLowStockAlertsByWarehouse(Guid warehouseId)
    {
        try
        {
            var alerts = await _lowStockMonitoringService.GetLowStockAlertsByWarehouseAsync(warehouseId);
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting low stock alerts for warehouse {WarehouseId}", warehouseId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get reorder suggestions
    /// </summary>
    [HttpGet("alerts/reorder-suggestions")]
    public async Task<ActionResult<IEnumerable<ReorderSuggestionDto>>> GetReorderSuggestions()
    {
        try
        {
            var suggestions = await _lowStockMonitoringService.GetReorderSuggestionsAsync();
            return Ok(suggestions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reorder suggestions");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get stock level report
    /// </summary>
    [HttpGet("reports/stock-levels")]
    public async Task<ActionResult<IEnumerable<StockLevelReportDto>>> GetStockLevelReport()
    {
        try
        {
            var report = await _lowStockMonitoringService.GetStockLevelReportAsync();
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock level report");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Update reorder level for a product
    /// </summary>
    [HttpPut("reorder-level/{productId}")]
    public async Task<ActionResult> UpdateReorderLevel(Guid productId, [FromBody] UpdateReorderLevelDto request)
    {
        try
        {
            var success = await _lowStockMonitoringService.UpdateReorderLevelAsync(productId, request.ReorderLevel);
            
            if (success)
            {
                return Ok(new { message = "Reorder level updated successfully" });
            }
            
            return NotFound(new { message = "Product not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating reorder level for product {ProductId}", productId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get health status
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "Stock Controller is healthy", timestamp = DateTime.UtcNow });
    }
}

public class UpdateReorderLevelDto
{
    public int ReorderLevel { get; set; }
}
