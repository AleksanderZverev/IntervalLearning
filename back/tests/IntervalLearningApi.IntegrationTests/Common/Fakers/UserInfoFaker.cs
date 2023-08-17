using Bogus;
using IntervalLearningApi.Models.ByUser;

namespace IntervalLearningApi.IntegrationTests.Common.Fakers;

public class UserInfoFaker : Faker<UserInfo>
{
    public UserInfoFaker()
    {
        CustomInstantiator((f) =>
        {
            return new UserInfo(
                id: f.Random.Long(min: 1),
                firstName: f.Person.FirstName,
                lastName: f.Person.LastName,
                email: f.Person.Email
            );
        });
    }
}