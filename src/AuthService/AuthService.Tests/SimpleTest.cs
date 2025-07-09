using AuthService.Infrastructure.Data;
using AuthService.Infrastructure.Repositories;
using AuthService.Infrastructure.Services;
using AuthService.Application.Services;
using AuthService.Application.DTOs;
using Microsoft.EntityFrameworkCore;

class SimpleTest
{
    static async Task RunSimpleTestAsync(string[] args)
    {
        Console.WriteLine("🚀 AuthService Task 2.1 - Simple Verification Test");
        Console.WriteLine("=" + new string('=', 60));

        try
        {
            // 1. Test Database Context
            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseInMemoryDatabase(databaseName: "SimpleTestDb")
                .Options;
            
            using var context = new AuthDbContext(options);
            context.Database.EnsureCreated();
            Console.WriteLine("✅ Database context created and seeded");

            // 2. Test Password Service
            var passwordService = new PasswordService();
            var hashedPassword = passwordService.HashPassword("TestPassword123!");
            var isValid = passwordService.VerifyPassword("TestPassword123!", hashedPassword);
            
            if (isValid)
                Console.WriteLine("✅ Password hashing and verification working");
            else
                throw new Exception("Password verification failed");

            // 3. Test Basic User Repository Operations
            var userRepo = new UserRepository(context);
            var roleRepo = new RoleRepository(context);

            // Check if seed data exists
            var roles = await roleRepo.GetAllAsync();
            Console.WriteLine($"✅ Seed data loaded: {roles.Count()} roles found");

            // 4. Test Simple User Creation (without roles)
            var createDto = new CreateUserDto
            {
                Username = "testuser",
                Email = "test@example.com", 
                Password = "TestPassword123!",
                FirstName = "Test",
                LastName = "User",
                RoleIds = new List<Guid>() // Empty roles list
            };

            var userService = new UserService(userRepo, roleRepo, passwordService);
            var createdUser = await userService.CreateUserAsync(createDto);
            
            if (createdUser != null && createdUser.Username == "testuser")
                Console.WriteLine("✅ User created successfully");
            else
                throw new Exception("User creation failed");

            // 5. Test User Retrieval
            var retrievedUser = await userService.GetUserByIdAsync(createdUser.Id);
            if (retrievedUser != null && retrievedUser.Email == "test@example.com")
                Console.WriteLine("✅ User retrieval working");
            else
                throw new Exception("User retrieval failed");

            // 6. Test Password Validation
            var passwordValid = await userService.ValidatePasswordAsync("testuser", "TestPassword123!");
            if (passwordValid)
                Console.WriteLine("✅ Password validation working");
            else
                throw new Exception("Password validation failed");

            Console.WriteLine("=" + new string('=', 60));
            Console.WriteLine("🎉 ALL BASIC TESTS PASSED!");
            Console.WriteLine("✅ Task 2.1 Core Authentication Models & Database - VERIFIED");
            Console.WriteLine("📋 Ready to proceed to Task 2.2: JWT Token Service");
        }
        catch (Exception ex)
        {
            Console.WriteLine("=" + new string('=', 60));
            Console.WriteLine("❌ TEST FAILED!");
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
    }
}
