using Microsoft.EntityFrameworkCore;
using SalesService.Application.Interfaces;
using SalesService.Domain.Entities;
using SalesService.Infrastructure.Data;

namespace SalesService.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly SalesDbContext _context;

    public CustomerRepository(SalesDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        return await _context.Customers
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Customer?> GetByIdAsync(Guid id)
    {
        return await _context.Customers
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Customer?> GetByEmailAsync(string email)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Email == email);
    }

    public async Task<Customer?> GetByTaxIdAsync(string taxId)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.TaxId == taxId);
    }

    public async Task<IEnumerable<Customer>> SearchAsync(string searchTerm)
    {
        return await _context.Customers
            .Where(c => c.Name.Contains(searchTerm) || 
                        c.ContactPerson.Contains(searchTerm) || 
                        (c.Email != null && c.Email.Contains(searchTerm)) ||
                        (c.Phone != null && c.Phone.Contains(searchTerm)) ||
                        (c.TaxId != null && c.TaxId.Contains(searchTerm)))
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Customer> AddAsync(Customer customer)
    {
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task UpdateAsync(Customer customer)
    {
        customer.UpdatedAt = DateTime.UtcNow;
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer != null)
        {
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Customers.AnyAsync(c => c.Id == id);
    }

    public async Task<bool> EmailExistsAsync(string email, Guid? excludeId = null)
    {
        if (excludeId.HasValue)
        {
            return await _context.Customers.AnyAsync(c => c.Email == email && c.Id != excludeId);
        }
        return await _context.Customers.AnyAsync(c => c.Email == email);
    }

    public async Task<bool> TaxIdExistsAsync(string taxId, Guid? excludeId = null)
    {
        if (excludeId.HasValue)
        {
            return await _context.Customers.AnyAsync(c => c.TaxId == taxId && c.Id != excludeId);
        }
        return await _context.Customers.AnyAsync(c => c.TaxId == taxId);
    }
}
