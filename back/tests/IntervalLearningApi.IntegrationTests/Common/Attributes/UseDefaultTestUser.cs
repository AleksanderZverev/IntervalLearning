using IntervalLearningApi.IntegrationTests.Common.Constants;

namespace IntervalLearningApi.IntegrationTests.Common.Attributes;

public class UseDefaultTestUser : Attribute
{
    public string Email => TestConstants.User.Email;
    public string Password => TestConstants.User.Password;
    public string FirstName => TestConstants.User.FirstName;
    public string LastName => TestConstants.User.LastName;
}