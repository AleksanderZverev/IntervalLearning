using Domain.Common.ValueObjects;

namespace Application.Common.Accounts.PasswordService;

public interface IPasswordService
{
    public string GeneratePasswordHash(string password);
}