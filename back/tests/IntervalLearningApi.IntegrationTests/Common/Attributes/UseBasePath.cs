using IntervalLearningApi.IntegrationTests.Common.Constants;

namespace IntervalLearningApi.IntegrationTests.Common.Attributes;

public class UseBasePath : Attribute
{
    private string basePathPattern;
    
    public string BasePath =>
        basePathPattern
            .Replace("{collectionId}", TestConstants.Collection.Id.ToString());

    public UseBasePath(string basePath)
    {
        basePathPattern = basePath;
    }
}