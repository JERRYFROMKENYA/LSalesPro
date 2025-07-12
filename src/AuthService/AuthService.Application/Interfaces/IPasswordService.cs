namespace AuthService.Application.Interfaces;

public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
    string GenerateResetToken();
    bool ValidateResetToken(string token, DateTime createdAt, TimeSpan validityPeriod);
    bool ValidatePassword(string password);
}
