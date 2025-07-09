using InventoryService.Application.DTOs;

namespace InventoryService.Application.Interfaces;

public interface IWarehouseService
{
    // Basic CRUD operations
    Task<IEnumerable<WarehouseDto>> GetAllAsync();
    Task<WarehouseDto?> GetByIdAsync(Guid id);
    Task<WarehouseDto?> GetByCodeAsync(string code);
    Task<WarehouseDto> CreateAsync(CreateWarehouseDto dto);
    Task<WarehouseDto?> UpdateAsync(Guid id, UpdateWarehouseDto dto);
    Task<bool> DeleteAsync(Guid id);

    // Search and filtering
    Task<IEnumerable<WarehouseDto>> GetByTypeAsync(string type);
    Task<IEnumerable<WarehouseDto>> GetActiveWarehousesAsync();
    Task<WarehousePagedResultDto> SearchPagedAsync(WarehouseSearchDto searchDto);
    Task<IEnumerable<WarehouseDto>> GetNearbyWarehousesAsync(double latitude, double longitude, double radiusKm);

    // Capacity management
    Task<WarehouseCapacityDto> GetWarehouseCapacityAsync(Guid warehouseId);
    Task<IEnumerable<WarehouseCapacityDto>> GetAllWarehouseCapacitiesAsync();
    Task<IEnumerable<WarehouseDto>> GetWarehousesWithCapacityAsync(int minimumCapacity);

    // Type management
    Task<IEnumerable<string>> GetWarehouseTypesAsync();

    // Validation
    Task<bool> ValidateCodeAsync(string code, Guid? excludeId = null);
    Task<bool> ExistsAsync(Guid id);

    // Business operations
    Task<bool> ActivateWarehouseAsync(Guid id);
    Task<bool> DeactivateWarehouseAsync(Guid id);
    Task<IEnumerable<WarehouseDto>> GetOvercapacityWarehousesAsync();
}
