using System.Threading.Tasks;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using SalesService.Api;
using Shared.Contracts.Sales;
using Xunit;

namespace SalesService.Tests
{
    public class SalesGrpcServiceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public SalesGrpcServiceIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                // Optionally override dependencies here for isolation/mocking
            });
        }

        [Fact]
        public async Task GetOrderById_ReturnsExpectedResult()
        {
            // Arrange
            var client = _factory.CreateDefaultClient();
            var channel = GrpcChannel.ForAddress(client.BaseAddress!, new GrpcChannelOptions { HttpClient = client });
            var grpcClient = new Sales.SalesClient(channel);

            // TODO: Insert a test order into the database or mock repository here if needed
            var testOrderId = "00000000-0000-0000-0000-000000000001"; // Replace with a real or seeded test order ID

            // Act
            var response = await grpcClient.GetOrderByIdAsync(new GetOrderByIdRequest { OrderId = testOrderId });

            // Assert
            Assert.NotNull(response);
            // TODO: Add more assertions based on expected response fields, e.g. response.OrderId, response.Status, etc.
        }
    }
}
