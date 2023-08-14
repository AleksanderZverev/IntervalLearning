using Bogus;
using IntervalLearningApi.Models.ByUser;

namespace IntervalLearningApi.IntegrationTests.User;

public class UserFaker : Faker<UserInfo>
{
    public UserFaker()
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

public class AuthenticationControllerTests_Utils
{
    
}