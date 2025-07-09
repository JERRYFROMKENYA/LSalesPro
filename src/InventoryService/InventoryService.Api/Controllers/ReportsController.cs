using Microsoft.AspNetCore.Mvc;
using InventoryService.Application.Interfaces;
using InventoryService.Application.DTOs;

namespace InventoryService.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IWarehouseService _warehouseService;
    private readonly ILowStockMonitoringService _lowStockMonitoringService;
    private readonly IMultiWarehouseStockService _multiWarehouseStockService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        IProductService productService,
        IWarehouseService warehouseService,
        ILowStockMonitoringService lowStockMonitoringService,
        IMultiWarehouseStockService multiWarehouseStockService,
        ILogger<ReportsController> logger)
    {
        _productService = productService;
        _warehouseService = warehouseService;
        _lowStockMonitoringService = lowStockMonitoringService;
        _multiWarehouseStockService = multiWarehouseStockService;
        _logger = logger;
    }

    /// <summary>
    /// Get inventory summary report
    /// </summary>
    [HttpGet("inventory-summary")]
    public async Task<ActionResult<InventorySummaryReportDto>> GetInventorySummary()
    {
        try
        {
            var products = await _productService.GetActiveProductsAsync();
            var warehouses = await _warehouseService.GetActiveWarehousesAsync();
            var lowStockCount = await _lowStockMonitoringService.GetLowStockProductCountAsync();
            var criticalStockCount = await _lowStockMonitoringService.GetCriticalStockProductCountAsync();

            var summary = new InventorySummaryReportDto
            {
                TotalProducts = products.Count(),
                TotalWarehouses = warehouses.Count(),
                LowStockProducts = lowStockCount,
                CriticalStockProducts = criticalStockCount,
                GeneratedAt = DateTime.UtcNow
            };

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating inventory summary report");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get warehouse capacity report
    /// </summary>
    [HttpGet("warehouse-capacity")]
    public async Task<ActionResult<IEnumerable<WarehouseCapacityReportDto>>> GetWarehouseCapacityReport()
    {
        try
        {
            var warehouses = await _warehouseService.GetAllAsync();
            var capacityReports = new List<WarehouseCapacityReportDto>();

            foreach (var warehouse in warehouses)
            {
                var utilizationReport = await _multiWarehouseStockService.GetWarehouseUtilizationAsync(warehouse.Id);
                
                capacityReports.Add(new WarehouseCapacityReportDto
                {
                    WarehouseId = warehouse.Id,
                    WarehouseName = warehouse.Name,
                    WarehouseCode = warehouse.Code,
                    TotalCapacity = warehouse.Capacity,
                    CurrentUtilization = utilizationReport?.UtilizationPercentage ?? 0,
                    AvailableCapacity = warehouse.Capacity - (int)(warehouse.Capacity * (utilizationReport?.UtilizationPercentage ?? 0) / 100),
                    IsActive = warehouse.IsActive
                });
            }

            return Ok(capacityReports);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating warehouse capacity report");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get product performance report
    /// </summary>
    [HttpGet("product-performance")]
    public async Task<ActionResult<IEnumerable<ProductPerformanceReportDto>>> GetProductPerformanceReport([FromQuery] int topCount = 20)
    {
        try
        {
            var products = await _productService.GetActiveProductsAsync();
            var performanceReports = new List<ProductPerformanceReportDto>();

            foreach (var product in products.Take(topCount))
            {
                var stockLevel = await _lowStockMonitoringService.GetProductStockLevelAsync(product.Id);
                
                performanceReports.Add(new ProductPerformanceReportDto
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Sku = product.Sku,
                    Category = product.Category,
                    TotalStock = stockLevel?.TotalStock ?? 0,
                    ReorderLevel = product.ReorderLevel,
                    IsLowStock = (stockLevel?.TotalStock ?? 0) <= product.ReorderLevel,
                    Price = product.Price,
                    LastUpdated = product.UpdatedAt ?? product.CreatedAt
                });
            }

            return Ok(performanceReports.OrderBy(p => p.TotalStock));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating product performance report");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get stock movement report for a date range
    /// </summary>
    [HttpGet("stock-movements")]
    public async Task<ActionResult<IEnumerable<StockMovementReportDto>>> GetStockMovementReport(
        [FromQuery] DateTime? fromDate = null, 
        [FromQuery] DateTime? toDate = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] Guid? productId = null)
    {
        try
        {
            // This would typically query stock movements from the database
            // For now, return a placeholder response
            await Task.Delay(1); // Remove async warning

            var movements = new List<StockMovementReportDto>
            {
                new StockMovementReportDto
                {
                    MovementId = Guid.NewGuid(),
                    ProductId = productId ?? Guid.NewGuid(),
                    WarehouseId = warehouseId ?? Guid.NewGuid(),
                    MovementType = "Stock In",
                    Quantity = 100,
                    Reason = "Purchase Order Received",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    CreatedBy = "System"
                }
            };

            return Ok(movements);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating stock movement report");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Export inventory data (placeholder for CSV/Excel export)
    /// </summary>
    [HttpGet("export/inventory")]
    public async Task<ActionResult> ExportInventoryData([FromQuery] string format = "csv")
    {
        try
        {
            // This would typically generate and return a file
            // For now, return a simple response
            var products = await _productService.GetActiveProductsAsync();
            
            var exportData = products.Select(p => new
            {
                p.Sku,
                p.Name,
                p.Category,
                p.Price,
                p.ReorderLevel,
                p.IsActive
            });

            return Ok(new { 
                message = $"Export functionality placeholder - {exportData.Count()} products ready for {format.ToUpper()} export",
                format = format,
                recordCount = exportData.Count(),
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting inventory data");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get health status
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "Reports Controller is healthy", timestamp = DateTime.UtcNow });
    }
}
