using System.ComponentModel.DataAnnotations;

namespace InventoryService.Application.DTOs;

public class LowStockAlertDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int ReorderLevel { get; set; }
    public int StockShortage => Math.Max(0, ReorderLevel - CurrentStock);
    public AlertSeverity Severity { get; set; }
    public DateTime AlertGeneratedAt { get; set; } = DateTime.UtcNow;
}

public class ReorderSuggestionDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int ReorderLevel { get; set; }
    public int SuggestedOrderQuantity { get; set; }
    public decimal EstimatedCost { get; set; }
    public DateTime LastOrderDate { get; set; }
    public int DaysSinceLastOrder => (DateTime.UtcNow - LastOrderDate).Days;
}

public class StockLevelReportDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int TotalAvailableStock { get; set; }
    public int TotalReservedStock { get; set; }
    public int TotalStock => TotalAvailableStock + TotalReservedStock;
    public int ReorderLevel { get; set; }
    public bool IsLowStock => TotalAvailableStock <= ReorderLevel;
    public bool IsCriticalStock => TotalAvailableStock <= 5;
    public IEnumerable<WarehouseStockBreakdownDto> WarehouseBreakdown { get; set; } = new List<WarehouseStockBreakdownDto>();
}

public class WarehouseStockBreakdownDto
{
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public int AvailableQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int TotalQuantity => AvailableQuantity + ReservedQuantity;
    public DateTime LastUpdated { get; set; }
}

public class UpdateReorderLevelDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Reorder level cannot be negative")]
    public int NewReorderLevel { get; set; }

    [StringLength(200)]
    public string? Reason { get; set; }
}

public enum AlertSeverity
{
    Low = 1,        // Stock below reorder level but above critical
    Medium = 2,     // Stock significantly below reorder level
    High = 3,       // Stock critically low (less than 5 units)
    Critical = 4    // Stock at zero or near zero
}
