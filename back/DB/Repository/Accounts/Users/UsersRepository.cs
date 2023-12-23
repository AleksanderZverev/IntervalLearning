using Application.Common.Interfaces.DB.Repositories;
using Application.Common.Interfaces.DB.Repositories.Accounts.Users;
using DB.Configurations.Account;
using Domain.User;
using Domain.User.ValueObjects;
using FluentResults;

namespace DB.Repository.Accounts.Users;

public class UsersRepository : BaseRepository<User>, IRepository<User, UserId, UserIdParams>
{
    public UsersRepository(ApplicationContext db) : base(db)
    {
    }

    public Result<UserId> GetUniqueId(UserIdParams param)
    {
        var newId = db.GetSequenceNextValue64(UserConfiguration.GetIdSequence());

        if (newId == default)
            return Result.Fail("Failure on getting next sequence value");
        
        return UserId.Create(newId);
    }
}