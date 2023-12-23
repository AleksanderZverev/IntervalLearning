using IntervalLearningApi.Controllers.Study.Cards.DTOs;
using IntervalLearningApi.Controllers.Study.Cards.Requests;
using IntervalLearningApi.Controllers.Study.Collections.DTOs;
using IntervalLearningApi.Controllers.Study.Collections.RequestModels.CreateCollection;
using IntervalLearningApi.Controllers.Study.RepeatsSchedules.DTOs;
using IntervalLearningApi.Controllers.Study.RepeatsSchedules.Requests.CreateSchedule;
using IntervalLearningApi.IntegrationTests.Common.Constants;
using IntervalLearningApi.IntegrationTests.Common.Fakers.Api;

namespace IntervalLearningApi.IntegrationTests.Common.TestScopes;

public class SharedApiTests : BaseApiTests, IClassFixture<SharedDockerIntervalLearningApiFactory>, IAsyncLifetime
{
    public SharedApiTests(SharedDockerIntervalLearningApiFactory apiFactory) : base(apiFactory.SharedFactory)
    {
    }

    protected Scope SharedScope;
    protected HttpClient sharedClient => SharedScope.Client;
    

    public virtual async Task InitializeAsync()
    {
        await base.InitializeAsync();
        SharedScope = await GetRandomUserScope();
    }
    
    protected async Task<List<CollectionDto>?> GetAllCollectionsAsync()
    {
        var getAllResponse = await sharedClient.GetAsync(
            AbsoluteQuery(ApiRoutes.Collections.BasePath, ApiRoutes.Collections.GetAll));
        var allCollections = getAllResponse.ToResponseDto<List<CollectionDto>>();
        return allCollections;
    }
    
    protected async Task<CollectionDto?> GetCollectionAsync(string collectionId)
    {
        var getCollectionResponse = await sharedClient.GetAsync(
            AbsoluteQuery(ApiRoutes.Collections.BasePath, ApiRoutes.Collections.GetCollectionPath(int.Parse(collectionId))));
        var collection = getCollectionResponse.ToResponseDto<CollectionDto>();
        return collection;
    }

    protected async Task<RepeatsScheduleDto> CreateSchedule(CreateScheduleRequest createScheduleRequest)
    {
        var createScheduleResponse = await sharedClient.PostAsJsonAsync(
            AbsoluteQuery(ApiRoutes.Schedule.BasePath, ApiRoutes.Schedule.Post_CreateSchedule),
            createScheduleRequest);

        var schedule = createScheduleResponse.ToResponseDto<RepeatsScheduleDto>();
        return schedule ?? throw new InvalidOperationException();
    }
    
    protected async Task<(RepeatsScheduleDto, RepeatsScheduleDto)> CreateRandomSchedule()
    {
        var scheduleInfo = new ScheduleFaker().Generate();
        var createdSchedule = await CreateSchedule(new CreateScheduleRequest()
        {
            Title = scheduleInfo.Title,
            Description = scheduleInfo.Description,
            ForgottenBehavior = scheduleInfo.ForgottenBehavior,
            CardsCountPerPhase = scheduleInfo.CardsCountPerPhase,
            Phases = scheduleInfo.Phases.Select(p => new CreatePhaseDto()
            {
                Id = p.Id,
                Description = p.Description,
                SecondsFromLastPhase = p.SecondsFromLastPhase,
                ShortDescription = p.ShortDescription,
                IsDefaultValueSide = p.IsDefaultValueSide,
            }).ToList(),
        });
        return (createdSchedule, scheduleInfo);
    }

    protected async Task<List<CardDto>> AddRandomCardsToCollection(string collectionId, int count)
    {
        var cards = new List<CardDto>(count);
        for (var i = 0; i < count; i++)
        {
            var fakeCard = new CardFaker().Generate();
            var createdCard = await CreateCardAsync(
                short.Parse(collectionId),
                new CreateCardRequest()
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

    protected async Task<(CollectionDto Collection, CardDto Card)> CreateRandomCardAsync()
    {
        var (collection, cards) = await CreateRandomCardsAsync(1);
        return (collection, cards.Single());
    }

    protected async Task<(CollectionDto Collection, List<CardDto> Cards)> CreateRandomCardsAsync(int count)
    {
        var collection = await CreateRandomCollectionAsync();
        var cards = await AddRandomCardsToCollection(collection.Id, count);
        return (collection, cards);
    }

    protected async Task<CardDto?> CreateCardAsync(short collectionId, CreateCardRequest card)
    {
        var createCardResponse = await sharedClient.PostAsJsonAsync(
            AbsoluteQuery(ApiRoutes.Cards.GetBasePath(collectionId), ApiRoutes.Cards.Post_CreateCard),
            card);

        if (!createCardResponse.IsSuccessStatusCode)
            return null;
        
        var createdCard = createCardResponse.ToResponseDto<CardDto>();
        return createdCard;
    }

    protected async Task<CollectionDto> CreateRandomCollectionAsync()
        => (await CreateRandomCollectionsAsync(1)).Single(); 
    
    protected async Task<List<CollectionDto>> CreateRandomCollectionsAsync(int count)
    {
        var randomCollections = new CollectionFaker().Generate(count);

        var result = new List<CollectionDto>(count);
        foreach (var randomCollection in randomCollections)
        {
            var createdCollection = await CreateCollectionAsync(new CreateCollectionRequest()
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

    protected Task<CollectionDto?> CreateCollectionAsync(string collectionName)
    {
        return CreateCollectionAsync(new CreateCollectionRequest()
        {
            Title = collectionName,
            IsDefaultBackSide = false,
            ThemeId = TestConstants.Theme.TestId
        });
    }
    

    private async Task<CollectionDto?> CreateCollectionAsync(CreateCollectionRequest createCollectionRequest)
    {
        var createCollectionResponse = await sharedClient.PostAsJsonAsync(
            AbsoluteQuery(ApiRoutes.Collections.BasePath, ApiRoutes.Collections.Create),
            createCollectionRequest);
        if (!createCollectionResponse.IsSuccessStatusCode)
            return null;
        var createdCollection = createCollectionResponse.ToResponseDto<CollectionDto>();
        return createdCollection;
    }

    public virtual async Task DisposeAsync()
    {
        await base.DisposeAsync();
        sharedClient.Dispose();
    }
}