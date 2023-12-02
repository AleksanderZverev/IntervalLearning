using Bogus;
using IntervalLearningApi.Controllers.Study.Collections.RequestModels.CreateCollection;

namespace IntervalLearningApi.IntegrationTests.Common.Fakers.Api;

public class CollectionFaker : Faker<CreateCollectionRequest>
{
    public CollectionFaker()
    {
        CustomInstantiator((f) =>
        {
            return new CreateCollectionRequest()
            {
                Title = f.Lorem.Sentence(wordCount: 4),
                IsDefaultBackSide = false,
            };
        });
    }
}