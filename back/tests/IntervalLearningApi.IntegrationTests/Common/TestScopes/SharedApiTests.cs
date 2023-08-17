using IntervalLearningApi.Controllers;
using IntervalLearningApi.IntegrationTests.Common.Constants;
using IntervalLearningApi.IntegrationTests.Common.Fakers.DB;
using IntervalLearningApi.Models.ByUser;
using IntervalLearningApi.Models.RepeatsSchedule;

namespace IntervalLearningApi.IntegrationTests.Common.TestScopes;

[CollectionDefinition("Shared api collection")]
public class SharedApiCollection : ICollectionFixture<IntervalLearningApiFactory>
{
}

[Collection("Shared api collection")]
public class SharedApiTests : BaseApiTests, IAsyncLifetime
{
    public SharedApiTests(IntervalLearningApiFactory apiFactory) : base(apiFactory)
    {
    }

    protected Scope SharedScope;
    protected HttpClient sharedClient => SharedScope.Client;
    

    public virtual async Task InitializeAsync()
    {
        await base.InitializeAsync();
        SharedScope = await GetRandomUserScope();
    }

    protected async Task<Schedule> CreateSchedule(RepeatsScheduleController.CreateScheduleRequest createScheduleRequest)
    {
        var createScheduleResponse = await sharedClient.PostAsJsonAsync(
            AbsoluteQuery(ApiRoutes.Schedule.BasePath, ApiRoutes.Schedule.Post_CreateSchedule),
            createScheduleRequest);

        var schedule = createScheduleResponse.ToResponseDto<Schedule>();
        return schedule ?? throw new InvalidOperationException();
    }

    protected async Task<(Collection Collection, List<Card> Cards)> CreateRandomCardsAsync(int count)
    {
        var collection = await CreateRandomCollectionAsync();
        var cards = await AddRandomCardsToCollection(collection.Id, count);
        return (collection, cards);
    }

    protected async Task<List<Card>> AddRandomCardsToCollection(string collectionId, int count)
    {
        var cards = new List<Card>(count);
        for (var i = 0; i < count; i++)
        {
            var fakeCard = new CardEntityFaker().Generate();
            var createdCard = await CreateCardAsync(
                short.Parse(collectionId),
                new CreateCardItem()
                {
                    BackText = fakeCard.BackSideText,
                    FrontText = fakeCard.FrontSideText,
                    PromptText = fakeCard.PromptText,
                    Description = fakeCard.Description,
                    Examples = fakeCard.Examples,
                });

            if (createdCard == null)
                throw new InvalidOperationException("Unable to create random card");

            cards.Add(createdCard);
        }

        return cards;
    }

    protected async Task<(Collection Collection, Card Card)> CreateRandomCardAsync()
    {
        var (collection, cards) = await CreateRandomCardsAsync(1);
        return (collection, cards.Single());
    }

    protected async Task<Card?> CreateCardAsync(short collectionId, CreateCardItem card)
    {
        var createCardResponse = await sharedClient.PostAsJsonAsync(
            AbsoluteQuery(ApiRoutes.Cards.GetBasePath(collectionId), ApiRoutes.Cards.Post_CreateCard),
            card);
        var createdCard = createCardResponse.ToResponseDto<Card>();
        return createdCard;
    }

    protected async Task<Collection> CreateRandomCollectionAsync()
        => (await CreateRandomCollectionsAsync(1)).Single(); 
    
    protected async Task<List<Collection>> CreateRandomCollectionsAsync(int count)
    {
        var randomCollections = new CollectionEntityFaker().Generate(count);

        var result = new List<Collection>(count);
        foreach (var randomCollection in randomCollections)
        {
            var createdCollection = await CreateCollectionAsync(new CreateCollectionItem()
            {
                Title = randomCollection.Title,
                IsDefaultBackSide = false,
                ThemeId = TestConstants.Theme.TestId,
            });

            if (createdCollection == null)
                throw new InvalidOperationException("Unable to create random collection");

            result.Add(createdCollection);
        }   

        return result;
    }

    protected Task<Collection?> CreateCollectionAsync(string collectionName)
    {
        return CreateCollectionAsync(new CreateCollectionItem()
        {
            Title = collectionName,
            IsDefaultBackSide = false,
            ThemeId = TestConstants.Theme.TestId
        });
    }
    

    private async Task<Collection?> CreateCollectionAsync(CreateCollectionItem createCollectionItem)
    {
        var createCollectionResponse = await sharedClient.PostAsJsonAsync(
            AbsoluteQuery(ApiRoutes.Collections.BasePath, ApiRoutes.Collections.Create),
            createCollectionItem);
        var createdCollection = createCollectionResponse.ToResponseDto<Collection>();
        return createdCollection;
    }

    public virtual async Task DisposeAsync()
    {
        await base.DisposeAsync();
        sharedClient.Dispose();
    }
}