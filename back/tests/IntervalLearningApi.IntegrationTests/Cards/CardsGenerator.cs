using IntervalLearningApi.IntegrationTests.Common.Fakers.Api;
using IntervalLearningApi.Models.ByUser;

namespace IntervalLearningApi.IntegrationTests.Cards;

public class CardsGenerator : LocalApiTests
{
    public CardsGenerator(LocalIntervalLearningApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Theory(Skip = "FOR CUSTOM USE ONLY")]
    [InlineData("test@mail.ru", "test123", "Junk", 100)]
    public async Task GenerateCards(string email, string password, string collectionName, int amount)
    {
        var client = await GetEmptyClient();
        await AuthorizeClientAsync(client, email, password);
        
        var collectionsResponse = await client.GetAsync(
            AbsoluteQuery(ApiRoutes.Collections.BasePath, ApiRoutes.Collections.SearchPrivate) +
            new QueryString()
                .Add("themeId", "1")
                .Add("searchName", collectionName));

        var collections = collectionsResponse.ToResponseDto<List<CollectionDto>>();

        if (collections == null || collections.Count == 0)
        {
            Assert.Fail("No collections found");
        }

        if (collections.Count > 1)
        {
            Assert.Fail("Found more than one collection");
        }

        var targetCollection = collections.Single();

        while (amount > 0)
        {
            var fakeCard = new CardFaker().Generate();
            var randomCard = new CreateCardItem()
            {
                BackText = fakeCard.BackText,
                FrontText = fakeCard.FrontText,
                PromptText = fakeCard.PromptText,
                Description = fakeCard.Description,
                Examples = fakeCard.Examples,
            };
            
            var createCardResponse = await client.PostAsJsonAsync(
                AbsoluteQuery(ApiRoutes.Cards.GetBasePath(short.Parse(targetCollection.Id)), ApiRoutes.Cards.Post_CreateCard),
                randomCard);
            
            if (!createCardResponse.IsSuccessStatusCode)
                Assert.Fail("Something went wrong");
            
            amount--;
        }
    }
}