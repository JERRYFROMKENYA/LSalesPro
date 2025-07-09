using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InventoryService.Tests;

public class GrpcTestRunner
{
    public static async Task<bool> RunTests(IServiceProvider serviceProvider)
    {
        Console.WriteLine("Starting gRPC service tests...");

        var logger = serviceProvider.GetRequiredService<ILogger<GrpcTestRunner>>();
        
        try
        {
            // Run the inventory gRPC service tests
            using (var scope = serviceProvider.CreateScope())
            {
                var test = ActivatorUtilities.CreateInstance<InventoryGrpcServiceTest>(scope.ServiceProvider);
                
                Console.WriteLine("\nTesting GetProductDetails...");
                var getProductDetailsResult = await test.TestGetProductDetailsMethod();
                Console.WriteLine($"GetProductDetails test result: {(getProductDetailsResult ? "PASSED ✅" : "FAILED ❌")}");
                
                // Add more test methods here as they are implemented
                
                // Return overall success (all tests passed)
                return getProductDetailsResult;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error running gRPC tests");
            Console.WriteLine($"Test execution error: {ex.Message}");
            return false;
        }
    }
}
