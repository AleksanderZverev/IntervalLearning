using Application.Common.Accounts.PasswordService;

namespace Infrastructure.Accounts.Passwords;

public class PasswordsService : IPasswordService
{
    public string GeneratePasswordHash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}