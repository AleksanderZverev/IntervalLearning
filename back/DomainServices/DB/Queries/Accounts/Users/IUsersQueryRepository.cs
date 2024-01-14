using Domain.User;
using Domain.User.ValueObjects;

namespace DomainServices.DB.Queries.Accounts.Users;

public interface IUsersQueryRepository
{
    Task<User?> FindByEmail(EmailAddress email);
    Task<User?> Find(UserId userId);
    Task<User?> FindUserByRefreshToken(string refreshToken);
}