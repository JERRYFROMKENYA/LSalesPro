using InventoryService.Application.DTOs;

namespace InventoryService.Application.Interfaces;

public interface IProductService
{
    // Basic CRUD operations
    Task<IEnumerable<ProductDto>> GetAllAsync();
    Task<ProductDto?> GetByIdAsync(Guid id);
    Task<ProductDto?> GetBySkuAsync(string sku);
    Task<ProductDto> CreateAsync(CreateProductDto dto);
    Task<ProductDto?> UpdateAsync(Guid id, UpdateProductDto dto);
    Task<bool> DeleteAsync(Guid id);

    // Search and filtering
    Task<IEnumerable<ProductDto>> SearchAsync(string searchTerm);
    Task<ProductPagedResultDto> SearchPagedAsync(ProductSearchDto searchDto);
    Task<IEnumerable<ProductDto>> GetByCategoryAsync(string category);
    Task<IEnumerable<ProductDto>> GetActiveProductsAsync();

    // Category management
    Task<IEnumerable<string>> GetCategoriesAsync();

    // Validation
    Task<bool> ValidateSkuAsync(string sku, Guid? excludeId = null);
    Task<bool> ExistsAsync(Guid id);

    // Business operations
    Task<bool> ActivateProductAsync(Guid id);
    Task<bool> DeactivateProductAsync(Guid id);
    Task<IEnumerable<ProductDto>> GetLowStockProductsAsync();
}
