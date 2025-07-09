using System.ComponentModel.DataAnnotations;

namespace InventoryService.Application.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Subcategory { get; set; }
    public decimal Price { get; set; }
    public decimal TaxRate { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? Packaging { get; set; }
    public int MinOrderQuantity { get; set; }
    public int ReorderLevel { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateProductDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Sku { get; set; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Subcategory { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Range(0, 1, ErrorMessage = "Tax rate must be between 0 and 1")]
    public decimal TaxRate { get; set; } = 0;

    [Required]
    [StringLength(20)]
    public string Unit { get; set; } = "Unit";

    [StringLength(50)]
    public string? Packaging { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Minimum order quantity must be at least 1")]
    public int MinOrderQuantity { get; set; } = 1;

    [Range(0, int.MaxValue, ErrorMessage = "Reorder level cannot be negative")]
    public int ReorderLevel { get; set; } = 0;

    public bool IsActive { get; set; } = true;
}

public class UpdateProductDto
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Subcategory { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Range(0, 1, ErrorMessage = "Tax rate must be between 0 and 1")]
    public decimal TaxRate { get; set; }

    [Required]
    [StringLength(20)]
    public string Unit { get; set; } = "Unit";

    [StringLength(50)]
    public string? Packaging { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Minimum order quantity must be at least 1")]
    public int MinOrderQuantity { get; set; } = 1;

    [Range(0, int.MaxValue, ErrorMessage = "Reorder level cannot be negative")]
    public int ReorderLevel { get; set; } = 0;

    public bool IsActive { get; set; }
}

public class ProductSearchDto
{
    public string? SearchTerm { get; set; }
    public string? Category { get; set; }
    public string? Subcategory { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? IsActive { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ProductPagedResultDto
{
    public IEnumerable<ProductDto> Products { get; set; } = new List<ProductDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
