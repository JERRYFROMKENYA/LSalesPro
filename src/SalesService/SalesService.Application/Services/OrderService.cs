using AutoMapper;
using Microsoft.Extensions.Logging;
using SalesService.Application.DTOs;
using SalesService.Application.Interfaces;
using SalesService.Domain.Entities;
using System.Collections.Concurrent;

namespace SalesService.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IInventoryServiceClient _inventoryServiceClient;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IOrderItemRepository orderItemRepository,
        ICustomerRepository customerRepository,
        IInventoryServiceClient inventoryServiceClient,
        IMapper mapper,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _customerRepository = customerRepository;
        _inventoryServiceClient = inventoryServiceClient;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<OrderDto>> GetAllAsync()
    {
        var orders = await _orderRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
            return null;
            
        return _mapper.Map<OrderDto>(order);
    }

    public async Task<OrderDto?> GetByOrderNumberAsync(string orderNumber)
    {
        var order = await _orderRepository.GetByOrderNumberAsync(orderNumber);
        if (order == null)
            return null;
            
        return _mapper.Map<OrderDto>(order);
    }

    public async Task<OrderResultDto> CreateAsync(CreateOrderDto dto)
    {
        // Validate customer exists
        var customer = await _customerRepository.GetByIdAsync(dto.CustomerId);
        if (customer == null)
        {
            return new OrderResultDto
            {
                Success = false,
                Message = "Customer not found",
                ErrorCode = "CUSTOMER_NOT_FOUND"
            };
        }

        // Check if customer is active
        if (!customer.IsActive)
        {
            return new OrderResultDto
            {
                Success = false,
                Message = "Customer account is inactive",
                ErrorCode = "CUSTOMER_INACTIVE"
            };
        }

        // Check credit limit if applicable
        if (customer.CreditLimit > 0 && 
            dto.TotalAmount > customer.AvailableCredit)
        {
            return new OrderResultDto
            {
                Success = false,
                Message = "Order exceeds customer's available credit",
                ErrorCode = "CREDIT_LIMIT_EXCEEDED"
            };
        }

        // Generate order number (e.g., ORD-YYYYMMDDxxxx)
        string orderNumber = GenerateOrderNumber();
        
        // Create the order
        var order = new Order
        {
            OrderNumber = orderNumber,
            CustomerId = dto.CustomerId,
            Status = "Pending",
            SubTotal = dto.SubTotal,
            TaxAmount = dto.TaxAmount,
            DiscountAmount = dto.DiscountAmount,
            TotalAmount = dto.TotalAmount,
            Notes = dto.Notes,
            OrderDate = DateTime.UtcNow,
            CreatedBy = dto.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };

        // Save order to get an Id
        order = await _orderRepository.AddAsync(order);

        // Process order items and check inventory
        var inventoryCheckResults = new ConcurrentDictionary<string, bool>();
        var orderItems = new ConcurrentBag<OrderItem>();
        var productAvailabilityRequests = dto.Items.Select(i => new ProductAvailabilityRequestDto
        {
            ProductId = i.ProductSku,
            Quantity = i.Quantity,
            WarehouseId = i.WarehouseCode
        }).ToList();

        // Check product availability in batches to avoid multiple network calls
        var availabilityResults = await _inventoryServiceClient.CheckProductsAvailabilityAsync(productAvailabilityRequests);
        
        // Map availability results to dictionary for easy lookup
        var availabilityDict = availabilityResults.ToDictionary(
            r => r.ProductId,
            r => r.IsAvailable
        );

        // Check if all products are available
        if (availabilityDict.Values.Any(v => !v))
        {
            var unavailableProducts = availabilityDict
                .Where(kvp => !kvp.Value)
                .Select(kvp => kvp.Key)
                .ToList();
                
            return new OrderResultDto
            {
                Success = false,
                Message = $"Some products are unavailable: {string.Join(", ", unavailableProducts)}",
                ErrorCode = "PRODUCTS_UNAVAILABLE",
                Order = _mapper.Map<OrderDto>(order)
            };
        }

        // Reserve stock for all items
        var allReservations = new List<StockReservationResultDto>();
        
        foreach (var item in dto.Items)
        {
            try
            {
                var reservation = await _inventoryServiceClient.ReserveStockAsync(new StockReservationRequestDto
                {
                    ProductId = item.ProductSku,
                    Quantity = item.Quantity,
                    WarehouseId = item.WarehouseCode,
                    ReservationDurationMinutes = 60 // 1 hour reservation by default
                });

                if (!reservation.Success)
                {
                    // If any reservation fails, release all previous reservations
                    foreach (var prevReservation in allReservations)
                    {
                        await _inventoryServiceClient.ReleaseStockReservationAsync(prevReservation.ReservationId);
                    }

                    return new OrderResultDto
                    {
                        Success = false,
                        Message = $"Failed to reserve stock for product {item.ProductSku}: {reservation.Message}",
                        ErrorCode = "STOCK_RESERVATION_FAILED",
                        Order = _mapper.Map<OrderDto>(order)
                    };
                }

                allReservations.Add(reservation);
                
                // Create order item
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductSku = item.ProductSku,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    LineTotal = item.LineTotal,
                    DiscountAmount = item.DiscountAmount,
                    TaxAmount = item.TaxAmount,
                    WarehouseCode = item.WarehouseCode,
                    ReservationId = reservation.ReservationId
                };

                await _orderItemRepository.AddAsync(orderItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during stock reservation for product {ProductSku}", item.ProductSku);
                
                // Release all previous reservations
                foreach (var prevReservation in allReservations)
                {
                    try
                    {
                        await _inventoryServiceClient.ReleaseStockReservationAsync(prevReservation.ReservationId);
                    }
                    catch (Exception releaseEx)
                    {
                        _logger.LogError(releaseEx, "Error releasing reservation {ReservationId}", prevReservation.ReservationId);
                    }
                }

                return new OrderResultDto
                {
                    Success = false,
                    Message = $"Error processing order: {ex.Message}",
                    ErrorCode = "SYSTEM_ERROR",
                    Order = _mapper.Map<OrderDto>(order)
                };
            }
        }

        // Update customer balance
        customer.CurrentBalance += dto.TotalAmount;
        customer.UpdatedAt = DateTime.UtcNow;
        await _customerRepository.UpdateAsync(customer);

        // Refresh the order with items
        order = await _orderRepository.GetByIdAsync(order.Id);
        
        _logger.LogInformation("Created new order {OrderNumber} for customer {CustomerId}", orderNumber, dto.CustomerId);

        return new OrderResultDto
        {
            Success = true,
            Message = "Order created successfully",
            Order = _mapper.Map<OrderDto>(order)
        };
    }

    public async Task<OrderResultDto> UpdateAsync(Guid id, UpdateOrderDto dto)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            return new OrderResultDto
            {
                Success = false,
                Message = "Order not found",
                ErrorCode = "ORDER_NOT_FOUND"
            };
        }

        // Can only update pending orders
        if (order.Status != "Pending")
        {
            return new OrderResultDto
            {
                Success = false,
                Message = $"Cannot update an order with status '{order.Status}'",
                ErrorCode = "INVALID_ORDER_STATUS"
            };
        }

        // Update basic order properties
        if (dto.Notes != null)
            order.Notes = dto.Notes;
            
        // Only update amounts if explicitly provided and order is still pending
        if (dto.SubTotal.HasValue)
            order.SubTotal = dto.SubTotal.Value;
            
        if (dto.TaxAmount.HasValue)
            order.TaxAmount = dto.TaxAmount.Value;
            
        if (dto.DiscountAmount.HasValue)
            order.DiscountAmount = dto.DiscountAmount.Value;
            
        if (dto.TotalAmount.HasValue)
            order.TotalAmount = dto.TotalAmount.Value;

        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = dto.UpdatedBy;
        
        await _orderRepository.UpdateAsync(order);
        
        _logger.LogInformation("Updated order {OrderId}", id);
        
        // Refresh the order with items
        order = await _orderRepository.GetByIdAsync(id);
        
        return new OrderResultDto
        {
            Success = true,
            Message = "Order updated successfully",
            Order = _mapper.Map<OrderDto>(order)
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            return false;
        }

        // Can only delete pending orders
        if (order.Status != "Pending")
        {
            return false;
        }

        // Release all stock reservations
        var orderItems = await _orderItemRepository.GetByOrderIdAsync(id);
        foreach (var item in orderItems)
        {
            if (!string.IsNullOrEmpty(item.ReservationId))
            {
                try
                {
                    await _inventoryServiceClient.ReleaseStockReservationAsync(item.ReservationId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error releasing reservation {ReservationId} for order deletion", item.ReservationId);
                }
            }
        }

        // Update customer balance
        var customer = await _customerRepository.GetByIdAsync(order.CustomerId);
        if (customer != null)
        {
            customer.CurrentBalance -= order.TotalAmount;
            customer.UpdatedAt = DateTime.UtcNow;
            await _customerRepository.UpdateAsync(customer);
        }

        await _orderRepository.DeleteAsync(id);
        
        _logger.LogInformation("Deleted order {OrderId}", id);
        
        return true;
    }

    public async Task<IEnumerable<OrderDto>> GetByCustomerIdAsync(Guid customerId)
    {
        var orders = await _orderRepository.GetByCustomerIdAsync(customerId);
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<IEnumerable<OrderDto>> GetByStatusAsync(string status)
    {
        var orders = await _orderRepository.GetByStatusAsync(status);
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<IEnumerable<OrderDto>> GetRecentOrdersAsync(int count)
    {
        var orders = await _orderRepository.GetRecentOrdersAsync(count);
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<OrderPagedResultDto> SearchAsync(OrderSearchDto searchDto)
    {
        // This would typically be implemented with more complex filtering
        // using a specification pattern or custom query builder
        // For now, we'll implement a simple search based on available repository methods
        
        // Start with all orders and filter in memory
        var allOrders = await _orderRepository.GetAllAsync();
        
        // Apply filters
        var filteredOrders = allOrders.AsQueryable();
        
        if (!string.IsNullOrEmpty(searchDto.OrderNumber))
        {
            filteredOrders = filteredOrders.Where(o => o.OrderNumber.Contains(searchDto.OrderNumber, StringComparison.OrdinalIgnoreCase));
        }
        
        if (searchDto.CustomerId.HasValue)
        {
            filteredOrders = filteredOrders.Where(o => o.CustomerId == searchDto.CustomerId);
        }
        
        if (!string.IsNullOrEmpty(searchDto.Status))
        {
            filteredOrders = filteredOrders.Where(o => o.Status == searchDto.Status);
        }
        
        if (searchDto.FromDate.HasValue)
        {
            filteredOrders = filteredOrders.Where(o => o.OrderDate >= searchDto.FromDate.Value);
        }
        
        if (searchDto.ToDate.HasValue)
        {
            filteredOrders = filteredOrders.Where(o => o.OrderDate <= searchDto.ToDate.Value);
        }
        
        // Apply sorting
        if (string.IsNullOrEmpty(searchDto.SortBy) || searchDto.SortBy.Equals("OrderDate", StringComparison.OrdinalIgnoreCase))
        {
            filteredOrders = searchDto.SortAscending
                ? filteredOrders.OrderBy(o => o.OrderDate)
                : filteredOrders.OrderByDescending(o => o.OrderDate);
        }
        else if (searchDto.SortBy.Equals("TotalAmount", StringComparison.OrdinalIgnoreCase))
        {
            filteredOrders = searchDto.SortAscending
                ? filteredOrders.OrderBy(o => o.TotalAmount)
                : filteredOrders.OrderByDescending(o => o.TotalAmount);
        }
        else if (searchDto.SortBy.Equals("OrderNumber", StringComparison.OrdinalIgnoreCase))
        {
            filteredOrders = searchDto.SortAscending
                ? filteredOrders.OrderBy(o => o.OrderNumber)
                : filteredOrders.OrderByDescending(o => o.OrderNumber);
        }
        
        // Get total count before pagination
        var totalCount = filteredOrders.Count();
        
        // Apply pagination
        var pageSize = searchDto.PageSize <= 0 ? 10 : searchDto.PageSize;
        var pageNumber = searchDto.PageNumber <= 0 ? 1 : searchDto.PageNumber;
        var pagedOrders = filteredOrders
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
            
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        
        return new OrderPagedResultDto
        {
            Orders = _mapper.Map<IEnumerable<OrderDto>>(pagedOrders),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages,
            HasPrevious = pageNumber > 1,
            HasNext = pageNumber < totalPages
        };
    }

    public async Task<OrderResultDto> ConfirmOrderAsync(Guid id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            return new OrderResultDto
            {
                Success = false,
                Message = "Order not found",
                ErrorCode = "ORDER_NOT_FOUND"
            };
        }

        // Can only confirm pending orders
        if (order.Status != "Pending")
        {
            return new OrderResultDto
            {
                Success = false,
                Message = $"Cannot confirm an order with status '{order.Status}'",
                ErrorCode = "INVALID_ORDER_STATUS"
            };
        }

        // Update order status
        order.Status = "Confirmed";
        order.ConfirmedAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;
        
        await _orderRepository.UpdateAsync(order);
        
        _logger.LogInformation("Confirmed order {OrderId}", id);
        
        return new OrderResultDto
        {
            Success = true,
            Message = "Order confirmed successfully",
            Order = _mapper.Map<OrderDto>(order)
        };
    }

    public async Task<OrderResultDto> ProcessOrderAsync(Guid id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            return new OrderResultDto
            {
                Success = false,
                Message = "Order not found",
                ErrorCode = "ORDER_NOT_FOUND"
            };
        }

        // Can only process confirmed orders
        if (order.Status != "Confirmed")
        {
            return new OrderResultDto
            {
                Success = false,
                Message = $"Cannot process an order with status '{order.Status}'",
                ErrorCode = "INVALID_ORDER_STATUS"
            };
        }

        // Update order status
        order.Status = "Processing";
        order.UpdatedAt = DateTime.UtcNow;
        
        await _orderRepository.UpdateAsync(order);
        
        _logger.LogInformation("Processing order {OrderId}", id);
        
        return new OrderResultDto
        {
            Success = true,
            Message = "Order is now being processed",
            Order = _mapper.Map<OrderDto>(order)
        };
    }

    public async Task<OrderResultDto> ShipOrderAsync(Guid id, string? trackingInfo = null)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            return new OrderResultDto
            {
                Success = false,
                Message = "Order not found",
                ErrorCode = "ORDER_NOT_FOUND"
            };
        }

        // Can only ship processing orders
        if (order.Status != "Processing")
        {
            return new OrderResultDto
            {
                Success = false,
                Message = $"Cannot ship an order with status '{order.Status}'",
                ErrorCode = "INVALID_ORDER_STATUS"
            };
        }

        // Update order status
        order.Status = "Shipped";
        order.ShippedAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;
        
        if (!string.IsNullOrEmpty(trackingInfo))
        {
            order.Notes = string.IsNullOrEmpty(order.Notes)
                ? $"Tracking: {trackingInfo}"
                : $"{order.Notes}\nTracking: {trackingInfo}";
        }
        
        await _orderRepository.UpdateAsync(order);
        
        _logger.LogInformation("Shipped order {OrderId}", id);
        
        return new OrderResultDto
        {
            Success = true,
            Message = "Order has been shipped",
            Order = _mapper.Map<OrderDto>(order)
        };
    }

    public async Task<OrderResultDto> DeliverOrderAsync(Guid id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            return new OrderResultDto
            {
                Success = false,
                Message = "Order not found",
                ErrorCode = "ORDER_NOT_FOUND"
            };
        }

        // Can only deliver shipped orders
        if (order.Status != "Shipped")
        {
            return new OrderResultDto
            {
                Success = false,
                Message = $"Cannot deliver an order with status '{order.Status}'",
                ErrorCode = "INVALID_ORDER_STATUS"
            };
        }

        // Update order status
        order.Status = "Delivered";
        order.DeliveredAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;
        
        await _orderRepository.UpdateAsync(order);
        
        _logger.LogInformation("Delivered order {OrderId}", id);
        
        return new OrderResultDto
        {
            Success = true,
            Message = "Order has been delivered",
            Order = _mapper.Map<OrderDto>(order)
        };
    }

    public async Task<OrderResultDto> CancelOrderAsync(Guid id, string reason)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            return new OrderResultDto
            {
                Success = false,
                Message = "Order not found",
                ErrorCode = "ORDER_NOT_FOUND"
            };
        }

        // Can only cancel pending, confirmed or processing orders
        if (order.Status != "Pending" && order.Status != "Confirmed" && order.Status != "Processing")
        {
            return new OrderResultDto
            {
                Success = false,
                Message = $"Cannot cancel an order with status '{order.Status}'",
                ErrorCode = "INVALID_ORDER_STATUS"
            };
        }

        // Release all stock reservations
        var orderItems = await _orderItemRepository.GetByOrderIdAsync(id);
        foreach (var item in orderItems)
        {
            if (!string.IsNullOrEmpty(item.ReservationId))
            {
                try
                {
                    await _inventoryServiceClient.ReleaseStockReservationAsync(item.ReservationId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error releasing reservation {ReservationId} for order cancellation", item.ReservationId);
                }
            }
        }

        // Update customer balance
        var customer = await _customerRepository.GetByIdAsync(order.CustomerId);
        if (customer != null)
        {
            customer.CurrentBalance -= order.TotalAmount;
            customer.UpdatedAt = DateTime.UtcNow;
            await _customerRepository.UpdateAsync(customer);
        }

        // Update order status
        order.Status = "Cancelled";
        order.CancelledAt = DateTime.UtcNow;
        order.CancellationReason = reason;
        order.UpdatedAt = DateTime.UtcNow;
        
        await _orderRepository.UpdateAsync(order);
        
        _logger.LogInformation("Cancelled order {OrderId}: {Reason}", id, reason);
        
        return new OrderResultDto
        {
            Success = true,
            Message = "Order has been cancelled",
            Order = _mapper.Map<OrderDto>(order)
        };
    }

    public async Task<OrderItemDto> AddItemToOrderAsync(Guid orderId, AddOrderItemDto dto)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            throw new InvalidOperationException("Order not found");
        }

        // Can only add items to pending orders
        if (order.Status != "Pending")
        {
            throw new InvalidOperationException($"Cannot add items to an order with status '{order.Status}'");
        }

        // Check product availability
        var availabilityResult = await _inventoryServiceClient.CheckProductAvailabilityAsync(
            dto.ProductSku, 
            dto.Quantity,
            dto.WarehouseCode);
            
        if (!availabilityResult.IsAvailable)
        {
            throw new InvalidOperationException($"Product {dto.ProductSku} is not available in the requested quantity");
        }

        // Reserve stock
        var reservationResult = await _inventoryServiceClient.ReserveStockAsync(new StockReservationRequestDto
        {
            ProductId = dto.ProductSku,
            Quantity = dto.Quantity,
            WarehouseId = dto.WarehouseCode,
            ReservationDurationMinutes = 60 // 1 hour reservation by default
        });

        if (!reservationResult.Success)
        {
            throw new InvalidOperationException($"Failed to reserve stock: {reservationResult.Message}");
        }

        // Create order item
        var orderItem = new OrderItem
        {
            OrderId = orderId,
            ProductSku = dto.ProductSku,
            ProductName = dto.ProductName,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice ?? 0M,
            LineTotal = dto.LineTotal,
            DiscountAmount = dto.DiscountAmount,
            TaxAmount = dto.TaxAmount,
            WarehouseCode = dto.WarehouseCode,
            ReservationId = reservationResult.ReservationId
        };

        orderItem = await _orderItemRepository.AddAsync(orderItem);
        
        // Update order totals
        order.SubTotal += orderItem.LineTotal;
        order.TaxAmount += orderItem.TaxAmount;
        order.TotalAmount = order.SubTotal + order.TaxAmount - order.DiscountAmount;
        order.UpdatedAt = DateTime.UtcNow;
        
        await _orderRepository.UpdateAsync(order);
        
        // Update customer balance
        var customer = await _customerRepository.GetByIdAsync(order.CustomerId);
        if (customer != null)
        {
            customer.CurrentBalance += orderItem.LineTotal;
            customer.UpdatedAt = DateTime.UtcNow;
            await _customerRepository.UpdateAsync(customer);
        }
        
        _logger.LogInformation("Added item {ProductSku} to order {OrderId}", dto.ProductSku, orderId);
        
        return _mapper.Map<OrderItemDto>(orderItem);
    }

    public async Task<OrderItemDto> UpdateOrderItemAsync(Guid orderItemId, UpdateOrderItemDto dto)
    {
        var orderItem = await _orderItemRepository.GetByIdAsync(orderItemId);
        if (orderItem == null)
        {
            throw new InvalidOperationException("Order item not found");
        }

        var order = await _orderRepository.GetByIdAsync(orderItem.OrderId);
        if (order == null)
        {
            throw new InvalidOperationException("Order not found");
        }

        // Can only update items in pending orders
        if (order.Status != "Pending")
        {
            throw new InvalidOperationException($"Cannot update items in an order with status '{order.Status}'");
        }

        decimal oldLineTotal = orderItem.LineTotal;
        decimal oldTaxAmount = orderItem.TaxAmount;

        // If quantity is changing, update stock reservation
        if (dto.Quantity.HasValue && dto.Quantity.Value != orderItem.Quantity)
        {
            // Release old reservation
            if (!string.IsNullOrEmpty(orderItem.ReservationId))
            {
                await _inventoryServiceClient.ReleaseStockReservationAsync(orderItem.ReservationId);
            }

            // Check availability and create new reservation
            var availabilityResult = await _inventoryServiceClient.CheckProductAvailabilityAsync(
                orderItem.ProductSku, 
                dto.Quantity.Value,
                orderItem.WarehouseCode);
                
            if (!availabilityResult.IsAvailable)
            {
                throw new InvalidOperationException($"Product {orderItem.ProductSku} is not available in the requested quantity");
            }

            // Create new reservation
            var reservationResult = await _inventoryServiceClient.ReserveStockAsync(new StockReservationRequestDto
            {
                ProductId = orderItem.ProductSku,
                Quantity = dto.Quantity.Value,
                WarehouseId = orderItem.WarehouseCode,
                ReservationDurationMinutes = 60 // 1 hour reservation by default
            });

            if (!reservationResult.Success)
            {
                throw new InvalidOperationException($"Failed to reserve stock: {reservationResult.Message}");
            }

            orderItem.ReservationId = reservationResult.ReservationId;
            orderItem.Quantity = dto.Quantity.Value;
        }

        // Update other properties if provided
        if (dto.UnitPrice.HasValue)
            orderItem.UnitPrice = dto.UnitPrice.Value;
            
        if (dto.DiscountAmount.HasValue)
            orderItem.DiscountAmount = dto.DiscountAmount.Value;
            
        if (dto.TaxAmount.HasValue)
            orderItem.TaxAmount = dto.TaxAmount.Value;
            
        // Recalculate line total
        orderItem.LineTotal = orderItem.Quantity * orderItem.UnitPrice - orderItem.DiscountAmount;
        
        await _orderItemRepository.UpdateAsync(orderItem);
        
        // Update order totals
        order.SubTotal = order.SubTotal - oldLineTotal + orderItem.LineTotal;
        order.TaxAmount = order.TaxAmount - oldTaxAmount + orderItem.TaxAmount;
        order.TotalAmount = order.SubTotal + order.TaxAmount - order.DiscountAmount;
        order.UpdatedAt = DateTime.UtcNow;
        
        await _orderRepository.UpdateAsync(order);
        
        // Update customer balance
        var customer = await _customerRepository.GetByIdAsync(order.CustomerId);
        if (customer != null)
        {
            customer.CurrentBalance = customer.CurrentBalance - oldLineTotal + orderItem.LineTotal;
            customer.UpdatedAt = DateTime.UtcNow;
            await _customerRepository.UpdateAsync(customer);
        }
        
        _logger.LogInformation("Updated item {OrderItemId} in order {OrderId}", orderItemId, order.Id);
        
        return _mapper.Map<OrderItemDto>(orderItem);
    }

    public async Task<bool> RemoveItemFromOrderAsync(Guid orderItemId)
    {
        var orderItem = await _orderItemRepository.GetByIdAsync(orderItemId);
        if (orderItem == null)
        {
            return false;
        }

        var order = await _orderRepository.GetByIdAsync(orderItem.OrderId);
        if (order == null)
        {
            return false;
        }

        // Can only remove items from pending orders
        if (order.Status != "Pending")
        {
            return false;
        }

        // Release stock reservation
        if (!string.IsNullOrEmpty(orderItem.ReservationId))
        {
            await _inventoryServiceClient.ReleaseStockReservationAsync(orderItem.ReservationId);
        }

        // Update order totals
        order.SubTotal -= orderItem.LineTotal;
        order.TaxAmount -= orderItem.TaxAmount;
        order.TotalAmount = order.SubTotal + order.TaxAmount - order.DiscountAmount;
        order.UpdatedAt = DateTime.UtcNow;
        
        await _orderRepository.UpdateAsync(order);
        
        // Update customer balance
        var customer = await _customerRepository.GetByIdAsync(order.CustomerId);
        if (customer != null)
        {
            customer.CurrentBalance -= orderItem.LineTotal;
            customer.UpdatedAt = DateTime.UtcNow;
            await _customerRepository.UpdateAsync(customer);
        }

        // Delete the order item
        await _orderItemRepository.DeleteAsync(orderItemId);
        
        _logger.LogInformation("Removed item {OrderItemId} from order {OrderId}", orderItemId, order.Id);
        
        return true;
    }

    public async Task<bool> ValidateOrderAsync(Guid id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            return false;
        }

        // Check if order has items
        var orderItems = await _orderItemRepository.GetByOrderIdAsync(id);
        if (!orderItems.Any())
        {
            return false;
        }

        // Validate customer
        var customer = await _customerRepository.GetByIdAsync(order.CustomerId);
        if (customer == null || !customer.IsActive)
        {
            return false;
        }

        // Check credit limit
        if (customer.CreditLimit > 0 && order.TotalAmount > customer.AvailableCredit)
        {
            return false;
        }

        // Validate inventory for all items
        foreach (var item in orderItems)
        {
            var availabilityResult = await _inventoryServiceClient.CheckProductAvailabilityAsync(
                item.ProductSku, 
                item.Quantity,
                item.WarehouseCode);
                
            if (!availabilityResult.IsAvailable)
            {
                return false;
            }
        }

        return true;
    }

    public async Task<bool> CheckInventoryAvailabilityAsync(Guid productId, int quantity, Guid? warehouseId = null)
    {
        try
        {
            var result = await _inventoryServiceClient.CheckProductAvailabilityAsync(
                productId.ToString(), 
                quantity,
                warehouseId?.ToString());
                
            return result.IsAvailable;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking inventory availability for product {ProductId}", productId);
            return false;
        }
    }

    private string GenerateOrderNumber()
    {
        // Generate order number with format: ORD-YYYYMMDDxxxx
        // where xxxx is a random 4-digit number
        string dateComponent = DateTime.UtcNow.ToString("yyyyMMdd");
        string randomComponent = new Random().Next(1000, 9999).ToString();
        return $"ORD-{dateComponent}{randomComponent}";
    }
}
