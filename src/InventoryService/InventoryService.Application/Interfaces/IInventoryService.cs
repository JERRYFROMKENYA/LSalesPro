namespace InventoryService.Application.Interfaces;

public interface IInventoryService
{
    Task<bool> CheckAvailabilityAsync(Guid productId, int quantity, Guid? warehouseId = null);
    Task<bool> ReserveStockAsync(Guid productId, int quantity, Guid? warehouseId = null);
    Task<bool> ReleaseStockAsync(Guid productId, int quantity, Guid? warehouseId = null);
    Task<int> GetAvailableStockAsync(Guid productId, Guid? warehouseId = null);
}
