using Microsoft.AspNetCore.Hosting;
// using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using System.Threading.Tasks;
// using Xunit;

namespace ApiGateway.Tests
{
    public class RoutingTests
    {
        // private readonly HttpClient _client;
        //
        // public RoutingTests()
        // {
        //     var host = new HostBuilder()
        //         .ConfigureWebHost(webBuilder =>
        //         {
        //             webBuilder.UseTestServer();
        //             webBuilder.UseStartup<ApiGateway.Program>();
        //         })
        //         .Start();
        //
        //     _client = host.GetTestClient();
        // }
        //
        // [Fact]
        // public async Task AuthService_Route_Should_Return_Success()
        // {
        //     var response = await _client.GetAsync("/api/v1/auth/user");
        //     Assert.True(response.IsSuccessStatusCode);
        // }
        //
        // [Fact]
        // public async Task InventoryService_Route_Should_Return_Success()
        // {
        //     var response = await _client.GetAsync("/api/inventory/products");
        //     Assert.True(response.IsSuccessStatusCode);
        // }
        //
        // [Fact]
        // public async Task SalesService_Route_Should_Return_Success()
        // {
        //     var response = await _client.GetAsync("/api/sales/orders");
        //     Assert.True(response.IsSuccessStatusCode);
        // }
    }
}
