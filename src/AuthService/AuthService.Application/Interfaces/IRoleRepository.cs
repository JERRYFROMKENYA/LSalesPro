using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid id);
    Task<Role?> GetByNameAsync(string name);
    Task<IEnumerable<Role>> GetAllAsync();
    Task<Role> CreateAsync(Role role);
    Task<Role> UpdateAsync(Role role);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Role?> GetRoleWithPermissionsAsync(Guid id);
    Task<IEnumerable<Role>> GetActiveRolesAsync();
    Task AddAsync(Role role);
}
