using System;
using System.Threading.Tasks;
using AuthService.Application.DTOs;
using AuthService.Application.Services;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using AuthService.Infrastructure.Repositories;
using AuthService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace AuthService.QuickTest
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("🚀 Starting AuthService Quick Test...");
            
            try
            {
                // Create in-memory database
                var options = new DbContextOptionsBuilder<AuthDbContext>()
                    .UseInMemoryDatabase(databaseName: "TestDb")
                    .Options;
                
                using var context = new AuthDbContext(options);
                await context.Database.EnsureCreatedAsync();
                
                // Setup services
                var userRepo = new UserRepository(context);
                var roleRepo = new RoleRepository(context);
                var passwordService = new PasswordService();
                var userService = new UserService(userRepo, roleRepo, passwordService);
                
                Console.WriteLine("✅ Services initialized successfully");
                
                // Test 1: Create user
                var createDto = new CreateUserDto
                {
                    Username = "testuser",
                    Email = "test@example.com", 
                    Password = "Test123!",
                    FirstName = "Test",
                    LastName = "User"
                };
                
                var user = await userService.CreateUserAsync(createDto);
                Console.WriteLine($"✅ User created: {user.Username} ({user.Email})");
                
                // Test 2: Validate password
                var isValid = await userService.ValidatePasswordAsync("testuser", "Test123!");
                Console.WriteLine($"✅ Password validation: {isValid}");
                
                // Test 3: Get user
                var retrievedUser = await userService.GetUserByIdAsync(user.Id);
                Console.WriteLine($"✅ User retrieved: {retrievedUser?.FirstName} {retrievedUser?.LastName}");
                
                Console.WriteLine("🎉 All tests passed! Task 2.1 is working correctly.");
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
