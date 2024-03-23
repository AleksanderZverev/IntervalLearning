using Domain.User.ValueObjects;

namespace Domain.UnitTests.Common.Bogus.User;

public class FakeUserId : Faker<UserId>
{
    public FakeUserId()
    {
        CustomInstantiator((f) => UserId.Create(f.Random.Long(min: 0)).Value);
    }
}