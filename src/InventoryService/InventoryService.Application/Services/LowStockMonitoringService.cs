using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Interfaces;

namespace InventoryService.Application.Services;

public class LowStockMonitoringService : ILowStockMonitoringService
{
    private readonly IProductRepository _productRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IInventoryItemRepository _inventoryItemRepository;

    public LowStockMonitoringService(
        IProductRepository productRepository,
        IWarehouseRepository warehouseRepository,
        IInventoryItemRepository inventoryItemRepository)
    {
        _productRepository = productRepository;
        _warehouseRepository = warehouseRepository;
        _inventoryItemRepository = inventoryItemRepository;
    }

    public async Task<IEnumerable<LowStockAlertDto>> GetLowStockAlertsAsync()
    {
        var alerts = new List<LowStockAlertDto>();
        
        // Get all active products
        var products = await _productRepository.GetActiveProductsAsync();
        
        foreach (var product in products)
        {
            // Get inventory items for this product across all warehouses
            var inventoryItems = await _inventoryItemRepository.GetByProductIdAsync(product.Id);
            
            foreach (var inventoryItem in inventoryItems)
            {
                if (inventoryItem.AvailableQuantity <= product.ReorderLevel)
                {
                    var warehouse = await _warehouseRepository.GetByIdAsync(inventoryItem.WarehouseId);
                    if (warehouse != null)
                    {
                        alerts.Add(new LowStockAlertDto
                        {
                            ProductId = product.Id,
                            ProductName = product.Name,
                            ProductSku = product.Sku,
                            WarehouseId = warehouse.Id,
                            WarehouseName = warehouse.Name,
                            WarehouseCode = warehouse.Code,
                            CurrentStock = inventoryItem.AvailableQuantity,
                            ReorderLevel = product.ReorderLevel,
                            Severity = GetAlertSeverity(inventoryItem.AvailableQuantity, product.ReorderLevel),
                            AlertGeneratedAt = DateTime.UtcNow
                        });
                    }
                }
            }
        }
        
        return alerts.OrderByDescending(a => a.Severity).ThenBy(a => a.CurrentStock);
    }

    public async Task<IEnumerable<LowStockAlertDto>> GetLowStockAlertsByWarehouseAsync(Guid warehouseId)
    {
        var allAlerts = await GetLowStockAlertsAsync();
        return allAlerts.Where(a => a.WarehouseId == warehouseId);
    }

    public async Task<LowStockAlertDto?> GetLowStockAlertByProductAsync(Guid productId, Guid warehouseId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) return null;

        var inventoryItem = await _inventoryItemRepository.GetByProductAndWarehouseAsync(productId, warehouseId);
        if (inventoryItem == null || inventoryItem.AvailableQuantity > product.ReorderLevel) 
            return null;

        var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId);
        if (warehouse == null) return null;

        return new LowStockAlertDto
        {
            ProductId = product.Id,
            ProductName = product.Name,
            ProductSku = product.Sku,
            WarehouseId = warehouse.Id,
            WarehouseName = warehouse.Name,
            WarehouseCode = warehouse.Code,
            CurrentStock = inventoryItem.AvailableQuantity,
            ReorderLevel = product.ReorderLevel,
            Severity = GetAlertSeverity(inventoryItem.AvailableQuantity, product.ReorderLevel),
            AlertGeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<ReorderSuggestionDto>> GetReorderSuggestionsAsync()
    {
        var suggestions = new List<ReorderSuggestionDto>();
        var lowStockAlerts = await GetLowStockAlertsAsync();
        
        foreach (var alert in lowStockAlerts)
        {
            var suggestedQuantity = CalculateSuggestedOrderQuantity(
                alert.CurrentStock, 
                alert.ReorderLevel, 
                alert.StockShortage);
                
            var product = await _productRepository.GetByIdAsync(alert.ProductId);
            var estimatedCost = product != null ? product.Price * suggestedQuantity : 0;
            
            suggestions.Add(new ReorderSuggestionDto
            {
                ProductId = alert.ProductId,
                ProductName = alert.ProductName,
                ProductSku = alert.ProductSku,
                WarehouseId = alert.WarehouseId,
                WarehouseName = alert.WarehouseName,
                CurrentStock = alert.CurrentStock,
                ReorderLevel = alert.ReorderLevel,
                SuggestedOrderQuantity = suggestedQuantity,
                EstimatedCost = estimatedCost,
                LastOrderDate = DateTime.UtcNow.AddDays(-30) // Placeholder - would come from order history
            });
        }
        
        return suggestions.OrderByDescending(s => s.EstimatedCost);
    }

    public async Task<IEnumerable<ReorderSuggestionDto>> GetReorderSuggestionsByWarehouseAsync(Guid warehouseId)
    {
        var allSuggestions = await GetReorderSuggestionsAsync();
        return allSuggestions.Where(s => s.WarehouseId == warehouseId);
    }

    public async Task<IEnumerable<StockLevelReportDto>> GetStockLevelReportAsync()
    {
        var reports = new List<StockLevelReportDto>();
        var products = await _productRepository.GetActiveProductsAsync();
        
        foreach (var product in products)
        {
            var inventoryItems = await _inventoryItemRepository.GetByProductIdAsync(product.Id);
            var warehouseBreakdown = new List<WarehouseStockBreakdownDto>();
            
            foreach (var inventoryItem in inventoryItems)
            {
                var warehouse = await _warehouseRepository.GetByIdAsync(inventoryItem.WarehouseId);
                if (warehouse != null)
                {
                    warehouseBreakdown.Add(new WarehouseStockBreakdownDto
                    {
                        WarehouseId = warehouse.Id,
                        WarehouseName = warehouse.Name,
                        WarehouseCode = warehouse.Code,
                        AvailableQuantity = inventoryItem.AvailableQuantity,
                        ReservedQuantity = inventoryItem.ReservedQuantity,
                        LastUpdated = inventoryItem.LastUpdated
                    });
                }
            }
            
            var totalAvailable = warehouseBreakdown.Sum(w => w.AvailableQuantity);
            var totalReserved = warehouseBreakdown.Sum(w => w.ReservedQuantity);
            
            reports.Add(new StockLevelReportDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ProductSku = product.Sku,
                Category = product.Category,
                TotalAvailableStock = totalAvailable,
                TotalReservedStock = totalReserved,
                ReorderLevel = product.ReorderLevel,
                WarehouseBreakdown = warehouseBreakdown
            });
        }
        
        return reports.OrderBy(r => r.TotalAvailableStock);
    }

    public async Task<StockLevelReportDto?> GetProductStockLevelAsync(Guid productId)
    {
        var allReports = await GetStockLevelReportAsync();
        return allReports.FirstOrDefault(r => r.ProductId == productId);
    }

    public async Task<bool> UpdateReorderLevelAsync(Guid productId, int newReorderLevel)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) return false;

        product.ReorderLevel = newReorderLevel;
        product.UpdatedAt = DateTime.UtcNow;
        
        var updated = await _productRepository.UpdateAsync(product);
        return updated != null;
    }

    public async Task<IEnumerable<ProductDto>> GetProductsWithLowStockAsync()
    {
        var lowStockAlerts = await GetLowStockAlertsAsync();
        var productIds = lowStockAlerts.Select(a => a.ProductId).Distinct();
        
        var products = new List<ProductDto>();
        foreach (var productId in productIds)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product != null)
            {
                products.Add(MapToDto(product));
            }
        }
        
        return products;
    }

    public async Task ProcessLowStockAlertsAsync()
    {
        var alerts = await GetLowStockAlertsAsync();
        
        // Log critical alerts
        var criticalAlerts = alerts.Where(a => a.Severity == AlertSeverity.Critical);
        foreach (var alert in criticalAlerts)
        {
            // In a real application, this would:
            // - Send notifications to managers
            // - Create automatic purchase orders
            // - Log to monitoring systems
            Console.WriteLine($"CRITICAL STOCK ALERT: {alert.ProductName} at {alert.WarehouseName} - Only {alert.CurrentStock} units remaining!");
        }
        
        // For now, just simulate processing
        await Task.CompletedTask;
    }

    public async Task<int> GetLowStockProductCountAsync()
    {
        var alerts = await GetLowStockAlertsAsync();
        return alerts.Select(a => a.ProductId).Distinct().Count();
    }

    public async Task<int> GetCriticalStockProductCountAsync(int criticalLevel = 5)
    {
        var reports = await GetStockLevelReportAsync();
        return reports.Count(r => r.TotalAvailableStock <= criticalLevel);
    }

    // Helper methods
    private static AlertSeverity GetAlertSeverity(int currentStock, int reorderLevel)
    {
        if (currentStock <= 0) return AlertSeverity.Critical;
        if (currentStock <= 5) return AlertSeverity.High;
        if (currentStock <= reorderLevel * 0.5) return AlertSeverity.Medium;
        return AlertSeverity.Low;
    }

    private static int CalculateSuggestedOrderQuantity(int currentStock, int reorderLevel, int stockShortage)
    {
        // Simple algorithm: order enough to reach 150% of reorder level
        var targetStock = (int)(reorderLevel * 1.5);
        var suggestedQuantity = Math.Max(targetStock - currentStock, stockShortage);
        
        // Ensure minimum order of 10 units
        return Math.Max(suggestedQuantity, 10);
    }

    private static ProductDto MapToDto(Domain.Entities.Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Sku = product.Sku,
            Name = product.Name,
            Description = product.Description,
            Category = product.Category,
            Subcategory = product.Subcategory,
            Price = product.Price,
            TaxRate = product.TaxRate,
            Unit = product.Unit,
            Packaging = product.Packaging,
            MinOrderQuantity = product.MinOrderQuantity,
            ReorderLevel = product.ReorderLevel,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
