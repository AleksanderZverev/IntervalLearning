using Bogus;
using DB.Models;
using IntervalLearningApi.IntegrationTests.Common.Constants;

namespace IntervalLearningApi.IntegrationTests.Collections;

public class CollectionEntityFaker : Faker<CollectionEntity>
{
    public CollectionEntityFaker()
    {
        CustomInstantiator((f) =>
        {
            return new CollectionEntity()
            {
                ParentUserId = TestConstants.User.Id,
                // Id = f.Random.Short(),
                Title = f.Lorem.Sentence(wordCount: 4),
                ThemeId = TestConstants.Theme.TestId,
                IsPublic = false,
                IsDefaultBackSide = false,
            };
        });
    }
}