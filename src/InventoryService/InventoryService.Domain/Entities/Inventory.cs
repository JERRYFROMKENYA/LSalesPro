using System.ComponentModel.DataAnnotations;

namespace InventoryService.Domain.Entities;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(50)]
    public string Sku { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? Subcategory { get; set; }
    
    public decimal Price { get; set; }
    
    public decimal TaxRate { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string Unit { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? Packaging { get; set; }
    
    public int MinOrderQuantity { get; set; } = 1;
    
    public int ReorderLevel { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
    
    public virtual ICollection<StockReservation> StockReservations { get; set; } = new List<StockReservation>();
}

public class Warehouse
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(10)]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string Type { get; set; } = string.Empty; // Main, Regional, Transit
    
    [Required]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;
    
    [EmailAddress]
    [MaxLength(256)]
    public string? ManagerEmail { get; set; }
    
    [MaxLength(20)]
    public string? Phone { get; set; }
    
    public int Capacity { get; set; }
    
    public double? Latitude { get; set; }
    
    public double? Longitude { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
    
    public virtual ICollection<StockReservation> StockReservations { get; set; } = new List<StockReservation>();
}

public class InventoryItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid ProductId { get; set; }
    
    public Guid WarehouseId { get; set; }
    
    public int AvailableQuantity { get; set; }
    
    public int ReservedQuantity { get; set; }
    
    public int TotalQuantity => AvailableQuantity + ReservedQuantity;
    
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    public string? LastUpdatedBy { get; set; }
    
    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    
    public virtual Warehouse Warehouse { get; set; } = null!;
}

public class StockReservation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid ProductId { get; set; }
    
    public Guid WarehouseId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string ReservationId { get; set; } = string.Empty; // External reference
    
    public int Quantity { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime ExpiresAt { get; set; }
    
    public DateTime? ReleasedAt { get; set; }
    
    public bool IsReleased => ReleasedAt.HasValue;
    
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    
    public bool IsActive => !IsReleased && !IsExpired;
    
    [MaxLength(200)]
    public string? Reason { get; set; }
    
    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    
    public virtual Warehouse Warehouse { get; set; } = null!;
}

public class StockMovement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid ProductId { get; set; }
    
    public Guid WarehouseId { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string MovementType { get; set; } = string.Empty; // IN, OUT, TRANSFER, ADJUSTMENT
    
    public int Quantity { get; set; }
    
    public int PreviousQuantity { get; set; }
    
    public int NewQuantity { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Reason { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Notes { get; set; }
    
    [MaxLength(50)]
    public string? ReferenceNumber { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [MaxLength(100)]
    public string? CreatedBy { get; set; }
    
    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    
    public virtual Warehouse Warehouse { get; set; } = null!;
}
