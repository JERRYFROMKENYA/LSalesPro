using InventoryService.Domain.Entities;
using InventoryService.Domain.Interfaces;
using InventoryService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Repositories;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly InventoryDbContext _context;

    public WarehouseRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Warehouse>> GetAllAsync()
    {
        return await _context.Warehouses
            .OrderBy(w => w.Name)
            .ToListAsync();
    }

    public async Task<Warehouse?> GetByIdAsync(Guid id)
    {
        return await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<Warehouse?> GetByCodeAsync(string code)
    {
        return await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Code == code);
    }

    public async Task<IEnumerable<Warehouse>> GetByTypeAsync(string type)
    {
        return await _context.Warehouses
            .Where(w => w.Type == type && w.IsActive)
            .OrderBy(w => w.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Warehouse>> GetActiveWarehousesAsync()
    {
        return await _context.Warehouses
            .Where(w => w.IsActive)
            .OrderBy(w => w.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Warehouse>> GetNearbyWarehousesAsync(double latitude, double longitude, double radiusKm)
    {
        // Simple distance calculation using Haversine formula approximation
        // For production, consider using spatial data types or PostGIS
        return await _context.Warehouses
            .Where(w => w.IsActive && w.Latitude.HasValue && w.Longitude.HasValue)
            .ToListAsync()
            .ContinueWith(task =>
            {
                var warehouses = task.Result;
                return warehouses.Where(w =>
                {
                    var distance = CalculateDistance(latitude, longitude, w.Latitude!.Value, w.Longitude!.Value);
                    return distance <= radiusKm;
                }).OrderBy(w => w.Name);
            });
    }

    public async Task<Warehouse> CreateAsync(Warehouse warehouse)
    {
        warehouse.CreatedAt = DateTime.UtcNow;
        
        _context.Warehouses.Add(warehouse);
        await _context.SaveChangesAsync();
        return warehouse;
    }

    public async Task<Warehouse?> UpdateAsync(Warehouse warehouse)
    {
        var existingWarehouse = await GetByIdAsync(warehouse.Id);
        if (existingWarehouse == null)
            return null;

        existingWarehouse.Name = warehouse.Name;
        existingWarehouse.Type = warehouse.Type;
        existingWarehouse.Address = warehouse.Address;
        existingWarehouse.ManagerEmail = warehouse.ManagerEmail;
        existingWarehouse.Phone = warehouse.Phone;
        existingWarehouse.Capacity = warehouse.Capacity;
        existingWarehouse.Latitude = warehouse.Latitude;
        existingWarehouse.Longitude = warehouse.Longitude;
        existingWarehouse.IsActive = warehouse.IsActive;

        await _context.SaveChangesAsync();
        return existingWarehouse;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var warehouse = await GetByIdAsync(id);
        if (warehouse == null)
            return false;

        // Soft delete - just mark as inactive
        warehouse.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Warehouses.AnyAsync(w => w.Id == id);
    }

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null)
    {
        var query = _context.Warehouses.Where(w => w.Code == code);
        if (excludeId.HasValue)
            query = query.Where(w => w.Id != excludeId.Value);
        
        return await query.AnyAsync();
    }

    public async Task<IEnumerable<string>> GetWarehouseTypesAsync()
    {
        return await _context.Warehouses
            .Where(w => w.IsActive)
            .Select(w => w.Type)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Warehouses.CountAsync(w => w.IsActive);
    }

    public async Task<IEnumerable<Warehouse>> GetPagedAsync(int page, int pageSize)
    {
        return await _context.Warehouses
            .Where(w => w.IsActive)
            .OrderBy(w => w.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetAvailableCapacityAsync(Guid warehouseId)
    {
        var warehouse = await GetByIdAsync(warehouseId);
        if (warehouse == null)
            return 0;

        // Calculate used capacity based on inventory items
        var usedCapacity = await _context.InventoryItems
            .Where(ii => ii.WarehouseId == warehouseId)
            .SumAsync(ii => ii.AvailableQuantity + ii.ReservedQuantity);

        return Math.Max(0, warehouse.Capacity - usedCapacity);
    }

    public async Task<IEnumerable<Warehouse>> GetWarehousesWithCapacityAsync(int minimumCapacity)
    {
        var warehouses = await GetActiveWarehousesAsync();
        var result = new List<Warehouse>();

        foreach (var warehouse in warehouses)
        {
            var availableCapacity = await GetAvailableCapacityAsync(warehouse.Id);
            if (availableCapacity >= minimumCapacity)
            {
                result.Add(warehouse);
            }
        }

        return result.OrderBy(w => w.Name);
    }

    public async Task<IEnumerable<Warehouse>> GetByIdsAsync(List<Guid> ids)
    {
        return await _context.Warehouses
            .Where(w => ids.Contains(w.Id))
            .ToListAsync();
    }

    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        // Haversine formula for calculating distance between two points on Earth
        const double R = 6371; // Earth's radius in kilometers

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        
        return R * c;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}
