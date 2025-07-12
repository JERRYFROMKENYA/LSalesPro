using SalesService.Application.DTOs;

namespace SalesService.Application.Interfaces;

public interface IOrderService
{
    // Basic CRUD operations
    Task<IEnumerable<OrderDto>> GetAllAsync();
    Task<OrderDto?> GetByIdAsync(Guid id);
    Task<OrderDto?> GetByOrderNumberAsync(string orderNumber);
    Task<OrderResultDto> CreateAsync(CreateOrderDto dto);
    Task<OrderResultDto> UpdateAsync(Guid id, UpdateOrderDto dto);
    Task<bool> DeleteAsync(Guid id);

    // Search and filtering
    Task<IEnumerable<OrderDto>> GetByCustomerIdAsync(Guid customerId);
    Task<IEnumerable<OrderDto>> GetByStatusAsync(string status);
    Task<IEnumerable<OrderDto>> GetRecentOrdersAsync(int count);
    Task<OrderPagedResultDto> SearchAsync(OrderSearchDto searchDto);
    
    // Order management
    Task<OrderResultDto> ConfirmOrderAsync(Guid id);
    Task<OrderResultDto> ProcessOrderAsync(Guid id);
    Task<OrderResultDto> ShipOrderAsync(Guid id, string? trackingInfo = null);
    Task<OrderResultDto> DeliverOrderAsync(Guid id);
    Task<OrderResultDto> CancelOrderAsync(Guid id, string reason);
    
    // Order items management
    Task<OrderItemDto> AddItemToOrderAsync(Guid orderId, AddOrderItemDto dto);
    Task<OrderItemDto> UpdateOrderItemAsync(Guid orderItemId, UpdateOrderItemDto dto);
    Task<bool> RemoveItemFromOrderAsync(Guid orderItemId);
    
    // Order validation
    Task<bool> ValidateOrderAsync(Guid id);
    Task<bool> CheckInventoryAvailabilityAsync(Guid productId, int quantity, Guid? warehouseId = null);

    // New methods for API usage
    Task<OrderResultDto> UpdateStatusAsync(UpdateOrderStatusDto dto);
    Task<OrderTotalPreviewDto> CalculateTotalAsync(CalculateOrderTotalDto dto);
}
