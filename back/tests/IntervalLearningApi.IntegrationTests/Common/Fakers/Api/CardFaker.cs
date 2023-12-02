using Bogus;
using IntervalLearningApi.Controllers.Study.Cards.Requests;

namespace IntervalLearningApi.IntegrationTests.Common.Fakers.Api;

public class CardFaker : Faker<CreateCardRequest>
{
    public CardFaker()
    {
        CustomInstantiator((f) =>
        {
            return new CreateCardRequest
            {
                FrontText = f.Lorem.Word(),
                BackText = f.Lorem.Word(),
                PromptText = f.Lorem.Word(),
                Description = f.Lorem.Sentence(wordCount: 20),
                Examples = Enumerable.Range(0, 4).Select((_) => f.Lorem.Sentence(7)).ToList(),
            };
        });
    }
}