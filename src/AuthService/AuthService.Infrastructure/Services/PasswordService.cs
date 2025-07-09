using AuthService.Application.Interfaces;
using BCrypt.Net;
using System.Security.Cryptography;

namespace AuthService.Infrastructure.Services;

public class PasswordService : IPasswordService
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        catch
        {
            return false;
        }
    }

    public string GenerateResetToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    public bool ValidateResetToken(string token, DateTime createdAt, TimeSpan validityPeriod)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var expiry = createdAt.Add(validityPeriod);
        return DateTime.UtcNow <= expiry;
    }
}
