namespace IntervalLearningApi.IntegrationTests.Common.Attributes;

public class UseBasePath : Attribute
{
    public string BasePath { get; }

    public UseBasePath(string basePath)
    {
        BasePath = basePath;
    }
}