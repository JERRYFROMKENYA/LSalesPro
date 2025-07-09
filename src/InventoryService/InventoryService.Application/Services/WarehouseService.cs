using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using InventoryService.Domain.Interfaces;

namespace InventoryService.Application.Services;

public class WarehouseService : IWarehouseService
{
    private readonly IWarehouseRepository _warehouseRepository;

    public WarehouseService(IWarehouseRepository warehouseRepository)
    {
        _warehouseRepository = warehouseRepository;
    }

    public async Task<IEnumerable<WarehouseDto>> GetAllAsync()
    {
        var warehouses = await _warehouseRepository.GetAllAsync();
        var result = new List<WarehouseDto>();
        
        foreach (var warehouse in warehouses)
        {
            var dto = await MapToDtoWithCapacityAsync(warehouse);
            result.Add(dto);
        }
        
        return result;
    }

    public async Task<WarehouseDto?> GetByIdAsync(Guid id)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(id);
        if (warehouse == null)
            return null;
            
        return await MapToDtoWithCapacityAsync(warehouse);
    }

    public async Task<WarehouseDto?> GetByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        var warehouse = await _warehouseRepository.GetByCodeAsync(code);
        if (warehouse == null)
            return null;
            
        return await MapToDtoWithCapacityAsync(warehouse);
    }

    public async Task<WarehouseDto> CreateAsync(CreateWarehouseDto dto)
    {
        // Validate code uniqueness
        if (await _warehouseRepository.CodeExistsAsync(dto.Code))
        {
            throw new InvalidOperationException($"Warehouse with code '{dto.Code}' already exists.");
        }

        var warehouse = MapToEntity(dto);
        var createdWarehouse = await _warehouseRepository.CreateAsync(warehouse);
        return await MapToDtoWithCapacityAsync(createdWarehouse);
    }

    public async Task<WarehouseDto?> UpdateAsync(Guid id, UpdateWarehouseDto dto)
    {
        var existingWarehouse = await _warehouseRepository.GetByIdAsync(id);
        if (existingWarehouse == null)
            return null;

        // Update the entity with new values
        existingWarehouse.Name = dto.Name;
        existingWarehouse.Type = dto.Type;
        existingWarehouse.Address = dto.Address;
        existingWarehouse.ManagerEmail = dto.ManagerEmail;
        existingWarehouse.Phone = dto.Phone;
        existingWarehouse.Capacity = dto.Capacity;
        existingWarehouse.Latitude = dto.Latitude;
        existingWarehouse.Longitude = dto.Longitude;
        existingWarehouse.IsActive = dto.IsActive;

        var updatedWarehouse = await _warehouseRepository.UpdateAsync(existingWarehouse);
        if (updatedWarehouse == null)
            return null;
            
        return await MapToDtoWithCapacityAsync(updatedWarehouse);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _warehouseRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<WarehouseDto>> GetByTypeAsync(string type)
    {
        var warehouses = await _warehouseRepository.GetByTypeAsync(type);
        var result = new List<WarehouseDto>();
        
        foreach (var warehouse in warehouses)
        {
            var dto = await MapToDtoWithCapacityAsync(warehouse);
            result.Add(dto);
        }
        
        return result;
    }

    public async Task<IEnumerable<WarehouseDto>> GetActiveWarehousesAsync()
    {
        var warehouses = await _warehouseRepository.GetActiveWarehousesAsync();
        var result = new List<WarehouseDto>();
        
        foreach (var warehouse in warehouses)
        {
            var dto = await MapToDtoWithCapacityAsync(warehouse);
            result.Add(dto);
        }
        
        return result;
    }

    public async Task<WarehousePagedResultDto> SearchPagedAsync(WarehouseSearchDto searchDto)
    {
        IEnumerable<Warehouse> warehouses;

        // Location-based search
        if (searchDto.Latitude.HasValue && searchDto.Longitude.HasValue && searchDto.RadiusKm.HasValue)
        {
            warehouses = await _warehouseRepository.GetNearbyWarehousesAsync(
                searchDto.Latitude.Value, 
                searchDto.Longitude.Value, 
                searchDto.RadiusKm.Value);
        }
        else if (!string.IsNullOrWhiteSpace(searchDto.Type))
        {
            warehouses = await _warehouseRepository.GetByTypeAsync(searchDto.Type);
        }
        else
        {
            warehouses = await _warehouseRepository.GetActiveWarehousesAsync();
        }

        // Apply search term filter
        if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm))
        {
            var term = searchDto.SearchTerm.ToLower();
            warehouses = warehouses.Where(w => 
                w.Name.ToLower().Contains(term) ||
                w.Code.ToLower().Contains(term) ||
                w.Address.ToLower().Contains(term));
        }

        // Apply capacity filters
        if (searchDto.MinCapacity.HasValue)
            warehouses = warehouses.Where(w => w.Capacity >= searchDto.MinCapacity.Value);

        if (searchDto.MaxCapacity.HasValue)
            warehouses = warehouses.Where(w => w.Capacity <= searchDto.MaxCapacity.Value);

        var totalCount = warehouses.Count();
        var pagedWarehouses = warehouses
            .Skip((searchDto.Page - 1) * searchDto.PageSize)
            .Take(searchDto.PageSize);

        var warehouseDtos = new List<WarehouseDto>();
        foreach (var warehouse in pagedWarehouses)
        {
            var dto = await MapToDtoWithCapacityAsync(warehouse);
            warehouseDtos.Add(dto);
        }

        return new WarehousePagedResultDto
        {
            Warehouses = warehouseDtos,
            TotalCount = totalCount,
            Page = searchDto.Page,
            PageSize = searchDto.PageSize
        };
    }

    public async Task<IEnumerable<WarehouseDto>> GetNearbyWarehousesAsync(double latitude, double longitude, double radiusKm)
    {
        var warehouses = await _warehouseRepository.GetNearbyWarehousesAsync(latitude, longitude, radiusKm);
        var result = new List<WarehouseDto>();
        
        foreach (var warehouse in warehouses)
        {
            var dto = await MapToDtoWithCapacityAsync(warehouse);
            result.Add(dto);
        }
        
        return result;
    }

    public async Task<WarehouseCapacityDto> GetWarehouseCapacityAsync(Guid warehouseId)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId);
        if (warehouse == null)
            throw new ArgumentException("Warehouse not found", nameof(warehouseId));

        var availableCapacity = await _warehouseRepository.GetAvailableCapacityAsync(warehouseId);
        var usedCapacity = warehouse.Capacity - availableCapacity;

        return new WarehouseCapacityDto
        {
            WarehouseId = warehouse.Id,
            WarehouseName = warehouse.Name,
            WarehouseCode = warehouse.Code,
            TotalCapacity = warehouse.Capacity,
            UsedCapacity = usedCapacity,
            AvailableCapacity = availableCapacity
        };
    }

    public async Task<IEnumerable<WarehouseCapacityDto>> GetAllWarehouseCapacitiesAsync()
    {
        var warehouses = await _warehouseRepository.GetActiveWarehousesAsync();
        var result = new List<WarehouseCapacityDto>();

        foreach (var warehouse in warehouses)
        {
            var capacityDto = await GetWarehouseCapacityAsync(warehouse.Id);
            result.Add(capacityDto);
        }

        return result.OrderBy(c => c.WarehouseName);
    }

    public async Task<IEnumerable<WarehouseDto>> GetWarehousesWithCapacityAsync(int minimumCapacity)
    {
        var warehouses = await _warehouseRepository.GetWarehousesWithCapacityAsync(minimumCapacity);
        var result = new List<WarehouseDto>();
        
        foreach (var warehouse in warehouses)
        {
            var dto = await MapToDtoWithCapacityAsync(warehouse);
            result.Add(dto);
        }
        
        return result;
    }

    public async Task<IEnumerable<string>> GetWarehouseTypesAsync()
    {
        return await _warehouseRepository.GetWarehouseTypesAsync();
    }

    public async Task<bool> ValidateCodeAsync(string code, Guid? excludeId = null)
    {
        return !await _warehouseRepository.CodeExistsAsync(code, excludeId);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _warehouseRepository.ExistsAsync(id);
    }

    public async Task<bool> ActivateWarehouseAsync(Guid id)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(id);
        if (warehouse == null)
            return false;

        warehouse.IsActive = true;
        await _warehouseRepository.UpdateAsync(warehouse);
        return true;
    }

    public async Task<bool> DeactivateWarehouseAsync(Guid id)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(id);
        if (warehouse == null)
            return false;

        warehouse.IsActive = false;
        await _warehouseRepository.UpdateAsync(warehouse);
        return true;
    }

    public async Task<IEnumerable<WarehouseDto>> GetOvercapacityWarehousesAsync()
    {
        var capacities = await GetAllWarehouseCapacitiesAsync();
        var overcapacityWarehouses = capacities.Where(c => c.UtilizationPercentage > 90);

        var result = new List<WarehouseDto>();
        foreach (var capacity in overcapacityWarehouses)
        {
            var warehouse = await GetByIdAsync(capacity.WarehouseId);
            if (warehouse != null)
                result.Add(warehouse);
        }

        return result;
    }

    // Mapping methods
    private async Task<WarehouseDto> MapToDtoWithCapacityAsync(Warehouse warehouse)
    {
        var availableCapacity = await _warehouseRepository.GetAvailableCapacityAsync(warehouse.Id);
        
        return new WarehouseDto
        {
            Id = warehouse.Id,
            Code = warehouse.Code,
            Name = warehouse.Name,
            Type = warehouse.Type,
            Address = warehouse.Address,
            ManagerEmail = warehouse.ManagerEmail,
            Phone = warehouse.Phone,
            Capacity = warehouse.Capacity,
            Latitude = warehouse.Latitude,
            Longitude = warehouse.Longitude,
            IsActive = warehouse.IsActive,
            CreatedAt = warehouse.CreatedAt,
            AvailableCapacity = availableCapacity
        };
    }

    private static Warehouse MapToEntity(CreateWarehouseDto dto)
    {
        return new Warehouse
        {
            Id = Guid.NewGuid(),
            Code = dto.Code,
            Name = dto.Name,
            Type = dto.Type,
            Address = dto.Address,
            ManagerEmail = dto.ManagerEmail,
            Phone = dto.Phone,
            Capacity = dto.Capacity,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };
    }
}
