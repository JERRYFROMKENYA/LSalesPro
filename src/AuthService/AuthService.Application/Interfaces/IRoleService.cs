using AuthService.Application.DTOs;

namespace AuthService.Application.Interfaces;

public interface IRoleService
{
    Task<RoleDto?> GetRoleByIdAsync(Guid id);
    Task<RoleDto?> GetRoleByNameAsync(string name);
    Task<IEnumerable<RoleDto>> GetAllRolesAsync();
    Task<IEnumerable<RoleDto>> GetActiveRolesAsync();
    Task<RoleDto> CreateRoleAsync(CreateRoleDto createRoleDto);
    Task<RoleDto> UpdateRoleAsync(Guid id, UpdateRoleDto updateRoleDto);
    Task DeleteRoleAsync(Guid id);
    Task AssignPermissionsToRoleAsync(Guid roleId, List<Guid> permissionIds);
    Task RemovePermissionsFromRoleAsync(Guid roleId, List<Guid> permissionIds);
}
