namespace InventoryService.Application.DTOs;

public class MultiWarehouseStockAvailabilityDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public int RequiredQuantity { get; set; }
    public int TotalAvailableQuantity { get; set; }
    public bool IsAvailable { get; set; }
    public List<WarehouseStockDto> WarehouseStocks { get; set; } = new();
}

public class StockAvailabilityDto
{
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public int AvailableQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int TotalStock { get; set; }
    public bool CanReserve { get; set; }
    public int MaxReservableQuantity { get; set; }
}

public class WarehouseStockDto
{
    public Guid WarehouseId { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public int AvailableQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? DistanceKm { get; set; }
}

public class StockAllocationDto
{
    public Guid WarehouseId { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public int AllocatedQuantity { get; set; }
    public double? DistanceKm { get; set; }
    public int Priority { get; set; }
    public string AllocationReason { get; set; } = string.Empty;
}

public class StockDistributionDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public int TotalStock { get; set; }
    public List<WarehouseStockDto> WarehouseDistribution { get; set; } = new();
    public decimal DistributionVariance { get; set; }
    public bool IsWellDistributed { get; set; }
}

public class WarehouseStockSummaryDto
{
    public Guid WarehouseId { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public int TotalProducts { get; set; }
    public int TotalStock { get; set; }
    public int TotalReserved { get; set; }
    public int TotalAvailable { get; set; }
    public decimal CapacityUtilization { get; set; }
    public List<ProductStockDto> ProductStocks { get; set; } = new();
}

public class ProductStockDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public int AvailableQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public bool IsLowStock { get; set; }
}

public class StockRebalanceRecommendationDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public Guid FromWarehouseId { get; set; }
    public string FromWarehouseName { get; set; } = string.Empty;
    public Guid ToWarehouseId { get; set; }
    public string ToWarehouseName { get; set; } = string.Empty;
    public int RecommendedQuantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public decimal PotentialCostSaving { get; set; }
    public int Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsExecuted { get; set; }
}
