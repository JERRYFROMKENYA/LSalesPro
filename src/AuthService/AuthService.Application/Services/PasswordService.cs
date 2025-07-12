using AuthService.Application.Interfaces;

namespace AuthService.Application.Services
{
    public class PasswordService : IPasswordService
    {
        public bool ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return false;

            var hasMinimumLength = password.Length >= 8;
            var hasUpperCase = password.Any(char.IsUpper);
            var hasNumber = password.Any(char.IsDigit);
            var hasSpecialCharacter = password.Any(ch => !char.IsLetterOrDigit(ch));

            return hasMinimumLength && hasUpperCase && hasNumber && hasSpecialCharacter;
        }

        public string GenerateResetToken()
        {
            return Guid.NewGuid().ToString();
        }

        public string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        public bool ValidateResetToken(string token, DateTime issuedAt, TimeSpan validityDuration)
        {
            return DateTime.UtcNow - issuedAt <= validityDuration;
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            var hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }
    }
}
