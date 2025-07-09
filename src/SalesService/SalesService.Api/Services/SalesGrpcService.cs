using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using SalesService.Application.DTOs;
using SalesService.Application.Interfaces;
using Shared.Contracts.Sales;

namespace SalesService.Api.Services
{
    public class SalesGrpcService : Sales.SalesBase
    {
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly IReportsService _reportsService;
        private readonly ILogger<SalesGrpcService> _logger;

        public SalesGrpcService(
            ICustomerService customerService,
            IOrderService orderService,
            IReportsService reportsService,
            ILogger<SalesGrpcService> logger)
        {
            _customerService = customerService;
            _orderService = orderService;
            _reportsService = reportsService;
            _logger = logger;
        }

        public override async Task<GetCustomerResponse> GetCustomer(GetCustomerRequest request, ServerCallContext context)
        {
            try
            {
                var customerId = Guid.Parse(request.CustomerId);
                var customer = await _customerService.GetByIdAsync(customerId);

                if (customer == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"Customer with ID {customerId} not found."));
                }

                return new GetCustomerResponse
                {
                    Id = customer.Id.ToString(),
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Email = customer.Email ?? string.Empty,
                    PhoneNumber = customer.PhoneNumber ?? string.Empty,
                    CustomerType = customer.CustomerType,
                    CustomerCategory = customer.CustomerCategory,
                    IsActive = customer.IsActive,
                    CreditLimit = (double)customer.CreditLimit,
                    CurrentBalance = (double)customer.CurrentBalance
                };
            }
            catch (FormatException)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid customer ID format."));
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer with ID {CustomerId}", request.CustomerId);
                throw new RpcException(new Status(StatusCode.Internal, "Error retrieving customer information."));
            }
        }

        public override async Task<CustomerValidationResponse> ValidateCustomer(CustomerValidationRequest request, ServerCallContext context)
        {
            try
            {
                var customerId = Guid.Parse(request.CustomerId);
                var customer = await _customerService.GetByIdAsync(customerId);

                if (customer == null)
                {
                    return new CustomerValidationResponse
                    {
                        IsValid = false,
                        ErrorMessage = $"Customer with ID {customerId} not found."
                    };
                }

                var isActive = customer.IsActive;
                var hasSufficientCredit = true;

                // Check credit if amount is provided
                if (request.OrderAmount > 0)
                {
                    hasSufficientCredit = customer.CurrentBalance + (decimal)request.OrderAmount <= customer.CreditLimit;
                }

                return new CustomerValidationResponse
                {
                    IsValid = isActive && hasSufficientCredit,
                    CustomerId = customer.Id.ToString(),
                    CustomerName = $"{customer.FirstName} {customer.LastName}",
                    IsActive = customer.IsActive,
                    CreditLimit = (double)customer.CreditLimit,
                    CurrentBalance = (double)customer.CurrentBalance,
                    ErrorMessage = !isActive ? "Customer account is inactive." : 
                                   !hasSufficientCredit ? "Insufficient credit available." : string.Empty
                };
            }
            catch (FormatException)
            {
                return new CustomerValidationResponse
                {
                    IsValid = false,
                    ErrorMessage = "Invalid customer ID format."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating customer with ID {CustomerId}", request.CustomerId);
                return new CustomerValidationResponse
                {
                    IsValid = false,
                    ErrorMessage = "Error validating customer information."
                };
            }
        }

        public override async Task<GetOrderResponse> GetOrder(GetOrderRequest request, ServerCallContext context)
        {
            try
            {
                OrderDto? order;

                if (!string.IsNullOrEmpty(request.OrderId))
                {
                    var orderId = Guid.Parse(request.OrderId);
                    order = await _orderService.GetByIdAsync(orderId);
                }
                else if (!string.IsNullOrEmpty(request.OrderNumber))
                {
                    order = await _orderService.GetByOrderNumberAsync(request.OrderNumber);
                }
                else
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Either OrderId or OrderNumber must be provided."));
                }

                if (order == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "Order not found."));
                }

                var response = new GetOrderResponse
                {
                    Id = order.Id.ToString(),
                    OrderNumber = order.OrderNumber,
                    CustomerId = order.CustomerId.ToString(),
                    CustomerName = order.CustomerName,
                    OrderDate = Timestamp.FromDateTime(order.OrderDate.ToUniversalTime()),
                    Status = order.Status,
                    TotalAmount = (double)order.TotalAmount,
                    ShippingAddress = order.ShippingAddress ?? string.Empty,
                    PaymentMethod = order.PaymentMethod ?? string.Empty
                };

                foreach (var item in order.Items)
                {
                    response.Items.Add(new OrderItemResponse
                    {
                        Id = item.Id.ToString(),
                        ProductSku = item.ProductSku,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = (double)item.UnitPrice,
                        LineTotal = (double)item.LineTotal
                    });
                }

                return response;
            }
            catch (FormatException)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ID format."));
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order information");
                throw new RpcException(new Status(StatusCode.Internal, "Error retrieving order information."));
            }
        }

        public override async Task<CreateOrderResponse> CreateOrder(CreateOrderRequest request, ServerCallContext context)
        {
            try
            {
                var customerId = Guid.Parse(request.CustomerId);
                
                // Map request to DTO
                var createOrderDto = new CreateOrderDto
                {
                    CustomerId = customerId,
                    ShippingAddress = request.ShippingAddress,
                    ShippingCity = request.ShippingCity,
                    ShippingState = request.ShippingState,
                    ShippingZipCode = request.ShippingZipCode,
                    BillingAddress = request.BillingAddress,
                    BillingCity = request.BillingCity,
                    BillingState = request.BillingState,
                    BillingZipCode = request.BillingZipCode,
                    PaymentMethod = request.PaymentMethod,
                    Notes = request.Notes,
                    Items = request.Items.Select(item => new CreateOrderItemDto
                    {
                        ProductSku = item.ProductSku,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = (decimal)item.UnitPrice
                    }).ToList()
                };

                // Create the order
                var result = await _orderService.CreateAsync(createOrderDto);
                
                if (!result.Success)
                {
                    return new CreateOrderResponse
                    {
                        Success = false,
                        ErrorMessage = result.ErrorMessage ?? result.Message,
                        ErrorCode = result.ErrorCode ?? "UNKNOWN_ERROR"
                    };
                }

                var order = result.Order;
                if (order == null)
                {
                    return new CreateOrderResponse
                    {
                        Success = false,
                        ErrorMessage = "Order created successfully but no order details returned.",
                        ErrorCode = "UNEXPECTED_ERROR"
                    };
                }

                var response = new CreateOrderResponse
                {
                    Success = true,
                    OrderId = order.Id.ToString(),
                    OrderNumber = order.OrderNumber,
                    TotalAmount = (double)order.TotalAmount,
                    Status = order.Status,
                    OrderDate = Timestamp.FromDateTime(order.OrderDate.ToUniversalTime())
                };

                return response;
            }
            catch (FormatException)
            {
                return new CreateOrderResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid ID format.",
                    ErrorCode = "INVALID_FORMAT"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return new CreateOrderResponse
                {
                    Success = false,
                    ErrorMessage = "Error creating order.",
                    ErrorCode = "SERVER_ERROR"
                };
            }
        }

        public override async Task<UpdateOrderStatusResponse> UpdateOrderStatus(UpdateOrderStatusRequest request, ServerCallContext context)
        {
            try
            {
                var orderId = Guid.Parse(request.OrderId);
                OrderResultDto result;

                switch (request.Status.ToLower())
                {
                    case "confirmed":
                        result = await _orderService.ConfirmOrderAsync(orderId);
                        break;
                    case "processing":
                        result = await _orderService.ProcessOrderAsync(orderId);
                        break;
                    case "shipped":
                        result = await _orderService.ShipOrderAsync(orderId, request.TrackingInfo);
                        break;
                    case "delivered":
                        result = await _orderService.DeliverOrderAsync(orderId);
                        break;
                    case "cancelled":
                        if (string.IsNullOrEmpty(request.Reason))
                        {
                            return new UpdateOrderStatusResponse
                            {
                                Success = false,
                                ErrorMessage = "Reason is required for cancelling an order.",
                                ErrorCode = "MISSING_REASON"
                            };
                        }
                        result = await _orderService.CancelOrderAsync(orderId, request.Reason);
                        break;
                    default:
                        return new UpdateOrderStatusResponse
                        {
                            Success = false,
                            ErrorMessage = $"Unsupported status: {request.Status}",
                            ErrorCode = "INVALID_STATUS"
                        };
                }

                if (!result.Success)
                {
                    return new UpdateOrderStatusResponse
                    {
                        Success = false,
                        ErrorMessage = result.ErrorMessage ?? result.Message,
                        ErrorCode = result.ErrorCode ?? "UNKNOWN_ERROR"
                    };
                }

                return new UpdateOrderStatusResponse
                {
                    Success = true,
                    OrderId = result.Order!.Id.ToString(),
                    OrderNumber = result.Order.OrderNumber,
                    Status = result.Order.Status
                };
            }
            catch (FormatException)
            {
                return new UpdateOrderStatusResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid order ID format.",
                    ErrorCode = "INVALID_FORMAT"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for OrderId: {OrderId}, Status: {Status}", 
                    request.OrderId, request.Status);
                
                return new UpdateOrderStatusResponse
                {
                    Success = false,
                    ErrorMessage = "Error updating order status.",
                    ErrorCode = "SERVER_ERROR"
                };
            }
        }

        public override async Task<UpdateCustomerBalanceResponse> UpdateCustomerBalance(UpdateCustomerBalanceRequest request, ServerCallContext context)
        {
            try
            {
                var customerId = Guid.Parse(request.CustomerId);
                var customer = await _customerService.GetByIdAsync(customerId);
                
                if (customer == null)
                {
                    return new UpdateCustomerBalanceResponse
                    {
                        Success = false,
                        ErrorMessage = $"Customer with ID {customerId} not found.",
                        ErrorCode = "CUSTOMER_NOT_FOUND"
                    };
                }

                // Update credit limit if provided
                if (request.HasCreditLimit)
                {
                    var creditLimitResult = await _customerService.UpdateCreditLimitAsync(
                        customerId, (decimal)request.CreditLimit);
                    
                    if (!creditLimitResult)
                    {
                        return new UpdateCustomerBalanceResponse
                        {
                            Success = false,
                            ErrorMessage = "Failed to update credit limit.",
                            ErrorCode = "UPDATE_FAILED"
                        };
                    }
                }
                
                // Get updated customer data
                customer = await _customerService.GetByIdAsync(customerId);
                if (customer == null)
                {
                    return new UpdateCustomerBalanceResponse
                    {
                        Success = false,
                        ErrorMessage = "Failed to retrieve updated customer data.",
                        ErrorCode = "RETRIEVAL_FAILED"
                    };
                }

                return new UpdateCustomerBalanceResponse
                {
                    Success = true,
                    CustomerId = customer.Id.ToString(),
                    CreditLimit = (double)customer.CreditLimit,
                    CurrentBalance = (double)customer.CurrentBalance,
                    AvailableCredit = (double)(customer.CreditLimit - customer.CurrentBalance)
                };
            }
            catch (FormatException)
            {
                return new UpdateCustomerBalanceResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid customer ID format.",
                    ErrorCode = "INVALID_FORMAT"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer balance for CustomerId: {CustomerId}", request.CustomerId);
                
                return new UpdateCustomerBalanceResponse
                {
                    Success = false,
                    ErrorMessage = "Error updating customer balance.",
                    ErrorCode = "SERVER_ERROR"
                };
            }
        }

        public override async Task<OrderStatusChangeResponse> NotifyOrderStatusChange(OrderStatusChangeRequest request, ServerCallContext context)
        {
            try
            {
                // Implement notification logic here
                _logger.LogInformation("Order status change notification received: OrderId={OrderId}, Status={OldStatus}->{NewStatus}", 
                    request.OrderId, request.PreviousStatus, request.NewStatus);
                
                // This could trigger internal events, send notifications to customers, etc.
                
                return new OrderStatusChangeResponse { Success = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing order status change notification");
                return new OrderStatusChangeResponse 
                { 
                    Success = false,
                    ErrorMessage = "Error processing notification"
                };
            }
        }

        public override async Task<CustomerOrderSummaryResponse> GetCustomerOrderSummary(CustomerOrderSummaryRequest request, ServerCallContext context)
        {
            try
            {
                var customerId = Guid.Parse(request.CustomerId);
                var fromDate = DateTimeOffset.FromUnixTimeMilliseconds(request.FromDate).DateTime;
                var toDate = DateTimeOffset.FromUnixTimeMilliseconds(request.ToDate).DateTime;
                
                var summary = await _reportsService.GetCustomerOrderSummaryAsync(customerId, fromDate, toDate);
                
                if (summary == null)
                {
                    return new CustomerOrderSummaryResponse
                    {
                        Success = false,
                        ErrorMessage = "Failed to retrieve customer order summary."
                    };
                }
                
                return new CustomerOrderSummaryResponse
                {
                    Success = true,
                    Summary = new CustomerOrderSummary
                    {
                        CustomerId = customerId.ToString(),
                        TotalOrders = summary.TotalOrders,
                        TotalValue = new Shared.Contracts.Common.Money 
                        { 
                            Amount = (double)summary.TotalValue,
                            Currency = "USD" // Assuming USD as default currency
                        },
                        AverageOrderValue = new Shared.Contracts.Common.Money
                        {
                            Amount = summary.TotalOrders > 0 ? (double)(summary.TotalValue / summary.TotalOrders) : 0,
                            Currency = "USD"
                        },
                        CancelledOrders = summary.CancelledOrders,
                        CompletedOrders = summary.CompletedOrders,
                        LastOrderDate = summary.LastOrderDate?.ToString("o") ?? string.Empty
                    }
                };
            }
            catch (FormatException)
            {
                return new CustomerOrderSummaryResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid customer ID format."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer order summary");
                return new CustomerOrderSummaryResponse
                {
                    Success = false,
                    ErrorMessage = "Error retrieving customer order summary."
                };
            }
        }
    }
}
