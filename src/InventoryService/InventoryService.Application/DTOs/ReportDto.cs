namespace InventoryService.Application.DTOs;

public class InventorySummaryReportDto
{
    public int TotalProducts { get; set; }
    public int TotalWarehouses { get; set; }
    public int LowStockProducts { get; set; }
    public int CriticalStockProducts { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public class WarehouseUtilizationDto
{
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public int TotalCapacity { get; set; }
    public int UsedCapacity { get; set; }
    public decimal UtilizationPercentage { get; set; }
    public int ProductCount { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class WarehouseCapacityReportDto
{
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public int TotalCapacity { get; set; }
    public decimal CurrentUtilization { get; set; }
    public int AvailableCapacity { get; set; }
    public bool IsActive { get; set; }
}

public class ProductPerformanceReportDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int TotalStock { get; set; }
    public int ReorderLevel { get; set; }
    public bool IsLowStock { get; set; }
    public decimal Price { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class StockMovementReportDto
{
    public Guid MovementId { get; set; }
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}