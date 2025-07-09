using System.ComponentModel.DataAnnotations;

namespace InventoryService.Application.DTOs;

public class StockTransferDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public Guid FromWarehouseId { get; set; }
    public string FromWarehouseName { get; set; } = string.Empty;
    public string FromWarehouseCode { get; set; } = string.Empty;
    public Guid ToWarehouseId { get; set; }
    public string ToWarehouseName { get; set; } = string.Empty;
    public string ToWarehouseCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime TransferDate { get; set; }
    public string Status { get; set; } = string.Empty; // Pending, InTransit, Completed, Cancelled
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ProcessedBy { get; set; }
}

public class CreateStockTransferDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public Guid FromWarehouseId { get; set; }

    [Required]
    public Guid ToWarehouseId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public int Quantity { get; set; }

    [Required]
    [StringLength(50)]
    public string Reason { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Notes { get; set; }
}
