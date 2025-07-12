namespace SalesService.Application.DTOs;

public class ProductAvailabilityDto
{
    public string ProductId { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public int AvailableQuantity { get; set; }
    public string? WarehouseId { get; set; }
    public string? Message { get; set; }
}

