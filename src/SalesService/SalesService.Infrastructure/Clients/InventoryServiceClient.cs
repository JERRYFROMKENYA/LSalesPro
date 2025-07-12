
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using SalesService.Application.DTOs;
using SalesService.Application.Interfaces;
using Shared.Contracts.Inventory;

namespace SalesService.Infrastructure.Clients;

public class InventoryServiceClient : IInventoryServiceClient
{
    private readonly InventoryService.InventoryServiceClient _client;
    private readonly ILogger<InventoryServiceClient> _logger;

    public InventoryServiceClient(ILogger<InventoryServiceClient> logger)
    {
        var channel = GrpcChannel.ForAddress("http://localhost:5003");
        _client = new InventoryService.InventoryServiceClient(channel);
        _logger = logger;
    }

    public async Task<ProductAvailabilityDto> CheckProductAvailabilityAsync(string productId, int quantity, string? warehouseId = null)
    {
        var request = new CheckAvailabilityRequest
        {
            ProductId = productId,
            Quantity = quantity,
            WarehouseId = warehouseId ?? string.Empty
        };

        try
        {
            var response = await _client.CheckProductAvailabilityAsync(request);
            return new ProductAvailabilityDto
            {
                ProductId = productId,
                IsAvailable = response.IsAvailable,
                AvailableQuantity = response.AvailableQuantity
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error checking product availability for {ProductId}", productId);
            return new ProductAvailabilityDto { ProductId = productId, IsAvailable = false };
        }
    }

    public async Task<IEnumerable<ProductAvailabilityDto>> CheckProductsAvailabilityAsync(IEnumerable<ProductAvailabilityRequestDto> requests)
    {
        var responses = new List<ProductAvailabilityDto>();
        foreach (var request in requests)
        {
            responses.Add(await CheckProductAvailabilityAsync(request.ProductId, request.Quantity, request.WarehouseId));
        }
        return responses;
    }

    public async Task<StockReservationResultDto> ReserveStockAsync(StockReservationRequestDto request)
    {
        var grpcRequest = new ReserveStockRequest
        {
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            WarehouseId = request.WarehouseId,
            TimeoutMinutes = request.ReservationDurationMinutes
        };

        try
        {
            var response = await _client.ReserveStockAsync(grpcRequest);
            return new StockReservationResultDto
            {
                Success = response.Success,
                ReservationId = response.ReservationId,
                Message = response.ErrorMessage
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error reserving stock for {ProductId}", request.ProductId);
            return new StockReservationResultDto { Success = false, Message = ex.Status.Detail };
        }
    }

    public async Task<bool> ReleaseStockReservationAsync(string reservationId)
    {
        var request = new ReleaseStockRequest { ReservationId = reservationId };

        try
        {
            var response = await _client.ReleaseStockAsync(request);
            return response.Success;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error releasing stock reservation {ReservationId}", reservationId);
            return false;
        }
    }

    public async Task<ProductPricingDto?> GetProductPricingInformationAsync(string productId)
    {
        var request = new GetProductDetailsRequest { ProductId = productId };

        try
        {
            var response = await _client.GetProductDetailsAsync(request);
            if (response?.Product == null)
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
            _logger.LogError(ex, "Error getting product pricing information for {ProductId}", productId);
            return null;
        }
    }
}
