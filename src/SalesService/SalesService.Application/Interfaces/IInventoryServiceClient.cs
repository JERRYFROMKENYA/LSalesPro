
using SalesService.Application.DTOs;

namespace SalesService.Application.Interfaces;

public interface IInventoryServiceClient
{
    Task<ProductAvailabilityDto> CheckProductAvailabilityAsync(string productId, int quantity, string? warehouseId = null);
    Task<IEnumerable<ProductAvailabilityDto>> CheckProductsAvailabilityAsync(IEnumerable<ProductAvailabilityRequestDto> requests);
    Task<StockReservationResultDto> ReserveStockAsync(StockReservationRequestDto request);
    Task<bool> ReleaseStockReservationAsync(string reservationId);
    Task<ProductPricingDto?> GetProductPricingInformationAsync(string productId);
}
