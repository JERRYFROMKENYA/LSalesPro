using System.ComponentModel.DataAnnotations;

namespace InventoryService.Application.DTOs;

public class StockTransferResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? TransferId { get; set; }
    public StockTransferDto? Transfer { get; set; }
}
