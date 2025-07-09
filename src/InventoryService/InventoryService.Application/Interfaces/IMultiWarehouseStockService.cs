using InventoryService.Application.DTOs;

namespace InventoryService.Application.Interfaces;

public interface IMultiWarehouseStockService
{
    // Stock availability across warehouses
    Task<int> GetTotalAvailableStockAsync(Guid productId);
    Task<Dictionary<Guid, int>> GetStockByWarehouseAsync(Guid productId);
    Task<bool> IsStockAvailableAsync(Guid productId, int requiredQuantity);
    Task<MultiWarehouseStockAvailabilityDto> CheckStockAvailabilityAsync(Guid productId, int requiredQuantity);

    // Stock allocation optimization
    Task<List<StockAllocationDto>> OptimizeStockAllocationAsync(Guid productId, int requiredQuantity, double? customerLatitude = null, double? customerLongitude = null);
    Task<StockAllocationDto?> FindBestWarehouseAsync(Guid productId, int requiredQuantity, double? latitude = null, double? longitude = null);

    // Stock transfers
    Task<StockTransferResultDto> TransferStockAsync(CreateStockTransferDto request);
    Task<List<StockTransferDto>> GetPendingTransfersAsync();
    Task<bool> ConfirmStockTransferAsync(Guid transferId);

    // Stock distribution analysis
    Task<List<StockDistributionDto>> GetStockDistributionAsync(Guid? productId = null);
    Task<List<WarehouseStockSummaryDto>> GetWarehouseStockSummariesAsync();
    
    // Rebalancing suggestions
    Task<List<StockRebalanceRecommendationDto>> GetStockRebalanceRecommendationsAsync();
    Task<bool> ExecuteStockRebalanceAsync(Guid recommendationId);
    
    // Warehouse utilization
    Task<WarehouseUtilizationDto> GetWarehouseUtilizationAsync(Guid warehouseId);
}
