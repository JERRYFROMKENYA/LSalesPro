using InventoryService.Domain.Entities;
using InventoryService.Domain.Interfaces;
using InventoryService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Repositories;

public class StockReservationRepository : IStockReservationRepository
{
    private readonly InventoryDbContext _context;

    public StockReservationRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<StockReservation> CreateReservationAsync(StockReservation reservation)
    {
        _context.StockReservations.Add(reservation);
        await _context.SaveChangesAsync();
        return reservation;
    }

    public async Task<StockReservation?> GetByReservationIdAsync(string reservationId)
    {
        return await _context.StockReservations
            .Include(r => r.Product)
            .Include(r => r.Warehouse)
            .FirstOrDefaultAsync(r => r.ReservationId == reservationId);
    }

    public async Task<IEnumerable<StockReservation>> GetActiveReservationsAsync(Guid productId, Guid warehouseId)
    {
        return await _context.StockReservations
            .Where(r => r.ProductId == productId && 
                       r.WarehouseId == warehouseId && 
                       r.ReleasedAt == null && 
                       r.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<IEnumerable<StockReservation>> GetExpiredReservationsAsync()
    {
        return await _context.StockReservations
            .Where(r => r.ReleasedAt == null && r.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<bool> ReleaseReservationAsync(string reservationId)
    {
        var reservation = await GetByReservationIdAsync(reservationId);
        if (reservation == null || reservation.ReleasedAt != null)
            return false;

        reservation.ReleasedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExtendReservationAsync(string reservationId, DateTime newExpiryTime)
    {
        var reservation = await GetByReservationIdAsync(reservationId);
        if (reservation == null || reservation.ReleasedAt != null)
            return false;

        reservation.ExpiresAt = newExpiryTime;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetTotalReservedQuantityAsync(Guid productId, Guid warehouseId)
    {
        return await _context.StockReservations
            .Where(r => r.ProductId == productId && 
                       r.WarehouseId == warehouseId && 
                       r.ReleasedAt == null && 
                       r.ExpiresAt > DateTime.UtcNow)
            .SumAsync(r => r.Quantity);
    }

    public async Task<bool> CanReserveQuantityAsync(Guid productId, Guid warehouseId, int quantity)
    {
        var inventoryItem = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.ProductId == productId && i.WarehouseId == warehouseId);

        if (inventoryItem == null)
            return false;

        var totalReserved = await GetTotalReservedQuantityAsync(productId, warehouseId);
        var availableQuantity = inventoryItem.AvailableQuantity - totalReserved;

        return availableQuantity >= quantity;
    }

    public async Task<IEnumerable<StockReservation>> GetReservationsByProductAsync(Guid productId)
    {
        return await _context.StockReservations
            .Where(r => r.ProductId == productId && r.ReleasedAt == null)
            .Include(r => r.Warehouse)
            .ToListAsync();
    }

    public async Task CleanupExpiredReservationsAsync()
    {
        var expiredReservations = await GetExpiredReservationsAsync();
        
        foreach (var reservation in expiredReservations)
        {
            reservation.ReleasedAt = DateTime.UtcNow;
        }

        if (expiredReservations.Any())
        {
            await _context.SaveChangesAsync();
        }
    }
}
