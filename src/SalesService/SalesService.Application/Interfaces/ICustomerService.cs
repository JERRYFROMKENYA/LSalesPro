using SalesService.Application.DTOs;

namespace SalesService.Application.Interfaces;

public interface ICustomerService
{
    // Basic CRUD operations
    Task<IEnumerable<CustomerDto>> GetAllAsync();
    Task<CustomerDto?> GetByIdAsync(Guid id);
    Task<CustomerDto> CreateAsync(CreateCustomerDto dto);
    Task<CustomerDto?> UpdateAsync(Guid id, UpdateCustomerDto dto);
    Task<bool> DeleteAsync(Guid id);

    // Search and filtering
    Task<IEnumerable<CustomerDto>> SearchAsync(string searchTerm);
    Task<CustomerPagedResultDto> SearchPagedAsync(CustomerSearchDto searchDto);
    Task<IEnumerable<CustomerDto>> GetByTypeAsync(string type);
    Task<IEnumerable<CustomerDto>> GetByCategoryAsync(string category);
    Task<IEnumerable<CustomerDto>> GetActiveCustomersAsync();

    // Customer type/category management
    Task<IEnumerable<string>> GetCustomerTypesAsync();
    Task<IEnumerable<string>> GetCustomerCategoriesAsync();

    // Validation
    Task<bool> ValidateEmailAsync(string email, Guid? excludeId = null);
    Task<bool> ValidateTaxIdAsync(string taxId, Guid? excludeId = null);
    Task<bool> ExistsAsync(Guid id);

    // Business operations
    Task<bool> UpdateCreditLimitAsync(Guid id, decimal newCreditLimit);
    Task<bool> ActivateCustomerAsync(Guid id);
    Task<bool> DeactivateCustomerAsync(Guid id);
}
