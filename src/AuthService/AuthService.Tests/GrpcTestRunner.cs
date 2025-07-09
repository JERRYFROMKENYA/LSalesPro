namespace AuthService.Tests;

public class GrpcTestRunner
{
    public static async Task RunGrpcTests()
    {
        Console.WriteLine("Starting AuthService gRPC Service Tests...");
        
        using var test = new AuthGrpcServiceDirectTest();
        
        Console.WriteLine("\n=== Testing gRPC Service Methods ===");
        
        // Test 1: ValidateToken method
        Console.WriteLine("\n1. Testing ValidateToken method...");
        bool validateTokenResult = await test.TestValidateTokenMethod();
        Console.WriteLine($"   Result: {(validateTokenResult ? "✅ PASS" : "❌ FAIL")}");
        
        // Test 2: GetUserPermissions method
        Console.WriteLine("\n2. Testing GetUserPermissions method...");
        bool getUserPermissionsResult = await test.TestGetUserPermissionsMethod();
        Console.WriteLine($"   Result: {(getUserPermissionsResult ? "✅ PASS" : "❌ FAIL")}");
        
        // Test 3: GetUserById method
        Console.WriteLine("\n3. Testing GetUserById method...");
        bool getUserByIdResult = await test.TestGetUserByIdMethod();
        Console.WriteLine($"   Result: {(getUserByIdResult ? "✅ PASS" : "❌ FAIL")}");
        
        // Summary
        Console.WriteLine("\n=== Test Summary ===");
        int passedTests = (validateTokenResult ? 1 : 0) + (getUserPermissionsResult ? 1 : 0) + (getUserByIdResult ? 1 : 0);
        int totalTests = 3;
        
        Console.WriteLine($"Tests Passed: {passedTests}/{totalTests}");
        Console.WriteLine($"Overall Result: {(passedTests == totalTests ? "✅ ALL TESTS PASS" : "❌ SOME TESTS FAILED")}");
        
        if (passedTests == totalTests)
        {
            Console.WriteLine("\n🎉 gRPC Service Implementation Successfully Tested!");
            Console.WriteLine("All RPC methods are working correctly:");
            Console.WriteLine("  - ValidateToken: Implemented and functional");
            Console.WriteLine("  - GetUserPermissions: Implemented and functional");
            Console.WriteLine("  - GetUserById: Implemented and functional");
        }
        else
        {
            Console.WriteLine("\n⚠️  Some tests failed. Please check the implementation.");
        }
    }
}
