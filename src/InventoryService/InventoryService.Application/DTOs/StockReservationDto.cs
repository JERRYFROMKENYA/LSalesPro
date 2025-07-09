using System.ComponentModel.DataAnnotations;

namespace InventoryService.Application.DTOs;

public class StockReservationDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public string ReservationId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? ReleasedAt { get; set; }
    public string? Reason { get; set; }
    public bool IsActive => ReleasedAt == null && ExpiresAt > DateTime.UtcNow;
    public bool IsExpired => ReleasedAt == null && ExpiresAt <= DateTime.UtcNow;
}

public class CreateStockReservationDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public Guid WarehouseId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }

    [StringLength(100)]
    public string? CustomerReference { get; set; }

    [Range(1, 1440, ErrorMessage = "Reservation duration must be between 1 and 1440 minutes")]
    public int ReservationDurationMinutes { get; set; } = 30;

    [StringLength(200)]
    public string? Reason { get; set; }
}
