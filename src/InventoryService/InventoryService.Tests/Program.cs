using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InventoryService.Tests;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        Console.WriteLine("=== InventoryService gRPC Tests ===");
        
        try
        {
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            
            // Run the tests
            var result = await GrpcTestRunner.RunTests(services.BuildServiceProvider());
            
            Console.WriteLine($"\nFinal test result: {(result ? "PASSED ✅" : "FAILED ❌")}");
            return result ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unhandled exception: {ex}");
            return 1;
        }
    }
}
