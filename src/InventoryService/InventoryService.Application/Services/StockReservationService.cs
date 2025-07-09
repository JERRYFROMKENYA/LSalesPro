using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using InventoryService.Domain.Interfaces;

namespace InventoryService.Application.Services;

public class StockReservationService : IStockReservationService
{
    private readonly IStockReservationRepository _reservationRepository;
    private readonly IProductRepository _productRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IInventoryItemRepository _inventoryItemRepository;

    public StockReservationService(
        IStockReservationRepository reservationRepository,
        IProductRepository productRepository,
        IWarehouseRepository warehouseRepository,
        IInventoryItemRepository inventoryItemRepository)
    {
        _reservationRepository = reservationRepository;
        _productRepository = productRepository;
        _warehouseRepository = warehouseRepository;
        _inventoryItemRepository = inventoryItemRepository;
    }

    public async Task<StockReservationResultDto> ReserveStockAsync(CreateStockReservationDto dto)
    {
        // Validate product exists
        var product = await _productRepository.GetByIdAsync(dto.ProductId);
        if (product == null)
        {
            return new StockReservationResultDto
            {
                Success = false,
                Message = "Product not found"
            };
        }

        // Validate warehouse exists
        var warehouse = await _warehouseRepository.GetByIdAsync(dto.WarehouseId);
        if (warehouse == null)
        {
            return new StockReservationResultDto
            {
                Success = false,
                Message = "Warehouse not found"
            };
        }

        // Check if quantity can be reserved
        var canReserve = await _reservationRepository.CanReserveQuantityAsync(
            dto.ProductId, dto.WarehouseId, dto.Quantity);

        if (!canReserve)
        {
            return new StockReservationResultDto
            {
                Success = false,
                Message = "Insufficient stock available for reservation"
            };
        }

        // Create reservation
        var reservationId = GenerateReservationId();
        var reservation = new StockReservation
        {
            Id = Guid.NewGuid(),
            ProductId = dto.ProductId,
            WarehouseId = dto.WarehouseId,
            ReservationId = reservationId,
            Quantity = dto.Quantity,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(dto.ReservationDurationMinutes),
            Reason = dto.Reason
        };

        var createdReservation = await _reservationRepository.CreateReservationAsync(reservation);

        return new StockReservationResultDto
        {
            Success = true,
            ReservationId = reservationId,
            Message = "Stock reserved successfully",
            Reservation = MapToDto(createdReservation)
        };
    }

    public async Task<StockReservationResultDto> ReleaseReservationAsync(string reservationId)
    {
        var reservation = await _reservationRepository.GetByReservationIdAsync(reservationId);
        if (reservation == null)
        {
            return new StockReservationResultDto
            {
                Success = false,
                Message = "Reservation not found"
            };
        }

        if (reservation.ReleasedAt != null)
        {
            return new StockReservationResultDto
            {
                Success = false,
                Message = "Reservation already released"
            };
        }

        var released = await _reservationRepository.ReleaseReservationAsync(reservationId);
        
        return new StockReservationResultDto
        {
            Success = released,
            Message = released ? "Reservation released successfully" : "Failed to release reservation",
            ReservationId = reservationId
        };
    }

    public async Task<bool> ExtendReservationAsync(string reservationId, int additionalMinutes)
    {
        var reservation = await _reservationRepository.GetByReservationIdAsync(reservationId);
        if (reservation == null || reservation.ReleasedAt != null)
            return false;

        var newExpiryTime = reservation.ExpiresAt.AddMinutes(additionalMinutes);
        return await _reservationRepository.ExtendReservationAsync(reservationId, newExpiryTime);
    }

    public async Task<StockReservationDto?> GetReservationAsync(string reservationId)
    {
        var reservation = await _reservationRepository.GetByReservationIdAsync(reservationId);
        return reservation == null ? null : MapToDto(reservation);
    }

    public async Task<StockAvailabilityDto> GetStockAvailabilityAsync(Guid productId, Guid warehouseId)
    {
        var availableQuantity = await GetAvailableQuantityAsync(productId, warehouseId);
        var reservedQuantity = await _reservationRepository.GetTotalReservedQuantityAsync(productId, warehouseId);
        var totalStock = availableQuantity + reservedQuantity;

        return new StockAvailabilityDto
        {
            ProductId = productId,
            WarehouseId = warehouseId,
            AvailableQuantity = availableQuantity,
            ReservedQuantity = reservedQuantity,
            TotalStock = totalStock,
            CanReserve = availableQuantity > 0,
            MaxReservableQuantity = availableQuantity
        };
    }

    public async Task<bool> CanReserveQuantityAsync(Guid productId, Guid warehouseId, int quantity)
    {
        return await _reservationRepository.CanReserveQuantityAsync(productId, warehouseId, quantity);
    }

    public async Task<int> GetAvailableQuantityAsync(Guid productId, Guid warehouseId)
    {
        var inventoryItem = await _inventoryItemRepository.GetByProductAndWarehouseAsync(productId, warehouseId);
        if (inventoryItem == null)
            return 0;

        var reservedQuantity = await _reservationRepository.GetTotalReservedQuantityAsync(productId, warehouseId);
        return Math.Max(0, inventoryItem.AvailableQuantity - reservedQuantity);
    }

    public async Task<IEnumerable<StockAvailabilityDto>> GetMultiWarehouseAvailabilityAsync(Guid productId)
    {
        var warehouses = await _warehouseRepository.GetActiveWarehousesAsync();
        var availabilities = new List<StockAvailabilityDto>();

        foreach (var warehouse in warehouses)
        {
            var availability = await GetStockAvailabilityAsync(productId, warehouse.Id);
            availabilities.Add(availability);
        }

        return availabilities.OrderByDescending(a => a.AvailableQuantity);
    }

    public async Task<StockReservationResultDto> ReserveFromBestWarehouseAsync(Guid productId, int quantity, string? reason = null)
    {
        var availabilities = await GetMultiWarehouseAvailabilityAsync(productId);
        var bestWarehouse = availabilities.FirstOrDefault(a => a.AvailableQuantity >= quantity);

        if (bestWarehouse == null)
        {
            return new StockReservationResultDto
            {
                Success = false,
                Message = "Insufficient stock available across all warehouses"
            };
        }

        var createDto = new CreateStockReservationDto
        {
            ProductId = productId,
            WarehouseId = bestWarehouse.WarehouseId,
            Quantity = quantity,
            Reason = reason ?? "Auto-selected best warehouse"
        };

        return await ReserveStockAsync(createDto);
    }

    public async Task<int> CleanupExpiredReservationsAsync()
    {
        var expiredReservations = await _reservationRepository.GetExpiredReservationsAsync();
        var count = expiredReservations.Count();

        await _reservationRepository.CleanupExpiredReservationsAsync();

        return count;
    }

    public async Task<IEnumerable<StockReservationDto>> GetExpiredReservationsAsync()
    {
        var expiredReservations = await _reservationRepository.GetExpiredReservationsAsync();
        return expiredReservations.Select(MapToDto);
    }

    public async Task<IEnumerable<StockReservationDto>> GetActiveReservationsAsync(Guid? productId = null)
    {
        IEnumerable<StockReservation> reservations;

        if (productId.HasValue)
        {
            reservations = await _reservationRepository.GetReservationsByProductAsync(productId.Value);
        }
        else
        {
            // Get all active reservations - would need a new repository method
            reservations = new List<StockReservation>(); // Placeholder
        }

        return reservations.Select(MapToDto);
    }

    private static string GenerateReservationId()
    {
        return $"RSV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }

    private static StockReservationDto MapToDto(StockReservation reservation)
    {
        return new StockReservationDto
        {
            Id = reservation.Id,
            ProductId = reservation.ProductId,
            WarehouseId = reservation.WarehouseId,
            ReservationId = reservation.ReservationId,
            Quantity = reservation.Quantity,
            CreatedAt = reservation.CreatedAt,
            ExpiresAt = reservation.ExpiresAt,
            ReleasedAt = reservation.ReleasedAt,
            Reason = reservation.Reason
        };
    }
}
