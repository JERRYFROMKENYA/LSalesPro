using InventoryService.Domain.Entities;

namespace InventoryService.Domain.Interfaces;

public interface IInventoryItemRepository
{
    Task<InventoryItem?> GetByProductAndWarehouseAsync(Guid productId, Guid warehouseId);
    Task<IEnumerable<InventoryItem>> GetByProductAsync(Guid productId);
    Task<IEnumerable<InventoryItem>> GetByProductIdAsync(Guid productId); // Alias for consistency
    Task<IEnumerable<InventoryItem>> GetByWarehouseAsync(Guid warehouseId);
    Task<IEnumerable<InventoryItem>> GetByWarehouseIdAsync(Guid warehouseId); // Alias for consistency
    Task<InventoryItem> CreateAsync(InventoryItem inventoryItem);
    Task<InventoryItem?> UpdateAsync(InventoryItem inventoryItem);
    Task<bool> UpdateQuantityAsync(Guid productId, Guid warehouseId, int newAvailableQuantity, int newReservedQuantity);
    Task<int> GetTotalAvailableQuantityAsync(Guid productId);
    Task<IEnumerable<InventoryItem>> GetLowStockItemsAsync(int threshold = 10);
    Task<bool> TransferStockAsync(Guid productId, Guid fromWarehouseId, Guid toWarehouseId, int quantity);
}
