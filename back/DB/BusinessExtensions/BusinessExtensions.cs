using DB.Configurations.Account;
using Domain.User.ValueObjects;
using FluentResults;

namespace DB.BusinessExtensions;

public static class BusinessExtensions
{
    public static Result<UserId> GetUniqueUserId(this ApplicationContext db)
    {
        var newId = db.GetSequenceNextValue64(UserConfiguration.GetIdSequence());

        if (newId == default)
            return Result.Fail("Failure on getting next sequence value");
        
        return UserId.Create(newId);
    }
}