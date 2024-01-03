namespace DomainServices.Accounts.PasswordService;

public interface IPasswordService
{
    public string GeneratePasswordHash(string password);
    public bool IsPasswordCorrect(string checkingPassword, string correctUserPasswordHash);
}