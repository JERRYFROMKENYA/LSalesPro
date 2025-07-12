using AuthService.Application.DTOs;

namespace AuthService.Application.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(Guid id);
    Task<UserDto?> GetUserByUsernameAsync(string username);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<IEnumerable<UserDto>> GetActiveUsersAsync();
    Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
    Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto);
    Task DeleteUserAsync(Guid id);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto);
    Task<bool> ValidatePasswordAsync(string username, string password);
    Task AssignRolesToUserAsync(Guid userId, List<Guid> roleIds);
    Task RemoveRolesFromUserAsync(Guid userId, List<Guid> roleIds);
    Task RegisterAsync(RegisterDto registerDto);
    Task<List<string>> GetUserPermissionsAsync(string userId);
}
