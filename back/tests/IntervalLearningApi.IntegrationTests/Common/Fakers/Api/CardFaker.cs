using Bogus;

namespace IntervalLearningApi.IntegrationTests.Common.Fakers.Api;

public class CardFaker : Faker<CreateCardItem>
{
    public CardFaker()
    {
        CustomInstantiator((f) =>
        {
            return new CreateCardItem
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