using IntervalLearningApi.IntegrationTests.Common.Constants;
using IntervalLearningApi.IntegrationTests.Common.Fakers.Api;
using IntervalLearningApi.Models.ByUser;
using IntervalLearningApi.Models.RepeatsSchedule;

namespace IntervalLearningApi.IntegrationTests.Common.TestScopes;

public class SharedApiTests : BaseApiTests, IClassFixture<SharedIntervalLearningApiFactory>, IAsyncLifetime
{
    public SharedApiTests(SharedIntervalLearningApiFactory apiFactory) : base(apiFactory.SharedFactory)
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
            var fakeCard = new CardFaker().Generate();
            var createdCard = await CreateCardAsync(
                short.Parse(collectionId),
                new CreateCardItem()
                {
                    BackText = fakeCard.BackText,
                    FrontText = fakeCard.FrontText,
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

        if (!createCardResponse.IsSuccessStatusCode)
            return null;
        
        var createdCard = createCardResponse.ToResponseDto<Card>();
        return createdCard;
    }

    protected async Task<Collection> CreateRandomCollectionAsync()
        => (await CreateRandomCollectionsAsync(1)).Single(); 
    
    protected async Task<List<Collection>> CreateRandomCollectionsAsync(int count)
    {
        var randomCollections = new CollectionFaker().Generate(count);

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
        if (!createCollectionResponse.IsSuccessStatusCode)
            return null;
        var createdCollection = createCollectionResponse.ToResponseDto<Collection>();
        return createdCollection;
    }

    public virtual async Task DisposeAsync()
    {
        await base.DisposeAsync();
        sharedClient.Dispose();
    }
}