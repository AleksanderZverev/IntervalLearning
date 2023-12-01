using Domain.User;
using Domain.User.ValueObjects;
using FluentResults;

namespace Application.Common.Interfaces.DB.Queries.Accounts.Users;

public interface IUsersQueryRepository
{
    Task<User?> FindByEmail(EmailAddress email);
    Task<User?> Find(UserId userId);
    Task<User?> FindUserByRefreshToken(string refreshToken);
}