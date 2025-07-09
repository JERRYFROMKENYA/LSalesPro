using InventoryService.Domain.Entities;

namespace InventoryService.Domain.Interfaces;

public interface IWarehouseRepository
{
    Task<IEnumerable<Warehouse>> GetAllAsync();
    Task<Warehouse?> GetByIdAsync(Guid id);
    Task<Warehouse?> GetByCodeAsync(string code);
    Task<IEnumerable<Warehouse>> GetByTypeAsync(string type);
    Task<IEnumerable<Warehouse>> GetActiveWarehousesAsync();
    Task<IEnumerable<Warehouse>> GetNearbyWarehousesAsync(double latitude, double longitude, double radiusKm);
    Task<Warehouse> CreateAsync(Warehouse warehouse);
    Task<Warehouse?> UpdateAsync(Warehouse warehouse);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null);
    Task<IEnumerable<string>> GetWarehouseTypesAsync();
    Task<int> GetTotalCountAsync();
    Task<IEnumerable<Warehouse>> GetPagedAsync(int page, int pageSize);
    Task<int> GetAvailableCapacityAsync(Guid warehouseId);
    Task<IEnumerable<Warehouse>> GetWarehousesWithCapacityAsync(int minimumCapacity);
    Task<IEnumerable<Warehouse>> GetByIdsAsync(List<Guid> ids);
}
