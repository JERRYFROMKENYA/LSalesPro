using AutoMapper;
using Microsoft.Extensions.Logging;
using SalesService.Application.DTOs;
using SalesService.Application.Interfaces;
using SalesService.Domain.Entities;

namespace SalesService.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(ICustomerRepository customerRepository, IMapper mapper, ILogger<CustomerService> logger)
    {
        _customerRepository = customerRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<CustomerDto>> GetAllAsync()
    {
        var customers = await _customerRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<CustomerDto>>(customers);
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        return customer == null ? null : _mapper.Map<CustomerDto>(customer);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
    {
        // Validate email uniqueness if provided
        if (!string.IsNullOrEmpty(dto.Email) && await _customerRepository.EmailExistsAsync(dto.Email))
        {
            throw new InvalidOperationException("Email address is already in use");
        }

        // Validate tax ID uniqueness if provided
        if (!string.IsNullOrEmpty(dto.TaxId) && await _customerRepository.TaxIdExistsAsync(dto.TaxId))
        {
            throw new InvalidOperationException("Tax ID is already in use");
        }

        var customer = _mapper.Map<Customer>(dto);
        customer = await _customerRepository.AddAsync(customer);
        
        _logger.LogInformation("Created new customer with ID: {CustomerId}", customer.Id);
        
        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task<CustomerDto?> UpdateAsync(Guid id, UpdateCustomerDto dto)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
        {
            return null;
        }

        // Validate email uniqueness if changing email
        if (!string.IsNullOrEmpty(dto.Email) && dto.Email != customer.Email && await _customerRepository.EmailExistsAsync(dto.Email, id))
        {
            throw new InvalidOperationException("Email address is already in use");
        }

        // Validate tax ID uniqueness if changing tax ID
        if (!string.IsNullOrEmpty(dto.TaxId) && dto.TaxId != customer.TaxId && await _customerRepository.TaxIdExistsAsync(dto.TaxId, id))
        {
            throw new InvalidOperationException("Tax ID is already in use");
        }

        _mapper.Map(dto, customer);
        customer.UpdatedAt = DateTime.UtcNow;
        
        await _customerRepository.UpdateAsync(customer);
        
        _logger.LogInformation("Updated customer with ID: {CustomerId}", customer.Id);
        
        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
        {
            return false;
        }

        // Check if customer has orders
        if (customer.Orders.Any())
        {
            // Soft delete - just mark as inactive
            customer.IsActive = false;
            customer.UpdatedAt = DateTime.UtcNow;
            await _customerRepository.UpdateAsync(customer);
            
            _logger.LogInformation("Customer with ID: {CustomerId} marked as inactive (soft delete) due to existing orders", id);
        }
        else
        {
            // Hard delete if no orders
            await _customerRepository.DeleteAsync(id);
            
            _logger.LogInformation("Customer with ID: {CustomerId} permanently deleted", id);
        }

        return true;
    }

    public async Task<IEnumerable<CustomerDto>> SearchAsync(string searchTerm)
    {
        var customers = await _customerRepository.SearchAsync(searchTerm);
        return _mapper.Map<IEnumerable<CustomerDto>>(customers);
    }

    public async Task<CustomerPagedResultDto> SearchPagedAsync(CustomerSearchDto searchDto)
    {
        // This would need a more complex repository method, for now we'll simulate with in-memory filtering
        var allCustomers = await _customerRepository.GetAllAsync();
        
        // Apply filters
        var filteredCustomers = allCustomers.AsQueryable();
        
        if (!string.IsNullOrEmpty(searchDto.SearchTerm))
        {
            filteredCustomers = filteredCustomers.Where(c => 
                c.Name.Contains(searchDto.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                c.ContactPerson.Contains(searchDto.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                (c.Email != null && c.Email.Contains(searchDto.SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (c.Phone != null && c.Phone.Contains(searchDto.SearchTerm, StringComparison.OrdinalIgnoreCase)));
        }
        
        if (!string.IsNullOrEmpty(searchDto.Type))
        {
            filteredCustomers = filteredCustomers.Where(c => c.CustomerType == searchDto.Type);
        }
        
        if (!string.IsNullOrEmpty(searchDto.Category))
        {
            filteredCustomers = filteredCustomers.Where(c => c.CustomerCategory == searchDto.Category);
        }
        
        if (searchDto.IsActive.HasValue)
        {
            filteredCustomers = filteredCustomers.Where(c => c.IsActive == searchDto.IsActive.Value);
        }
        
        // Get total count before pagination
        var totalCount = filteredCustomers.Count();
        
        // Apply sorting
        if (searchDto.SortAscending)
        {
            filteredCustomers = searchDto.SortBy.ToLower() switch
            {
                "name" => filteredCustomers.OrderBy(c => c.Name),
                "type" => filteredCustomers.OrderBy(c => c.CustomerType),
                "category" => filteredCustomers.OrderBy(c => c.CustomerCategory),
                "createdat" => filteredCustomers.OrderBy(c => c.CreatedAt),
                _ => filteredCustomers.OrderBy(c => c.Name)
            };
        }
        else
        {
            filteredCustomers = searchDto.SortBy.ToLower() switch
            {
                "name" => filteredCustomers.OrderByDescending(c => c.Name),
                "type" => filteredCustomers.OrderByDescending(c => c.CustomerType),
                "category" => filteredCustomers.OrderByDescending(c => c.CustomerCategory),
                "createdat" => filteredCustomers.OrderByDescending(c => c.CreatedAt),
                _ => filteredCustomers.OrderByDescending(c => c.Name)
            };
        }
        
        // Apply pagination
        var pagedCustomers = filteredCustomers
            .Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
            .Take(searchDto.PageSize)
            .ToList();
        
        // Calculate total pages
        var totalPages = (int)Math.Ceiling(totalCount / (double)searchDto.PageSize);
        
        // Map to DTOs
        var customerDtos = _mapper.Map<IEnumerable<CustomerDto>>(pagedCustomers);
        
        // Create result
        var result = new CustomerPagedResultDto
        {
            Customers = customerDtos,
            TotalCount = totalCount,
            PageNumber = searchDto.PageNumber,
            PageSize = searchDto.PageSize,
            TotalPages = totalPages,
            HasPrevious = searchDto.PageNumber > 1,
            HasNext = searchDto.PageNumber < totalPages
        };
        
        return result;
    }

    public async Task<IEnumerable<CustomerDto>> GetByTypeAsync(string type)
    {
        var allCustomers = await _customerRepository.GetAllAsync();
        var filteredCustomers = allCustomers.Where(c => c.CustomerType == type);
        return _mapper.Map<IEnumerable<CustomerDto>>(filteredCustomers);
    }

    public async Task<IEnumerable<CustomerDto>> GetByCategoryAsync(string category)
    {
        var allCustomers = await _customerRepository.GetAllAsync();
        var filteredCustomers = allCustomers.Where(c => c.CustomerCategory == category);
        return _mapper.Map<IEnumerable<CustomerDto>>(filteredCustomers);
    }

    public async Task<IEnumerable<CustomerDto>> GetActiveCustomersAsync()
    {
        var allCustomers = await _customerRepository.GetAllAsync();
        var activeCustomers = allCustomers.Where(c => c.IsActive);
        return _mapper.Map<IEnumerable<CustomerDto>>(activeCustomers);
    }

    public async Task<IEnumerable<string>> GetCustomerTypesAsync()
    {
        var allCustomers = await _customerRepository.GetAllAsync();
        return allCustomers.Select(c => c.CustomerType).Distinct().OrderBy(t => t).ToList();
    }

    public async Task<IEnumerable<string>> GetCustomerCategoriesAsync()
    {
        var allCustomers = await _customerRepository.GetAllAsync();
        return allCustomers.Select(c => c.CustomerCategory).Distinct().OrderBy(c => c).ToList();
    }

    public async Task<bool> ValidateEmailAsync(string email, Guid? excludeId = null)
    {
        return !await _customerRepository.EmailExistsAsync(email, excludeId);
    }

    public async Task<bool> ValidateTaxIdAsync(string taxId, Guid? excludeId = null)
    {
        return !await _customerRepository.TaxIdExistsAsync(taxId, excludeId);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _customerRepository.ExistsAsync(id);
    }

    public async Task<bool> UpdateCreditLimitAsync(Guid id, decimal newCreditLimit)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
        {
            return false;
        }

        customer.CreditLimit = newCreditLimit;
        customer.UpdatedAt = DateTime.UtcNow;
        
        await _customerRepository.UpdateAsync(customer);
        
        _logger.LogInformation("Updated credit limit for customer ID: {CustomerId} to {NewCreditLimit}", id, newCreditLimit);
        
        return true;
    }

    public async Task<bool> ActivateCustomerAsync(Guid id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
        {
            return false;
        }

        customer.IsActive = true;
        customer.UpdatedAt = DateTime.UtcNow;
        
        await _customerRepository.UpdateAsync(customer);
        
        _logger.LogInformation("Activated customer with ID: {CustomerId}", id);
        
        return true;
    }

    public async Task<bool> DeactivateCustomerAsync(Guid id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
        {
            return false;
        }

        customer.IsActive = false;
        customer.UpdatedAt = DateTime.UtcNow;
        
        await _customerRepository.UpdateAsync(customer);
        
        _logger.LogInformation("Deactivated customer with ID: {CustomerId}", id);
        
        return true;
    }

    // Stub implementations for new interface methods
    public async Task<IEnumerable<OrderDto>> GetOrderHistoryAsync(Guid customerId)
    {
        // TODO: Implement actual logic
        return new List<OrderDto>();
    }

    public async Task<CustomerCreditStatusDto> GetCreditStatusAsync(Guid customerId)
    {
        // TODO: Implement actual logic
        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer == null)
        {
            return null; // Or throw an exception
        }

        return new CustomerCreditStatusDto
        {
            CustomerId = customer.Id,
            CreditLimit = customer.CreditLimit,
            CurrentBalance = customer.CurrentBalance,
            AvailableCredit = customer.AvailableCredit,
            HasCreditLimit = customer.HasCreditLimit,
            IsCreditAvailable = customer.AvailableCredit > 0
        };
    }

    public async Task<IEnumerable<CustomerMapDataDto>> GetMapDataAsync()
    {
        var customers = await _customerRepository.GetAllAsync();
        return customers.Select(c => new CustomerMapDataDto
        {
            CustomerId = c.Id,
            CustomerName = c.Name,
            Latitude = c.Latitude ?? 0,
            Longitude = c.Longitude ?? 0,
            Address = c.Address ?? string.Empty
        });
    }
}
