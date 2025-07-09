using InventoryService.Domain.Entities;

namespace InventoryService.Domain.Interfaces;

public interface IStockReservationRepository
{
    Task<StockReservation> CreateReservationAsync(StockReservation reservation);
    Task<StockReservation?> GetByReservationIdAsync(string reservationId);
    Task<IEnumerable<StockReservation>> GetActiveReservationsAsync(Guid productId, Guid warehouseId);
    Task<IEnumerable<StockReservation>> GetExpiredReservationsAsync();
    Task<bool> ReleaseReservationAsync(string reservationId);
    Task<bool> ExtendReservationAsync(string reservationId, DateTime newExpiryTime);
    Task<int> GetTotalReservedQuantityAsync(Guid productId, Guid warehouseId);
    Task<bool> CanReserveQuantityAsync(Guid productId, Guid warehouseId, int quantity);
    Task<IEnumerable<StockReservation>> GetReservationsByProductAsync(Guid productId);
    Task CleanupExpiredReservationsAsync();
}
