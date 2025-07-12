using Grpc.Core;
using Shared.Contracts.Inventory;
using InventoryService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryService.Api.Services;

public class InventoryGrpcService : Shared.Contracts.Inventory.InventoryService.InventoryServiceBase
{
    private readonly IProductService _productService;
    private readonly IMultiWarehouseStockService _multiWarehouseStockService;
    private readonly IStockReservationService _stockReservationService;
    private readonly ILogger<InventoryGrpcService> _logger;

    public InventoryGrpcService(
        IProductService productService,
        IMultiWarehouseStockService multiWarehouseStockService,
        IStockReservationService stockReservationService,
        ILogger<InventoryGrpcService> logger)
    {
        _productService = productService;
        _multiWarehouseStockService = multiWarehouseStockService;
        _stockReservationService = stockReservationService;
        _logger = logger;
    }

    public override async Task<CheckAvailabilityResponse> CheckProductAvailability(
        CheckAvailabilityRequest request, 
        ServerCallContext context)
    {
        _logger.LogInformation("🔍 gRPC: CheckProductAvailability - ProductId: {ProductId}, Quantity: {Quantity}", 
            request.ProductId, request.Quantity);

        try
        {
            // Validate request
            if (string.IsNullOrEmpty(request.ProductId) || !Guid.TryParse(request.ProductId, out var productId))
            {
                return new CheckAvailabilityResponse
                {
                    IsAvailable = false,
                    AvailableQuantity = 0,
                    ErrorMessage = "Invalid product ID format"
                };
            }

            if (request.Quantity <= 0)
            {
                return new CheckAvailabilityResponse
                {
                    IsAvailable = false,
                    AvailableQuantity = 0,
                    ErrorMessage = "Quantity must be greater than 0"
                };
            }

            // Check product availability across all warehouses
            var availabilityCheck = await _multiWarehouseStockService.CheckStockAvailabilityAsync(productId, request.Quantity);
            
            _logger.LogInformation("✅ gRPC: Availability check complete - Available: {IsAvailable}, Total: {TotalQuantity}", 
                availabilityCheck.IsAvailable, availabilityCheck.TotalAvailableQuantity);

            // Convert warehouse stocks to proto format
            var warehouseStocks = availabilityCheck.WarehouseStocks.Select(ws => new WarehouseStock
            {
                WarehouseId = ws.WarehouseId.ToString(),
                WarehouseName = ws.WarehouseName,
                AvailableQuantity = ws.AvailableQuantity,
                ReservedQuantity = ws.ReservedQuantity
            }).ToList();

            var response = new CheckAvailabilityResponse
            {
                IsAvailable = availabilityCheck.IsAvailable,
                AvailableQuantity = availabilityCheck.TotalAvailableQuantity,
                ErrorMessage = string.Empty
            };

            response.WarehouseStocks.AddRange(warehouseStocks);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ gRPC Error checking product availability for ProductId: {ProductId}", request.ProductId);
            return new CheckAvailabilityResponse
            {
                IsAvailable = false,
                AvailableQuantity = 0,
                ErrorMessage = $"Internal server error: {ex.Message}"
            };
        }
    }

    public override async Task<ReserveStockResponse> ReserveStock(
        ReserveStockRequest request, 
        ServerCallContext context)
    {
        _logger.LogInformation("🔒 gRPC: ReserveStock - ProductId: {ProductId}, WarehouseId: {WarehouseId}, Quantity: {Quantity}", 
            request.ProductId, request.WarehouseId, request.Quantity);

        try
        {
            // Validate request
            if (string.IsNullOrEmpty(request.ProductId) || !Guid.TryParse(request.ProductId, out var productId))
            {
                return new ReserveStockResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid product ID format"
                };
            }

            if (string.IsNullOrEmpty(request.WarehouseId) || !Guid.TryParse(request.WarehouseId, out var warehouseId))
            {
                return new ReserveStockResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid warehouse ID format"
                };
            }

            if (request.Quantity <= 0)
            {
                return new ReserveStockResponse
                {
                    Success = false,
                    ErrorMessage = "Quantity must be greater than 0"
                };
            }

            // Create reservation request
            var reservationRequest = new InventoryService.Application.DTOs.CreateStockReservationDto
            {
                ProductId = productId,
                WarehouseId = warehouseId,
                Quantity = request.Quantity,
                ReservationDurationMinutes = request.TimeoutMinutes > 0 ? request.TimeoutMinutes : 30,
                Reason = "gRPC reservation request"
            };

            // Reserve stock using business service
            var reservationResult = await _stockReservationService.ReserveStockAsync(reservationRequest);

            _logger.LogInformation("🔒 gRPC: Reservation result - Success: {Success}, ReservationId: {ReservationId}", 
                reservationResult.Success, reservationResult.ReservationId);

            return new ReserveStockResponse
            {
                Success = reservationResult.Success,
                ReservationId = reservationResult.ReservationId ?? string.Empty,
                ReservedQuantity = reservationResult.Reservation?.Quantity ?? 0,
                ExpiryTime = reservationResult.Reservation?.ExpiresAt.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? string.Empty,
                ErrorMessage = reservationResult.Success ? string.Empty : reservationResult.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ gRPC Error reserving stock for ProductId: {ProductId}", request.ProductId);
            return new ReserveStockResponse
            {
                Success = false,
                ErrorMessage = $"Internal server error: {ex.Message}"
            };
        }
    }

    public override async Task<GetProductDetailsResponse> GetProductDetails(
        GetProductDetailsRequest request, 
        ServerCallContext context)
    {
        _logger.LogInformation("📦 gRPC: GetProductDetails - ProductId: {ProductId}", request.ProductId);

        try
        {
            // Validate request
            if (string.IsNullOrEmpty(request.ProductId) || !Guid.TryParse(request.ProductId, out var productId))
            {
                return new GetProductDetailsResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid product ID format"
                };
            }

            // Get product details
            var product = await _productService.GetByIdAsync(productId);
            if (product == null)
            {
                return new GetProductDetailsResponse
                {
                    Success = false,
                    ErrorMessage = "Product not found"
                };
            }

            // Get stock levels across warehouses
            var stockLevels = new List<WarehouseStock>();
            var stockByWarehouse = await _multiWarehouseStockService.GetStockByWarehouseAsync(productId);
            
            // Transform warehouse stock data to the gRPC format
            foreach (var warehouseStock in stockByWarehouse)
            {
                // We have warehouse ID and available quantity, but need to get warehouse name and reserved quantity
                stockLevels.Add(new WarehouseStock
                {
                    WarehouseId = warehouseStock.Key.ToString(),
                    AvailableQuantity = warehouseStock.Value,
                    // Note: We don't have warehouse name and reserved quantity in the simple dictionary
                    // They would be added here if we had a more detailed DTO
                    TotalQuantity = warehouseStock.Value // Assuming total = available if we don't have reserved data
                });
            }

            _logger.LogInformation("📦 gRPC: Product details retrieved - Name: {ProductName}, Stock Levels: {StockLevelCount}", 
                product.Name, stockLevels.Count);

            // Create the product info object that matches the proto definition
            var productInfo = new ProductInfo
            {
                Id = product.Id.ToString(),
                Name = product.Name,
                Description = product.Description ?? string.Empty,
                Sku = product.Sku,
                UnitPrice = (double)product.Price,
                Category = product.Category ?? string.Empty,
                IsActive = product.IsActive
            };

            // Add stock levels to product info
            productInfo.StockLevels.AddRange(stockLevels);

            // Create the response with the product info
            var response = new GetProductDetailsResponse
            {
                Product = productInfo,
                Success = true,
                ErrorMessage = string.Empty
            };

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ gRPC Error getting product details for ProductId: {ProductId}", request.ProductId);
            return new GetProductDetailsResponse
            {
                Success = false,
                ErrorMessage = $"Internal server error: {ex.Message}"
            };
        }
    }

    public override async Task<ReleaseStockResponse> ReleaseStock(
        ReleaseStockRequest request, 
        ServerCallContext context)
    {
        _logger.LogInformation("🔓 gRPC: ReleaseStock - ReservationId: {ReservationId}", request.ReservationId);

        try
        {
            if (string.IsNullOrEmpty(request.ReservationId))
            {
                return new ReleaseStockResponse
                {
                    Success = false,
                    ErrorMessage = "Reservation ID is required"
                };
            }

            var result = await _stockReservationService.ReleaseReservationAsync(request.ReservationId);

            return new ReleaseStockResponse
            {
                Success = result.Success,
                ErrorMessage = result.Success ? string.Empty : result.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ gRPC Error releasing stock for ReservationId: {ReservationId}", request.ReservationId);
            return new ReleaseStockResponse
            {
                Success = false,
                ErrorMessage = $"Internal server error: {ex.Message}"
            };
        }
    }
}
