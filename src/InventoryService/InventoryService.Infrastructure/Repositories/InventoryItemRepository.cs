using InventoryService.Domain.Entities;
using InventoryService.Domain.Interfaces;
using InventoryService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Repositories;

public class InventoryItemRepository : IInventoryItemRepository
{
    private readonly InventoryDbContext _context;

    public InventoryItemRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryItem?> GetByProductAndWarehouseAsync(Guid productId, Guid warehouseId)
    {
        return await _context.InventoryItems
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .FirstOrDefaultAsync(i => i.ProductId == productId && i.WarehouseId == warehouseId);
    }

    public async Task<IEnumerable<InventoryItem>> GetByProductAsync(Guid productId)
    {
        return await _context.InventoryItems
            .Include(i => i.Warehouse)
            .Where(i => i.ProductId == productId)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryItem>> GetByWarehouseAsync(Guid warehouseId)
    {
        return await _context.InventoryItems
            .Include(i => i.Product)
            .Where(i => i.WarehouseId == warehouseId)
            .ToListAsync();
    }

    public async Task<InventoryItem> CreateAsync(InventoryItem inventoryItem)
    {
        inventoryItem.LastUpdated = DateTime.UtcNow;
        _context.InventoryItems.Add(inventoryItem);
        await _context.SaveChangesAsync();
        return inventoryItem;
    }

    public async Task<InventoryItem?> UpdateAsync(InventoryItem inventoryItem)
    {
        var existingItem = await GetByProductAndWarehouseAsync(inventoryItem.ProductId, inventoryItem.WarehouseId);
        if (existingItem == null)
            return null;

        existingItem.AvailableQuantity = inventoryItem.AvailableQuantity;
        existingItem.ReservedQuantity = inventoryItem.ReservedQuantity;
        existingItem.LastUpdated = DateTime.UtcNow;
        existingItem.LastUpdatedBy = inventoryItem.LastUpdatedBy;

        await _context.SaveChangesAsync();
        return existingItem;
    }

    public async Task<bool> UpdateQuantityAsync(Guid productId, Guid warehouseId, int newAvailableQuantity, int newReservedQuantity)
    {
        var inventoryItem = await GetByProductAndWarehouseAsync(productId, warehouseId);
        if (inventoryItem == null)
            return false;

        inventoryItem.AvailableQuantity = newAvailableQuantity;
        inventoryItem.ReservedQuantity = newReservedQuantity;
        inventoryItem.LastUpdated = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetTotalAvailableQuantityAsync(Guid productId)
    {
        return await _context.InventoryItems
            .Where(i => i.ProductId == productId)
            .SumAsync(i => i.AvailableQuantity);
    }

    public async Task<IEnumerable<InventoryItem>> GetLowStockItemsAsync(int threshold = 10)
    {
        return await _context.InventoryItems
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Where(i => i.AvailableQuantity <= threshold)
            .ToListAsync();
    }

    public async Task<bool> TransferStockAsync(Guid productId, Guid fromWarehouseId, Guid toWarehouseId, int quantity)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var fromItem = await GetByProductAndWarehouseAsync(productId, fromWarehouseId);
            var toItem = await GetByProductAndWarehouseAsync(productId, toWarehouseId);

            if (fromItem == null || fromItem.AvailableQuantity < quantity)
            {
                await transaction.RollbackAsync();
                return false;
            }

            // Reduce quantity from source warehouse
            fromItem.AvailableQuantity -= quantity;
            fromItem.LastUpdated = DateTime.UtcNow;

            // Add quantity to destination warehouse
            if (toItem == null)
            {
                // Create new inventory item for destination warehouse
                toItem = new InventoryItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    WarehouseId = toWarehouseId,
                    AvailableQuantity = quantity,
                    ReservedQuantity = 0,
                    LastUpdated = DateTime.UtcNow
                };
                _context.InventoryItems.Add(toItem);
            }
            else
            {
                toItem.AvailableQuantity += quantity;
                toItem.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task<IEnumerable<InventoryItem>> GetByProductIdAsync(Guid productId)
    {
        return await GetByProductAsync(productId);
    }

    public async Task<IEnumerable<InventoryItem>> GetByWarehouseIdAsync(Guid warehouseId)
    {
        return await GetByWarehouseAsync(warehouseId);
    }
}
