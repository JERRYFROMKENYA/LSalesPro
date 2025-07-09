using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Contracts.Inventory;
using InventoryService.Api.Services;
using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using Xunit;

namespace InventoryService.Tests;

public class InventoryGrpcServiceTest
{
    private readonly Mock<IProductService> _mockProductService;
    private readonly Mock<IMultiWarehouseStockService> _mockMultiWarehouseStockService;
    private readonly Mock<IStockReservationService> _mockStockReservationService;
    private readonly Mock<ILogger<InventoryGrpcService>> _mockLogger;
    private readonly InventoryGrpcService _grpcService;

    public InventoryGrpcServiceTest()
    {
        _mockProductService = new Mock<IProductService>();
        _mockMultiWarehouseStockService = new Mock<IMultiWarehouseStockService>();
        _mockStockReservationService = new Mock<IStockReservationService>();
        _mockLogger = new Mock<ILogger<InventoryGrpcService>>();

        _grpcService = new InventoryGrpcService(
            _mockProductService.Object,
            _mockMultiWarehouseStockService.Object,
            _mockStockReservationService.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task GetProductDetails_ValidProduct_ShouldReturnCorrectDetails()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var mockProduct = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            Sku = "TST-001",
            Description = "Test product description",
            Category = "Test Category",
            Price = 19.99m,
            IsActive = true,
            ReorderLevel = 10
        };

        // Setup mock warehouse stock
        var warehouseStocks = new Dictionary<Guid, int>
        {
            { Guid.NewGuid(), 50 },
            { Guid.NewGuid(), 25 }
        };

        _mockProductService.Setup(s => s.GetByIdAsync(productId)).ReturnsAsync(mockProduct);
        _mockMultiWarehouseStockService.Setup(s => s.GetStockByWarehouseAsync(productId)).ReturnsAsync(warehouseStocks);

        // Create the gRPC request
        var request = new GetProductDetailsRequest { ProductId = productId.ToString() };
        var serverCallContext = new Mock<ServerCallContext>().Object;

        // Act
        var response = await _grpcService.GetProductDetails(request, serverCallContext);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Product);
        Assert.Equal(productId.ToString(), response.Product.Id);
        Assert.Equal(mockProduct.Name, response.Product.Name);
        Assert.Equal(warehouseStocks.Count, response.Product.StockLevels.Count);
        Assert.Empty(response.ErrorMessage);
    }
    
    [Fact]
    public async Task GetProductDetails_InvalidProductId_ShouldReturnError()
    {
        // Arrange
        var request = new GetProductDetailsRequest { ProductId = "invalid-id" };
        var serverCallContext = new Mock<ServerCallContext>().Object;

        // Act
        var response = await _grpcService.GetProductDetails(request, serverCallContext);

        // Assert
        Assert.False(response.Success);
        Assert.NotEmpty(response.ErrorMessage);
        Assert.Contains("Invalid product ID format", response.ErrorMessage);
    }
    
    [Fact]
    public async Task GetProductDetails_ProductNotFound_ShouldReturnError()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _mockProductService.Setup(s => s.GetByIdAsync(nonExistentId)).ReturnsAsync((ProductDto)null);

        var request = new GetProductDetailsRequest { ProductId = nonExistentId.ToString() };
        var serverCallContext = new Mock<ServerCallContext>().Object;

        // Act
        var response = await _grpcService.GetProductDetails(request, serverCallContext);

        // Assert
        Assert.False(response.Success);
        Assert.NotEmpty(response.ErrorMessage);
        Assert.Contains("Product not found", response.ErrorMessage);
    }
    
    public async Task<bool> TestGetProductDetailsMethod()
    {
        try
        {
            // Run all the test methods
            await GetProductDetails_ValidProduct_ShouldReturnCorrectDetails();
            await GetProductDetails_InvalidProductId_ShouldReturnError();
            await GetProductDetails_ProductNotFound_ShouldReturnError();
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in test: {ex.Message}");
            return false;
        }
    }
}
