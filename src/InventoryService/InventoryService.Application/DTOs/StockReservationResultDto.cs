namespace InventoryService.Application.DTOs;

public class StockReservationResultDto
{
    public bool Success { get; set; }
    public string? ReservationId { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? ProductId { get; set; }
    public int? ReservedQuantity { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public StockReservationDto? Reservation { get; set; }
    public List<string> Errors { get; set; } = new();
}
