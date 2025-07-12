using System.Threading.Tasks;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using SalesService.Api;
using Shared.Contracts.Sales;
using Xunit;

using Microsoft.Extensions.DependencyInjection.Extensions;
using SalesService.Application.Settings;

using SalesService.Infrastructure.Data;
using SalesService.Domain.Entities;

using Moq;
using SalesService.Application.Interfaces;
using SalesService.Application.DTOs;

namespace SalesService.Tests
{
    public class SalesGrpcServiceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public SalesGrpcServiceIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.Configure<ServiceSettings>(settings =>
                    {
                        settings.InventoryServiceUrl = "http://localhost:5003";
                    });

                    var mockInventoryServiceClient = new Mock<IInventoryServiceClient>();
                    mockInventoryServiceClient.Setup(client => client.CheckProductsAvailabilityAsync(It.IsAny<List<ProductAvailabilityRequestDto>>()))
                        .ReturnsAsync(new List<ProductAvailabilityResultDto> { new ProductAvailabilityResultDto { ProductId = "SF-MAX-20W50", IsAvailable = true } });

                    mockInventoryServiceClient.Setup(client => client.ReserveStockAsync(It.IsAny<StockReservationRequestDto>()))
                        .ReturnsAsync(new StockReservationResultDto { Success = true, ReservationId = Guid.NewGuid().ToString() });

                    services.AddSingleton(mockInventoryServiceClient.Object);
                });
            });
        }

        private async Task<CreateOrderResponse> CreateTestOrderAsync(Sales.SalesClient client, string customerId)
        {
            var request = new CreateOrderRequest
            {
                CustomerId = customerId,
                ShippingAddress = "123 Main St",
                PaymentMethod = "Credit Card",
                Items =
                {
                    new OrderItemRequest { ProductSku = "SF-MAX-20W50", ProductName = "SuperFuel Max 20W-50", Quantity = 1, UnitPrice = 4500.00 }
                }
            };

            return await client.CreateOrderAsync(request);
        }

        [Fact]
        public async Task GetOrder_ReturnsExpectedResult()
        {
            // Arrange
            var client = _factory.CreateDefaultClient();
            var channel = GrpcChannel.ForAddress(client.BaseAddress!, new GrpcChannelOptions { HttpClient = client });
            var grpcClient = new Sales.SalesClient(channel);

            // Create a test customer
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<SalesDbContext>();
                var customer = new Customer { Id = Guid.NewGuid(), FirstName = "Test", LastName = "Customer", IsActive = true };
                dbContext.Customers.Add(customer);
                await dbContext.SaveChangesAsync();

                // Act
                var createOrderResponse = await CreateTestOrderAsync(grpcClient, customer.Id.ToString());
                Assert.True(createOrderResponse.Success, createOrderResponse.ErrorMessage);

                var response = await grpcClient.GetOrderAsync(new GetOrderRequest { OrderId = createOrderResponse.OrderId });

                // Assert
                Assert.NotNull(response);
                Assert.Equal(createOrderResponse.OrderId, response.Id);
            }
        }
    }
}
