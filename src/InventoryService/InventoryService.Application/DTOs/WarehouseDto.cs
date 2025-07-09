using System.ComponentModel.DataAnnotations;

namespace InventoryService.Application.DTOs;

public class WarehouseDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? ManagerEmail { get; set; }
    public string? Phone { get; set; }
    public int Capacity { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? AvailableCapacity { get; set; }
    public int? InventoryItemCount { get; set; }
}

public class CreateWarehouseDto
{
    [Required]
    [StringLength(10, MinimumLength = 2)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Type { get; set; } = "Regional"; // Main, Regional, Transit

    [Required]
    [StringLength(500, MinimumLength = 10)]
    public string Address { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(256)]
    public string? ManagerEmail { get; set; }

    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than 0")]
    public int Capacity { get; set; } = 1000;

    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
    public double? Latitude { get; set; }

    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
    public double? Longitude { get; set; }

    public bool IsActive { get; set; } = true;
}

public class UpdateWarehouseDto
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Type { get; set; } = "Regional";

    [Required]
    [StringLength(500, MinimumLength = 10)]
    public string Address { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(256)]
    public string? ManagerEmail { get; set; }

    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than 0")]
    public int Capacity { get; set; }

    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
    public double? Latitude { get; set; }

    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
    public double? Longitude { get; set; }

    public bool IsActive { get; set; }
}

public class WarehouseSearchDto
{
    public string? SearchTerm { get; set; }
    public string? Type { get; set; }
    public int? MinCapacity { get; set; }
    public int? MaxCapacity { get; set; }
    public bool? IsActive { get; set; } = true;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? RadiusKm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class WarehousePagedResultDto
{
    public IEnumerable<WarehouseDto> Warehouses { get; set; } = new List<WarehouseDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

public class WarehouseCapacityDto
{
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public int TotalCapacity { get; set; }
    public int UsedCapacity { get; set; }
    public int AvailableCapacity { get; set; }
    public double UtilizationPercentage => TotalCapacity > 0 ? (double)UsedCapacity / TotalCapacity * 100 : 0;
}
