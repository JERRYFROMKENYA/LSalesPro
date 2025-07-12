using SalesService.Application.DTOs;

namespace SalesService.Application.Interfaces;

public interface IAuthServiceClient
{
    // User validation
    Task<UserValidationResultDto> ValidateTokenAsync(string token);
    Task<UserDetailsDto?> GetUserByIdAsync(string userId);
    Task<List<string>> GetUserPermissionsAsync(string userId);
}
