using IntervalLearningApi.IntegrationTests.Common.Constants;

namespace IntervalLearningApi.IntegrationTests.Common.Attributes;

public class UseBasePath : Attribute
{
    public string BasePath { get; }

    public UseBasePath(string basePath)
    {
        BasePath = basePath;
    }
}

public class UseDefaultTestUser : Attribute
{
    public string Email => TestUserConstants.Email;
    public string Password => TestUserConstants.Password;
} 