using Domain.User;
using Domain.User.ValueObjects;
using Mapster;

namespace IntervalLearningApi.Controllers.Accounts.DTOs;

public class UserInfoRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<UserId, long>()
            .Map(d => d, s => s.Value);

        config.NewConfig<User, UserInfo>()
            .Map(d => d.FirstName, s => s.UserName.FirstName)
            .Map(d => d.LastName, s => s.UserName.LastName)
            .MapToConstructor(true);
    }
}

public class UserInfo
{
    public string Id { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string Email { get; }

    public UserInfo(long id, string firstName, string lastName, string email)
    {
        Id = id.ToString();
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }
}