using System;
using System.Threading.Tasks;
using AuthService.Tests;

class GrpcTestProgram
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🔍 Task 2.4 Authentication gRPC Service Test Runner");
        Console.WriteLine("===================================================");

        await GrpcTestRunner.RunGrpcTests();
    }
}
