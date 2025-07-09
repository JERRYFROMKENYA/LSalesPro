using InventoryService.Domain.Entities;

namespace InventoryService.Domain.Interfaces;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(Guid id);
    Task<Product?> GetBySkuAsync(string sku);
    Task<IEnumerable<Product>> GetByCategoryAsync(string category);
    Task<IEnumerable<Product>> SearchAsync(string searchTerm);
    Task<IEnumerable<Product>> GetActiveProductsAsync();
    Task<Product> CreateAsync(Product product);
    Task<Product?> UpdateAsync(Product product);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> SkuExistsAsync(string sku, Guid? excludeId = null);
    Task<IEnumerable<string>> GetCategoriesAsync();
    Task<int> GetTotalCountAsync();
    Task<IEnumerable<Product>> GetPagedAsync(int page, int pageSize);
    Task<IEnumerable<Product>> GetByIdsAsync(List<Guid> ids);
}
