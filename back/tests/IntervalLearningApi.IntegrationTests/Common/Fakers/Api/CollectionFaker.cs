using Bogus;
using IntervalLearningApi.Models.ByUser;

namespace IntervalLearningApi.IntegrationTests.Common.Fakers.Api;

public class CollectionFaker : Faker<CreateCollectionItem>
{
    public CollectionFaker()
    {
        CustomInstantiator((f) =>
        {
            return new CreateCollectionItem()
            {
                Title = f.Lorem.Sentence(wordCount: 4),
                IsDefaultBackSide = false,
            };
        });
    }
}