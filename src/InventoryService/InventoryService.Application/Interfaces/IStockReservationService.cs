using InventoryService.Application.DTOs;

namespace InventoryService.Application.Interfaces;

public interface IStockReservationService
{
    // Reservation management
    Task<StockReservationResultDto> ReserveStockAsync(CreateStockReservationDto dto);
    Task<StockReservationResultDto> ReleaseReservationAsync(string reservationId);
    Task<bool> ExtendReservationAsync(string reservationId, int additionalMinutes);
    Task<StockReservationDto?> GetReservationAsync(string reservationId);

    // Availability checks
    Task<StockAvailabilityDto> GetStockAvailabilityAsync(Guid productId, Guid warehouseId);
    Task<bool> CanReserveQuantityAsync(Guid productId, Guid warehouseId, int quantity);
    Task<int> GetAvailableQuantityAsync(Guid productId, Guid warehouseId);

    // Multi-warehouse operations
    Task<IEnumerable<StockAvailabilityDto>> GetMultiWarehouseAvailabilityAsync(Guid productId);
    Task<StockReservationResultDto> ReserveFromBestWarehouseAsync(Guid productId, int quantity, string? reason = null);

    // Maintenance operations
    Task<int> CleanupExpiredReservationsAsync();
    Task<IEnumerable<StockReservationDto>> GetExpiredReservationsAsync();
    Task<IEnumerable<StockReservationDto>> GetActiveReservationsAsync(Guid? productId = null);
}
