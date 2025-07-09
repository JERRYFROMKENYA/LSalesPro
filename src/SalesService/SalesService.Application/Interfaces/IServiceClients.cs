using SalesService.Application.DTOs;

namespace SalesService.Application.Interfaces;

public interface IInventoryServiceClient
{
    // Product availability checks
    Task<ProductAvailabilityResultDto> CheckProductAvailabilityAsync(string productId, int quantity, string? warehouseId = null);
    Task<List<ProductAvailabilityResultDto>> CheckProductsAvailabilityAsync(List<ProductAvailabilityRequestDto> requests);
    
    // Stock reservation
    Task<StockReservationResultDto> ReserveStockAsync(StockReservationRequestDto request);
    Task<bool> ReleaseStockReservationAsync(string reservationId);
    
    // Product information
    Task<ProductDetailsDto?> GetProductDetailsAsync(string productId);
    Task<List<ProductDetailsDto>> GetProductsBatchAsync(List<string> productIds);
}

public interface IAuthServiceClient
{
    // User validation
    Task<UserValidationResultDto> ValidateTokenAsync(string token);
    Task<UserDetailsDto?> GetUserByIdAsync(string userId);
    Task<List<string>> GetUserPermissionsAsync(string userId);
}
