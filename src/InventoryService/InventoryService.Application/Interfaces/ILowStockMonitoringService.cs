using InventoryService.Application.DTOs;

namespace InventoryService.Application.Interfaces;

public interface ILowStockMonitoringService
{
    // Low stock alerts
    Task<IEnumerable<LowStockAlertDto>> GetLowStockAlertsAsync();
    Task<IEnumerable<LowStockAlertDto>> GetLowStockAlertsByWarehouseAsync(Guid warehouseId);
    Task<LowStockAlertDto?> GetLowStockAlertByProductAsync(Guid productId, Guid warehouseId);
    
    // Reorder suggestions
    Task<IEnumerable<ReorderSuggestionDto>> GetReorderSuggestionsAsync();
    Task<IEnumerable<ReorderSuggestionDto>> GetReorderSuggestionsByWarehouseAsync(Guid warehouseId);
    
    // Stock level monitoring
    Task<IEnumerable<StockLevelReportDto>> GetStockLevelReportAsync();
    Task<StockLevelReportDto?> GetProductStockLevelAsync(Guid productId);
    
    // Alert configuration
    Task<bool> UpdateReorderLevelAsync(Guid productId, int newReorderLevel);
    Task<IEnumerable<ProductDto>> GetProductsWithLowStockAsync();
    
    // Monitoring operations
    Task ProcessLowStockAlertsAsync();
    Task<int> GetLowStockProductCountAsync();
    Task<int> GetCriticalStockProductCountAsync(int criticalLevel = 5);
}
