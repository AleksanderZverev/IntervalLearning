using System.Net.Http.Json;
using FluentAssertions;
using IntervalLearningApi.Constants;
using IntervalLearningApi.IntegrationTests.Common;
using IntervalLearningApi.IntegrationTests.Common.Attributes;
using IntervalLearningApi.IntegrationTests.Common.Extensions;
using IntervalLearningApi.Models.ByUser;

namespace IntervalLearningApi.IntegrationTests.Collections;

[UseBasePath(ApiRoutes.Collections.BasePath)]
[UseDefaultTestUser]
public class CollectionsControllerTests : BaseTests
{
    [TestCase("New collection")]
    public async Task CreateCollection_ShouldCreate(string collectionName)
    {
        var createResponse = await client.PostAsJsonAsync(ApiRoutes.Collections.Create, new CreateCollectionItem()
        {
            Title = collectionName,
            IsDefaultBackSide = false,
            ThemeId = 1,
        });

        createResponse.IsSuccessStatusCode.Should().BeTrue();
        var response = createResponse.ToResponseDto<Collection>();

        response.Should().NotBeNull();
        response.Title.Should().BeEquivalentTo(collectionName);
    }
}