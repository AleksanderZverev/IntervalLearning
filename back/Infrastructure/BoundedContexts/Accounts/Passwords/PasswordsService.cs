using DomainServices.BoundedContext.Accounts.PasswordService;

namespace Infrastructure.BoundedContexts.Accounts.Passwords;

public class PasswordsService : IPasswordService
{
    public string GeneratePasswordHash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool IsPasswordCorrect(string checkingPassword, string correctUserPasswordHash)
    {
        return BCrypt.Net.BCrypt.Verify(checkingPassword, correctUserPasswordHash);
    }
}