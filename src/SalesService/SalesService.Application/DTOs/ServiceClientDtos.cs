namespace SalesService.Application.DTOs;

public class ProductAvailabilityRequestDto
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? WarehouseId { get; set; }
}

public class ProductAvailabilityResultDto
{
    public string ProductId { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public int AvailableQuantity { get; set; }
    public List<WarehouseStockDto> WarehouseStocks { get; set; } = new List<WarehouseStockDto>();
    public string? ErrorMessage { get; set; }
}

public class WarehouseStockDto
{
    public string WarehouseId { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public int AvailableQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int TotalQuantity { get; set; }
}

public class StockReservationRequestDto
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? WarehouseId { get; set; } = string.Empty;
    public string? ReservationId { get; set; } // For idempotent operations
    public int TimeoutMinutes { get; set; } = 30; // Default 30 minutes
    public int ReservationDurationMinutes { get; set; } = 60; // Default 60 minutes
}

public class StockReservationResultDto
{
    public bool Success { get; set; }
    public string? ReservationId { get; set; }
    public int ReservedQuantity { get; set; }
    public DateTime? ExpiryTime { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Message { get; set; }
}

public class ProductDetailsDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<WarehouseStockDto> StockLevels { get; set; } = new List<WarehouseStockDto>();
}

public class UserValidationResultDto
{
    public bool IsValid { get; set; }
    public string? UserId { get; set; }
    public string? Username { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
    public List<string> Permissions { get; set; } = new List<string>();
    public DateTime? ExpiresAt { get; set; }
}

public class UserDetailsDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
}
