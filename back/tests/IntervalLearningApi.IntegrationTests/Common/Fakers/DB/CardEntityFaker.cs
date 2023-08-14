using Bogus;
using DB.Models;
using IntervalLearningApi.IntegrationTests.Common.Constants;

namespace IntervalLearningApi.IntegrationTests.Collections;

public class CardEntityFaker : Faker<CardEntity>
{
    public CardEntityFaker()
    {
        CustomInstantiator((f) =>
        {
            return new CardEntity()
            {
                ParentUserId = TestConstants.User.Id,
                // Id =
                FrontSideText = f.Lorem.Word(),
                BackSideText = f.Lorem.Word(),
                PromptText = f.Lorem.Word(),
                Description = f.Lorem.Sentence(wordCount: 20),
                Examples = Enumerable.Range(0, 4).Select((_) => f.Lorem.Sentence(7)).ToList(),
            };
        });
    }
}