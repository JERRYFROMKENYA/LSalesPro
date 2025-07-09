using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using InventoryService.Domain.Interfaces;

namespace InventoryService.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return products.Select(MapToDto);
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        return product == null ? null : MapToDto(product);
    }

    public async Task<ProductDto?> GetBySkuAsync(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return null;

        var product = await _productRepository.GetBySkuAsync(sku);
        return product == null ? null : MapToDto(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        // Validate SKU uniqueness
        if (await _productRepository.SkuExistsAsync(dto.Sku))
        {
            throw new InvalidOperationException($"Product with SKU '{dto.Sku}' already exists.");
        }

        var product = MapToEntity(dto);
        var createdProduct = await _productRepository.CreateAsync(product);
        return MapToDto(createdProduct);
    }

    public async Task<ProductDto?> UpdateAsync(Guid id, UpdateProductDto dto)
    {
        var existingProduct = await _productRepository.GetByIdAsync(id);
        if (existingProduct == null)
            return null;

        // Update the entity with new values
        existingProduct.Name = dto.Name;
        existingProduct.Description = dto.Description;
        existingProduct.Category = dto.Category;
        existingProduct.Subcategory = dto.Subcategory;
        existingProduct.Price = dto.Price;
        existingProduct.TaxRate = dto.TaxRate;
        existingProduct.Unit = dto.Unit;
        existingProduct.Packaging = dto.Packaging;
        existingProduct.MinOrderQuantity = dto.MinOrderQuantity;
        existingProduct.ReorderLevel = dto.ReorderLevel;
        existingProduct.IsActive = dto.IsActive;

        var updatedProduct = await _productRepository.UpdateAsync(existingProduct);
        return updatedProduct == null ? null : MapToDto(updatedProduct);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _productRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<ProductDto>> SearchAsync(string searchTerm)
    {
        var products = await _productRepository.SearchAsync(searchTerm);
        return products.Select(MapToDto);
    }

    public async Task<ProductPagedResultDto> SearchPagedAsync(ProductSearchDto searchDto)
    {
        IEnumerable<Product> products;

        if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm))
        {
            products = await _productRepository.SearchAsync(searchDto.SearchTerm);
        }
        else if (!string.IsNullOrWhiteSpace(searchDto.Category))
        {
            products = await _productRepository.GetByCategoryAsync(searchDto.Category);
        }
        else
        {
            products = await _productRepository.GetActiveProductsAsync();
        }

        // Apply additional filters
        if (searchDto.MinPrice.HasValue)
            products = products.Where(p => p.Price >= searchDto.MinPrice.Value);

        if (searchDto.MaxPrice.HasValue)
            products = products.Where(p => p.Price <= searchDto.MaxPrice.Value);

        if (!string.IsNullOrWhiteSpace(searchDto.Subcategory))
            products = products.Where(p => p.Subcategory == searchDto.Subcategory);

        var totalCount = products.Count();
        var pagedProducts = products
            .Skip((searchDto.Page - 1) * searchDto.PageSize)
            .Take(searchDto.PageSize)
            .Select(MapToDto);

        return new ProductPagedResultDto
        {
            Products = pagedProducts,
            TotalCount = totalCount,
            Page = searchDto.Page,
            PageSize = searchDto.PageSize
        };
    }

    public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(string category)
    {
        var products = await _productRepository.GetByCategoryAsync(category);
        return products.Select(MapToDto);
    }

    public async Task<IEnumerable<ProductDto>> GetActiveProductsAsync()
    {
        var products = await _productRepository.GetActiveProductsAsync();
        return products.Select(MapToDto);
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        return await _productRepository.GetCategoriesAsync();
    }

    public async Task<bool> ValidateSkuAsync(string sku, Guid? excludeId = null)
    {
        return !await _productRepository.SkuExistsAsync(sku, excludeId);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _productRepository.ExistsAsync(id);
    }

    public async Task<bool> ActivateProductAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return false;

        product.IsActive = true;
        product.UpdatedAt = DateTime.UtcNow;
        
        await _productRepository.UpdateAsync(product);
        return true;
    }

    public async Task<bool> DeactivateProductAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return false;

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        
        await _productRepository.UpdateAsync(product);
        return true;
    }

    public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync()
    {
        // For now, return products that need attention (placeholder for inventory integration)
        var products = await _productRepository.GetActiveProductsAsync();
        return products.Where(p => p.ReorderLevel > 0).Select(MapToDto);
    }

    // Mapping methods
    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Sku = product.Sku,
            Name = product.Name,
            Description = product.Description,
            Category = product.Category,
            Subcategory = product.Subcategory,
            Price = product.Price,
            TaxRate = product.TaxRate,
            Unit = product.Unit,
            Packaging = product.Packaging,
            MinOrderQuantity = product.MinOrderQuantity,
            ReorderLevel = product.ReorderLevel,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }

    private static Product MapToEntity(CreateProductDto dto)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Sku = dto.Sku,
            Name = dto.Name,
            Description = dto.Description,
            Category = dto.Category,
            Subcategory = dto.Subcategory,
            Price = dto.Price,
            TaxRate = dto.TaxRate,
            Unit = dto.Unit,
            Packaging = dto.Packaging,
            MinOrderQuantity = dto.MinOrderQuantity,
            ReorderLevel = dto.ReorderLevel,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
