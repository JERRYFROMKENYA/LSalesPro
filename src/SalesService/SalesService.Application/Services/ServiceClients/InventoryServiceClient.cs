using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SalesService.Application.DTOs;
using SalesService.Application.Interfaces;
using SalesService.Application.Settings;
using Shared.Contracts;
using Shared.Contracts.Inventory;

namespace SalesService.Application.Services.ServiceClients;

public class InventoryServiceClient : IInventoryServiceClient, IDisposable
{
    private readonly GrpcChannel _channel;
    private readonly Shared.Contracts.Inventory.InventoryService.InventoryServiceClient _client;
    private readonly ILogger<InventoryServiceClient> _logger;
    private bool _disposed;

    public InventoryServiceClient(IOptions<ServiceSettings> settings, ILogger<InventoryServiceClient> logger)
    {
        _logger = logger;
        
        try
        {
            var inventoryServiceUrl = settings.Value.InventoryServiceUrl;
            if (string.IsNullOrEmpty(inventoryServiceUrl))
            {
                throw new InvalidOperationException("InventoryService URL not configured");
            }

            _channel = GrpcChannel.ForAddress(inventoryServiceUrl);
            _client = new Shared.Contracts.Inventory.InventoryService.InventoryServiceClient(_channel);
            
            _logger.LogInformation("InventoryServiceClient initialized with URL: {Url}", inventoryServiceUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize InventoryServiceClient");
            throw;
        }
    }

    public async Task<ProductAvailabilityDto> CheckProductAvailabilityAsync(string productId, int quantity, string? warehouseId = null)
    {
        try
        {
            var request = new CheckAvailabilityRequest
            {
                ProductId = productId,
                Quantity = quantity
            };

            if (!string.IsNullOrEmpty(warehouseId))
            {
                request.WarehouseId = warehouseId;
            }

            var response = await _client.CheckProductAvailabilityAsync(request);

            return new ProductAvailabilityDto
            {
                ProductId = productId,
                IsAvailable = response.IsAvailable,
                AvailableQuantity = response.AvailableQuantity,
                // WarehouseId is not present in response; if needed, get from warehouse_stocks or use input warehouseId
                WarehouseId = warehouseId, // fallback to input value
                Message = response.ErrorMessage
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error checking product availability for ProductId: {ProductId}, Quantity: {Quantity}, WarehouseId: {WarehouseId}", 
                productId, quantity, warehouseId);
            return new ProductAvailabilityDto
            {
                ProductId = productId,
                IsAvailable = false,
                AvailableQuantity = 0,
                WarehouseId = warehouseId,
                Message = $"Error: {ex.Status.Detail}"
            };
        }
    }

    public async Task<IEnumerable<ProductAvailabilityDto>> CheckProductsAvailabilityAsync(IEnumerable<ProductAvailabilityRequestDto> requests)
    {
        var results = new List<ProductAvailabilityDto>();
        foreach (var request in requests)
        {
            var result = await CheckProductAvailabilityAsync(
                request.ProductId,
                request.Quantity,
                request.WarehouseId
            );
            results.Add(result);
        }
        return results;
    }

    public async Task<StockReservationResultDto> ReserveStockAsync(StockReservationRequestDto request
    )
    {
        try
        {
            var grpcRequest = new ReserveStockRequest
            {
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                TimeoutMinutes = request.TimeoutMinutes
            };

            if (!string.IsNullOrEmpty(request.WarehouseId))
            {
                grpcRequest.WarehouseId = request.WarehouseId;
            }

            if (!string.IsNullOrEmpty(request.ReservationId))
            {
                grpcRequest.ReservationId = request.ReservationId;
            }

            var response = await _client.ReserveStockAsync(grpcRequest);

            return new StockReservationResultDto
            {
                Success = response.Success,
                ReservationId = response.ReservationId,
                ReservedQuantity = response.ReservedQuantity,
                ExpiryTime = DateTime.Parse(response.ExpiryTime),
                ErrorMessage = response.ErrorMessage
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error reserving stock for ProductId: {ProductId}, Quantity: {Quantity}, WarehouseId: {WarehouseId}", 
                request.ProductId, request.Quantity, request.WarehouseId);
            
            return new StockReservationResultDto
            {
                Success = false,
                ErrorMessage = $"Error: {ex.Status.Detail}"
            };
        }
    }

    public async Task<bool> ReleaseStockReservationAsync(string reservationId)
    {
        try
        {
            var request = new ReleaseStockRequest
            {
                ReservationId = reservationId
            };

            var response = await _client.ReleaseStockAsync(request);

            return response.Success;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error releasing stock reservation: {ReservationId}", reservationId);
            return false;
        }
    }

    public async Task<ProductPricingDto?> GetProductPricingInformationAsync(string productId)
    {
        try
        {
            var request = new GetProductDetailsRequest
            {
                ProductId = productId
            };

            var response = await _client.GetProductDetailsAsync(request);

            if (response == null || response.Product == null || string.IsNullOrEmpty(response.Product.Id))
            {
                return null;
            }

            return new ProductPricingDto
            {
                ProductId = response.Product.Id,
                UnitPrice = (decimal)response.Product.UnitPrice,
                TaxRate = (decimal)response.Product.TaxRate
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error getting product pricing information for ProductId: {ProductId}", productId);
            return null;
        }
    }

    public async Task<ProductDetailsDto?> GetProductDetailsAsync(string productId)
    {
        try
        {
            var request = new GetProductDetailsRequest
            {
                ProductId = productId
            };

            var response = await _client.GetProductDetailsAsync(request);

            if (response == null || response.Product == null || string.IsNullOrEmpty(response.Product.Id))
            {
                return null;
            }

            return new ProductDetailsDto
            {
                Id = response.Product.Id,
                Name = response.Product.Name,
                Description = response.Product.Description,
                Sku = response.Product.Sku,
                UnitPrice = (decimal)response.Product.UnitPrice,
                Category = response.Product.Category,
                IsActive = response.Product.IsActive,
                StockLevels = response.Product.StockLevels.Select(sl => new WarehouseStockDto
                {
                    WarehouseId = sl.WarehouseId,
                    WarehouseName = sl.WarehouseName,
                    AvailableQuantity = sl.AvailableQuantity,
                    ReservedQuantity = sl.ReservedQuantity,
                    TotalQuantity = sl.TotalQuantity
                }).ToList()
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error getting product details for ProductId: {ProductId}", productId);
            return null;
        }
    }

    public async Task<List<ProductDetailsDto>> GetProductsBatchAsync(List<string> productIds)
    {
        var results = new List<ProductDetailsDto>();
        
        foreach (var productId in productIds)
        {
            // Process requests in sequence to avoid overwhelming the gRPC service
            var result = await GetProductDetailsAsync(productId);
            if (result != null)
            {
                results.Add(result);
            }
        }
        
        return results;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _channel?.Dispose();
        }

        _disposed = true;
    }
}
