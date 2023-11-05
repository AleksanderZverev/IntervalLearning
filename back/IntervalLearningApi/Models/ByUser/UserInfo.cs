using DB.Models;
using Mapster;

namespace IntervalLearningApi.Models.ByUser;

public class UserInfoRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<UserEntity, UserInfo>()
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