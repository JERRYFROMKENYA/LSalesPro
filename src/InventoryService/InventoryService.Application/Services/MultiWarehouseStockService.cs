using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Interfaces;

namespace InventoryService.Application.Services;

public class MultiWarehouseStockService : IMultiWarehouseStockService
{
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly IProductRepository _productRepository;
    private readonly IWarehouseRepository _warehouseRepository;

    public MultiWarehouseStockService(
        IInventoryItemRepository inventoryItemRepository,
        IProductRepository productRepository,
        IWarehouseRepository warehouseRepository)
    {
        _inventoryItemRepository = inventoryItemRepository;
        _productRepository = productRepository;
        _warehouseRepository = warehouseRepository;
    }

    public async Task<int> GetTotalAvailableStockAsync(Guid productId)
    {
        var inventoryItems = await _inventoryItemRepository.GetByProductIdAsync(productId);
        return inventoryItems.Sum(item => item.AvailableQuantity);
    }

    public async Task<Dictionary<Guid, int>> GetStockByWarehouseAsync(Guid productId)
    {
        var inventoryItems = await _inventoryItemRepository.GetByProductIdAsync(productId);
        return inventoryItems.ToDictionary(item => item.WarehouseId, item => item.AvailableQuantity);
    }

    public async Task<bool> IsStockAvailableAsync(Guid productId, int requiredQuantity)
    {
        var totalStock = await GetTotalAvailableStockAsync(productId);
        return totalStock >= requiredQuantity;
    }

    public async Task<MultiWarehouseStockAvailabilityDto> CheckStockAvailabilityAsync(Guid productId, int requiredQuantity)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID {productId} not found.");
        }

        var inventoryItems = await _inventoryItemRepository.GetByProductIdAsync(productId);
        var warehouses = await _warehouseRepository.GetByIdsAsync(inventoryItems.Select(i => i.WarehouseId).ToList());

        var warehouseStocks = inventoryItems.Select(item =>
        {
            var warehouse = warehouses.FirstOrDefault(w => w.Id == item.WarehouseId);
            return new WarehouseStockDto
            {
                WarehouseId = item.WarehouseId,
                WarehouseCode = warehouse?.Code ?? "Unknown",
                WarehouseName = warehouse?.Name ?? "Unknown",
                AvailableQuantity = item.AvailableQuantity,
                ReservedQuantity = item.ReservedQuantity,
                Latitude = warehouse?.Latitude,
                Longitude = warehouse?.Longitude
            };
        }).ToList();

        var totalAvailable = warehouseStocks.Sum(ws => ws.AvailableQuantity);

        return new MultiWarehouseStockAvailabilityDto
        {
            ProductId = productId,
            ProductName = product.Name,
            ProductSku = product.Sku,
            RequiredQuantity = requiredQuantity,
            TotalAvailableQuantity = totalAvailable,
            IsAvailable = totalAvailable >= requiredQuantity,
            WarehouseStocks = warehouseStocks
        };
    }

    public async Task<List<StockAllocationDto>> OptimizeStockAllocationAsync(Guid productId, int requiredQuantity, double? customerLatitude = null, double? customerLongitude = null)
    {
        var stockAvailability = await CheckStockAvailabilityAsync(productId, requiredQuantity);
        
        if (!stockAvailability.IsAvailable)
        {
            return new List<StockAllocationDto>();
        }

        var warehousesWithStock = stockAvailability.WarehouseStocks
            .Where(ws => ws.AvailableQuantity > 0)
            .ToList();

        // Calculate distances if customer location is provided
        if (customerLatitude.HasValue && customerLongitude.HasValue)
        {
            foreach (var warehouse in warehousesWithStock)
            {
                if (warehouse.Latitude.HasValue && warehouse.Longitude.HasValue)
                {
                    warehouse.DistanceKm = CalculateDistance(
                        customerLatitude.Value, customerLongitude.Value,
                        warehouse.Latitude.Value, warehouse.Longitude.Value);
                }
            }
        }

        // Sort by distance (if available) and then by available quantity
        var sortedWarehouses = warehousesWithStock
            .OrderBy(ws => ws.DistanceKm ?? double.MaxValue)
            .ThenByDescending(ws => ws.AvailableQuantity)
            .ToList();

        var allocations = new List<StockAllocationDto>();
        var remainingQuantity = requiredQuantity;
        var priority = 1;

        foreach (var warehouse in sortedWarehouses)
        {
            if (remainingQuantity <= 0) break;

            var allocationQuantity = Math.Min(remainingQuantity, warehouse.AvailableQuantity);
            
            allocations.Add(new StockAllocationDto
            {
                WarehouseId = warehouse.WarehouseId,
                WarehouseCode = warehouse.WarehouseCode,
                WarehouseName = warehouse.WarehouseName,
                AllocatedQuantity = allocationQuantity,
                DistanceKm = warehouse.DistanceKm,
                Priority = priority++,
                AllocationReason = warehouse.DistanceKm.HasValue ? "Closest warehouse" : "Best available stock"
            });

            remainingQuantity -= allocationQuantity;
        }

        return allocations;
    }

    public async Task<StockAllocationDto?> FindBestWarehouseAsync(Guid productId, int requiredQuantity, double? latitude = null, double? longitude = null)
    {
        var allocations = await OptimizeStockAllocationAsync(productId, requiredQuantity, latitude, longitude);
        return allocations.FirstOrDefault();
    }

    public async Task<StockTransferResultDto> TransferStockAsync(CreateStockTransferDto request)
    {
        try
        {
            // Validate request
            if (request.FromWarehouseId == request.ToWarehouseId)
            {
                return new StockTransferResultDto
                {
                    Success = false,
                    Message = "Source and destination warehouses cannot be the same"
                };
            }

            // Get source inventory item
            var fromItem = await _inventoryItemRepository.GetByProductAndWarehouseAsync(request.ProductId, request.FromWarehouseId);
            if (fromItem == null || fromItem.AvailableQuantity < request.Quantity)
            {
                return new StockTransferResultDto
                {
                    Success = false,
                    Message = $"Insufficient stock available. Available: {fromItem?.AvailableQuantity ?? 0}, Required: {request.Quantity}"
                };
            }

            // Get or create destination inventory item
            var toItem = await _inventoryItemRepository.GetByProductAndWarehouseAsync(request.ProductId, request.ToWarehouseId);
            if (toItem == null)
            {
                toItem = new Domain.Entities.InventoryItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = request.ProductId,
                    WarehouseId = request.ToWarehouseId,
                    AvailableQuantity = 0,
                    ReservedQuantity = 0,
                    LastUpdated = DateTime.UtcNow
                };
                await _inventoryItemRepository.CreateAsync(toItem);
            }

            // Execute transfer
            var success = await _inventoryItemRepository.TransferStockAsync(
                request.ProductId, 
                request.FromWarehouseId, 
                request.ToWarehouseId, 
                request.Quantity);

            if (success)
            {
                // Create transfer record
                var transferDto = new StockTransferDto
                {
                    Id = Guid.NewGuid(),
                    ProductId = request.ProductId,
                    FromWarehouseId = request.FromWarehouseId,
                    ToWarehouseId = request.ToWarehouseId,
                    Quantity = request.Quantity,
                    Reason = request.Reason,
                    Notes = request.Notes,
                    TransferDate = DateTime.UtcNow,
                    Status = "Completed"
                };

                return new StockTransferResultDto
                {
                    Success = true,
                    Message = "Stock transfer completed successfully",
                    TransferId = transferDto.Id,
                    Transfer = transferDto
                };
            }
            else
            {
                return new StockTransferResultDto
                {
                    Success = false,
                    Message = "Stock transfer failed due to insufficient quantity or other constraints"
                };
            }
        }
        catch (Exception ex)
        {
            return new StockTransferResultDto
            {
                Success = false,
                Message = $"Stock transfer failed: {ex.Message}"
            };
        }
    }

    public Task<List<StockTransferDto>> GetPendingTransfersAsync()
    {
        // For now, return empty list as we haven't implemented transfer tracking yet
        // This would be implemented with a StockTransfer entity and repository
        return Task.FromResult(new List<StockTransferDto>());
    }

    public Task<bool> ConfirmStockTransferAsync(Guid transferId)
    {
        // Placeholder for transfer confirmation logic
        // Would be implemented with StockTransfer entity tracking
        return Task.FromResult(true);
    }

    public async Task<List<StockDistributionDto>> GetStockDistributionAsync(Guid? productId = null)
    {
        var distributions = new List<StockDistributionDto>();

        if (productId.HasValue)
        {
            var distribution = await GetProductStockDistributionAsync(productId.Value);
            if (distribution != null)
            {
                distributions.Add(distribution);
            }
        }
        else
        {
            var products = await _productRepository.GetActiveProductsAsync();
            foreach (var product in products)
            {
                var distribution = await GetProductStockDistributionAsync(product.Id);
                if (distribution != null)
                {
                    distributions.Add(distribution);
                }
            }
        }

        return distributions;
    }

    public async Task<List<WarehouseStockSummaryDto>> GetWarehouseStockSummariesAsync()
    {
        var warehouses = await _warehouseRepository.GetActiveWarehousesAsync();
        var summaries = new List<WarehouseStockSummaryDto>();

        foreach (var warehouse in warehouses)
        {
            var inventoryItems = await _inventoryItemRepository.GetByWarehouseIdAsync(warehouse.Id);
            var products = await _productRepository.GetByIdsAsync(inventoryItems.Select(i => i.ProductId).ToList());

            var productStocks = inventoryItems.Select(item =>
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                return new ProductStockDto
                {
                    ProductId = item.ProductId,
                    ProductName = product?.Name ?? "Unknown",
                    ProductSku = product?.Sku ?? "Unknown",
                    AvailableQuantity = item.AvailableQuantity,
                    ReservedQuantity = item.ReservedQuantity,
                    IsLowStock = product != null && item.AvailableQuantity <= product.ReorderLevel
                };
            }).ToList();

            var totalStock = productStocks.Sum(ps => ps.AvailableQuantity + ps.ReservedQuantity);
            var totalAvailable = productStocks.Sum(ps => ps.AvailableQuantity);
            var totalReserved = productStocks.Sum(ps => ps.ReservedQuantity);

            summaries.Add(new WarehouseStockSummaryDto
            {
                WarehouseId = warehouse.Id,
                WarehouseCode = warehouse.Code,
                WarehouseName = warehouse.Name,
                TotalProducts = productStocks.Count,
                TotalStock = totalStock,
                TotalAvailable = totalAvailable,
                TotalReserved = totalReserved,
                CapacityUtilization = warehouse.Capacity > 0 ? (decimal)totalStock / warehouse.Capacity * 100 : 0,
                ProductStocks = productStocks
            });
        }

        return summaries;
    }

    public Task<List<StockRebalanceRecommendationDto>> GetStockRebalanceRecommendationsAsync()
    {
        // Placeholder for rebalance recommendations
        // Would analyze stock distribution and suggest improvements
        return Task.FromResult(new List<StockRebalanceRecommendationDto>());
    }

    public Task<bool> ExecuteStockRebalanceAsync(Guid recommendationId)
    {
        // Placeholder for executing rebalance recommendations
        return Task.FromResult(true);
    }

    // Helper methods
    private async Task<StockDistributionDto?> GetProductStockDistributionAsync(Guid productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) return null;

        var stockAvailability = await CheckStockAvailabilityAsync(productId, 0);
        var totalStock = stockAvailability.WarehouseStocks.Sum(ws => ws.AvailableQuantity + ws.ReservedQuantity);

        if (totalStock == 0) return null;

        // Calculate distribution variance (simple implementation)
        var averageStock = (double)totalStock / stockAvailability.WarehouseStocks.Count;
        var variance = stockAvailability.WarehouseStocks
            .Select(ws => Math.Pow((ws.AvailableQuantity + ws.ReservedQuantity) - averageStock, 2))
            .Average();

        return new StockDistributionDto
        {
            ProductId = productId,
            ProductName = product.Name,
            ProductSku = product.Sku,
            TotalStock = totalStock,
            WarehouseDistribution = stockAvailability.WarehouseStocks,
            DistributionVariance = (decimal)variance,
            IsWellDistributed = variance < (averageStock * 0.5) // Simple threshold
        };
    }

    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        // Haversine formula for calculating distance between two points on Earth
        const double R = 6371; // Earth's radius in kilometers

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }

    public async Task<WarehouseUtilizationDto> GetWarehouseUtilizationAsync(Guid warehouseId)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId);
        if (warehouse == null)
        {
            throw new InvalidOperationException($"Warehouse with ID {warehouseId} not found.");
        }

        var inventoryItems = await _inventoryItemRepository.GetByWarehouseIdAsync(warehouseId);
        var totalStock = inventoryItems.Sum(item => item.AvailableQuantity + item.ReservedQuantity);
        var productCount = inventoryItems.Count();

        return new WarehouseUtilizationDto
        {
            WarehouseId = warehouse.Id,
            WarehouseName = warehouse.Name,
            WarehouseCode = warehouse.Code,
            TotalCapacity = warehouse.Capacity,
            UsedCapacity = totalStock,
            UtilizationPercentage = warehouse.Capacity > 0 ? (decimal)totalStock / warehouse.Capacity * 100 : 0,
            ProductCount = productCount,
            LastUpdated = DateTime.UtcNow
        };
    }
}
