using Domain.Schedule;
using Domain.Schedule.ValueObjects;
using Domain.User.ValueObjects;
using IntervalLearningApi.Controllers.Study.Cards.DTOs;
using IntervalLearningApi.Controllers.Study.Cards.Requests.RememberCard;
using IntervalLearningApi.Controllers.Study.Cards.Requests.StartCards;
using IntervalLearningApi.Controllers.Study.Collections.DTOs;
using IntervalLearningApi.Controllers.Study.Collections.RequestModels.GetNotFinished;
using IntervalLearningApi.Controllers.Study.Collections.RequestModels.GetRepeatCollections;
using IntervalLearningApi.Controllers.Study.Collections.Responses.GetRepeatCollectionsV2;
using IntervalLearningApi.Controllers.Study.RepeatsSchedules.DTOs;
using IntervalLearningApi.Controllers.Study.RepeatsSchedules.Requests.CreateSchedule;
using IntervalLearningApi.IntegrationTests.Common.Services;
using IntervalLearningApi.IntegrationTests.Learning.Common;
using IntervalLearningApi.IntegrationTests.Learning.Scenarios;

namespace IntervalLearningApi.IntegrationTests.Learning;

public class CardAndCollectionsControllerTests : SharedApiTests
{
    public CardAndCollectionsControllerTests(SharedDockerIntervalLearningApiFactory apiFactory) : base(apiFactory)
    {
    }
    
    public static List<ForgottenBehavior> Behaviors = new()
    {
        ForgottenBehavior.StartFromFirstStep,
        ForgottenBehavior.StayOnCurrentStep,
        ForgottenBehavior.MoveToNextStep,
        ForgottenBehavior.MoveToPreviousStep,
    };

    public static IEnumerable<object[]> TestBehaviors = Behaviors.Select(behavior => new object[] { behavior });

    private async Task<RepeatsScheduleDto> CreateTestSchedule(ForgottenBehavior forgottenBehavior)
    {
        var createSchedule = await CreateSchedule(new CreateScheduleRequest()
        {
            Title = "[For tests] Test schedule",
            Description = "Only for tests",
            ForgottenBehavior = (int)forgottenBehavior,
            CardsCountPerPhase = 10,
            Phases = LearningCommons.phasesDuration.Select((d, i) => new CreatePhaseDto()
            {
                Id = (i + 1).ToString(),
                SecondsFromLastPhase = (uint)d.TotalSeconds,
            }).ToList(),
        });

        return createSchedule;
    } 
    
    private async Task<RepeatsScheduleDto> CreateTestScheduleWithRepetitions(ForgottenBehavior forgottenBehavior)
    {
        var createSchedule = await CreateSchedule(new CreateScheduleRequest()
        {
            Title = "[For tests] Test schedule with repetitions",
            Description = "Only for tests",
            ForgottenBehavior = (int)forgottenBehavior,
            CardsCountPerPhase = 10,
            Phases = LearningCommons.phasesDurationWithRepetitions.Select((d, i) => new CreatePhaseDto()
            {
                Id = (i + 1).ToString(),
                SecondsFromLastPhase = (uint)d.TotalSeconds,
            }).ToList(),
        });

        return createSchedule;
    }
    
    private async Task<RepeatsScheduleDto> CreateTestScheduleWithRepetitions_MoveToStartFeature(ForgottenBehavior forgottenBehavior)
    {
        var createSchedule = await CreateSchedule(new CreateScheduleRequest()
        {
            Title = "[For tests] Test schedule with repetitions [Move to start FF]",
            Description = "Only for tests [Move to start FF enabled]",
            ForgottenBehavior = (int)forgottenBehavior,
            CardsCountPerPhase = 12,
            Phases = LearningCommons.phasesDurationWithRepetitionClassic.Select((d, i) => new CreatePhaseDto()
            {
                Id = (i + 1).ToString(),
                SecondsFromLastPhase = (uint)d.TotalSeconds,
            }).ToList(),
            MoveToStartWhenPossibleFeatureFlag = true,
        });

        return createSchedule;
    }
    
    private async Task<RepeatsScheduleDto> CreateTestScheduleWithDuplicateDurations(ForgottenBehavior forgottenBehavior)
    {
        var createSchedule = await CreateSchedule(new CreateScheduleRequest()
        {
            Title = "[For tests] Test schedule with duplicate durations",
            Description = "Only for tests",
            ForgottenBehavior = (int)forgottenBehavior,
            CardsCountPerPhase = 10,
            Phases = LearningCommons.PhasesDurationWithDuplications.Select((d, i) => new CreatePhaseDto()
            {
                Id = (i + 1).ToString(),
                SecondsFromLastPhase = (uint)d.TotalSeconds,
            }).ToList(),
        });

        return createSchedule;
    } 
    
    private async Task<RepeatsScheduleDto> CreateTestScheduleWithStartRepetition(ForgottenBehavior forgottenBehavior)
    {
        var createSchedule = await CreateSchedule(new CreateScheduleRequest()
        {
            Title = "[For tests] Test schedule with duplicate durations",
            Description = "Only for tests",
            ForgottenBehavior = (int)forgottenBehavior,
            CardsCountPerPhase = 10,
            Phases = LearningCommons.phasesDurationWithStartRepetition.Select((d, i) => new CreatePhaseDto()
            {
                Id = (i + 1).ToString(),
                SecondsFromLastPhase = (uint)d.TotalSeconds,
            }).ToList(),
        });

        return createSchedule;
    } 

    private string CardsQuery(string collectionId, string path)
        => AbsoluteQuery(
            ApiRoutes.Cards.GetBasePath(short.Parse(collectionId)),
            path);
    
    private string CollectionsQuery(string path)
        => AbsoluteQuery(
            ApiRoutes.Collections.BasePath,
            path);
    
    private async Task<int> StartCardsWithSkippingRepeatingAsync(
        HttpClient client,
        CollectionDto collection,
        List<CardDto> cards, 
        RepeatsScheduleDto schedule)
    {
        var startCardsResponse = await StartCardsAsync(client, collection, cards, schedule);

        var cardsToRepeat = startCardsResponse.CardMovementInfos
            .Where(m => m.NextRepetitionDate <= DateTime.Now.AddMinutes(5))
            .SelectMany(m => cards.Where(c => m.CardIds.Contains(c.Id)))
            .ToList();
        
        if (cardsToRepeat.Count > 0)
        {
            var repeatingResponse = await RememberCardsAsync(
                client,
                collection,
                cardsToRepeat,
                schedule,
                0,
                LearningScenarios.RememberedWeight);
            
            repeatingResponse.EnsureSuccessStatusCode();
            return repeatingResponse.ToResponseDto<RememberCardResponse>().NextPhaseIndex;
        }
        
        return startCardsResponse.NextPhaseIndex;
    }

    private async Task<StartCardsResponse?> StartCardsAsync(
        HttpClient client,
        CollectionDto collection,
        List<CardDto> cards, 
        RepeatsScheduleDto schedule)
    {
        var startCardsResponse = await client.PostAsJsonAsync(
            CardsQuery(collection.Id, ApiRoutes.Cards.Post_StartCards),
            new StartCardsRequest()
            {
                CardIds = cards.Select(c => short.Parse(c.Id)).ToList(),
                ScheduleId = short.Parse(schedule.Id),
                ScheduleUserId = UserId.Create(long.Parse(schedule.ParentUserId)).Value,
            }
        );
        var startCards = startCardsResponse.ToResponseDto<StartCardsResponse>();
        return startCards;
    }
    
    private async Task RelearnCardAsync(
        HttpClient client,
        CollectionDto collection,
        CardDto card,
        RepeatsScheduleDto? schedule = null)
    {
        var query = new QueryString().Add("cardId", card.Id);

        if (schedule != null)
        {
            query = query.Add("scheduleUserId", schedule.ParentUserId)
                .Add("scheduleId", schedule.Id);
        }
        
        var relearnCardResponse = await client.PatchAsync(
            CardsQuery(collection.Id, ApiRoutes.Cards.Patch_RelearnCard) + query,
            
            new StringContent(string.Empty)
        );
    }
    
    private async Task StopLearningCardAsync(
        HttpClient client,
        CollectionDto collection,
        CardDto card,
        RepeatsScheduleDto schedule)
    {
        await client.DeleteAsync(
            CardsQuery(collection.Id, ApiRoutes.Cards.GetStopRepeatingCardPath(card.Id)) 
            + new QueryString()
                .Add("scheduleUserId", schedule.ParentUserId)
                .Add("scheduleId", schedule.Id)
        );
    }

    private async Task PostponeRepeatingCardAsync(
        HttpClient client,
        CollectionDto collection,
        CardDto card,
        RepeatsScheduleDto schedule, 
        int postponeDays)
    {
        var postponeResult = await client.PatchAsync(
            CardsQuery(collection.Id, ApiRoutes.Cards.GetPostponeRepeatingCardPath(card.Id)) 
            + new QueryString()
                .Add("scheduleUserId", schedule.ParentUserId)
                .Add("scheduleId", schedule.Id)
                .Add("postponeDays", postponeDays.ToString()),
            new StringContent(string.Empty)
        );
    }

    private async Task<List<CardDto>> GetRelearningCardsAsync(
        HttpClient client,
        CollectionDto collection)
    {
        var relearningCardsResponse = await client.GetAsync(
            CardsQuery(collection.Id, ApiRoutes.Cards.Get_GetAllRelearningCards) + new QueryString().Add("count", "100")
        );
        return relearningCardsResponse.ToResponseDto<List<CardDto>>();
    }
    
    public record ScenarioStep(float Weight, int NextPhaseIndexDiff);

    public class ScenarioStepV2
    {
        public LearningScenarios.Weight Weight { get; }

        public LearningScenarios.Move Move { get; }

        public ScenarioStepV2(LearningScenarios.Weight weight, LearningScenarios.Move move)
        {
            Weight = weight;
            Move = move;
        }
    }
    
    public record ScenarioV2(string Name, ForgottenBehavior Behavior, ICollection<ScenarioStepV2> Steps)
    {
        public static List<ScenarioV2> ScenariosFor(ForgottenBehavior behavior, params (string Name, ScenarioStepV2[] Steps)[] scenarioSteps)
        {
            return scenarioSteps.Select(tuple => new ScenarioV2(tuple.Name, behavior, tuple.Steps)).ToList();
        }
            
        public override string ToString()
        {
            return $"[{Name}] {Behavior}: " + string.Join(
                ", ",
                Steps.Select(s => $"{s.Weight} → {s.Move}"));
        }
    }

    public record Scenario(ForgottenBehavior Behavior, List<ScenarioStep> Steps, int ResultStep)
    {
        public override string ToString()
        {
            return $"{Behavior}: " + string.Join(" → ", Steps.Select(s => $"w:{s.Weight}({s.NextPhaseIndexDiff})")) + $" = {ResultStep}";
        }
    };
    
    public static IEnumerable<object[]> TestOnTheStartMoveScenarios = LearningScenarios.TestOnTheStartScenarios.ToMemberData();
    public static IEnumerable<object[]> TestMovingStepBackScenarios = LearningScenarios.ShouldStepStayOrStepBackScenarios.ToMemberData();
    public static IEnumerable<object[]> TestOnTheLastStepScenarios = LearningScenarios.ReachedEndScenarios.ToMemberData();
    public static IEnumerable<object[]> TestOnCompletingScenarios = LearningScenarios.OnCompletingScenarios.ToMemberData();

    [Theory]
    [MemberData(nameof(TestBehaviors))]
    public async Task StartCards_ShouldReturnCorrectNextRepeatInfo(ForgottenBehavior forgottenBehavior)
    {
        //Arrange
        var (client, user) = SharedScope;
        var schedule = await CreateTestSchedule(forgottenBehavior);
        var (collection, cards) = await CreateRandomCardsAsync(10);
        
        //Act
        var startDate = DateTime.UtcNow;
        var startCards = await StartCardsAsync(client, collection, cards, schedule);

        //Assert
        startCards.Should().NotBeNull();
        
        var expectedRepeatDate = startDate.Add(LearningCommons.phasesDuration.First());
        startCards.NextRepeatDate.Should().BeCloseTo(expectedRepeatDate, TimeSpan.FromMinutes(5));
        startCards.NextPhaseIndex.Should().Be(0);
        
        startCards.NextRepeatPhase.Id.Should().Be("1");
        TimeSpan.FromSeconds(startCards.NextRepeatPhase.SecondsFromLastPhase).Should()
            .Be(LearningCommons.phasesDuration.First());

        startCards.CardMovementInfos.Should().NotBeNullOrEmpty().And.HaveCount(1);
        var moveInfos = startCards.CardMovementInfos.Single();
        moveInfos.CardIds.Should().BeEquivalentTo(cards.Select(c => c.Id));
        moveInfos.NextRepetitionDate.Should().BeCloseTo(expectedRepeatDate, TimeSpan.FromMinutes(5));
        moveInfos.CardIds.Should().OnlyHaveUniqueItems();
    }
    
    [Theory]
    [MemberData(nameof(TestBehaviors))]
    public async Task StartCards_ShouldActuallyStartCards(ForgottenBehavior behavior)
    {
        //Arrange
        var (client, user) = SharedScope;
        var schedule = await CreateTestSchedule(behavior);
        var (collection, cards) = await CreateRandomCardsAsync(10);

        //Act
        await StartCardsAsync(client, collection, cards, schedule);
        
        //Assert
        var getRepeatCollectionsResponse = await client.GetAsync(
            CollectionsQuery(ApiRoutes.Collections.GetRepeatCollections));
        var repeatCollections = getRepeatCollectionsResponse.ToResponseDto<RepeatingCollectionResponse>();

        repeatCollections.DateToRepeatingPhases.Should().NotBeNull().And.NotBeEmpty();
        repeatCollections.DateToRepeatingPhases.Keys.Should().HaveCount(1);
        AssertHasDate(LearningCommons.phasesDuration, repeatCollections, schedule.Id, 0);
        AssertHasPhasesAtDate(LearningCommons.phasesDuration, repeatCollections, schedule.Id, 0, 0);
        AssertHasCollectionsAtDatePhase(LearningCommons.phasesDuration, repeatCollections, schedule.Id, 0, 0,
            new CollectionAssertion(collection.Id, cards.Count));
    }

    [Theory(Skip = "Not implemented logic")]
    [MemberData(nameof(TestBehaviors))]
    public async Task StartCards_ShouldDecrementCollectionCounter(ForgottenBehavior behavior)
    {
        //Arrange
        var (client, user) = SharedScope;
        var schedule = await CreateTestSchedule(behavior);
        var cardsCount = 10;
        var (collection, cards) = await CreateRandomCardsAsync(cardsCount);

        //Act
        await StartCardsAsync(client, collection, cards, schedule);
        
        //Assert
        var newCollection = await GetCollectionAsync(collection.Id);
        newCollection.NotStartedCards.Should().Be(0);
    }
    
    [Theory]
    [MemberData(nameof(TestBehaviors))]
    public async Task StartCards_ShouldDeleteRelearningCards(ForgottenBehavior behavior)
    {
        //Arrange
        var (client, user) = SharedScope;
        var schedule = await CreateTestSchedule(behavior);
        var cardsCount = 10;
        var (collection, cards) = await CreateRandomCardsAsync(cardsCount);
        foreach (var card in cards)
        {
            await RelearnCardAsync(client, collection, card);
        }

        //Act
        await StartCardsAsync(client, collection, cards, schedule);
        
        //Assert
        var relearningCards = await GetRelearningCardsAsync(client, collection);
        relearningCards.Should().BeNullOrEmpty();
    }
    
    [Theory(Skip = "Not implemented logic")]
    [MemberData(nameof(TestBehaviors))]
    public async Task StartCards_ShouldIncrementCollectionCounter_WhenNewCardsAdded(ForgottenBehavior behavior)
    {
        //Arrange
        var (client, user) = SharedScope;
        var schedule = await CreateTestSchedule(behavior);
        var cardsCount = 10;
        var (collection, cards) = await CreateRandomCardsAsync(cardsCount);

        //Act
        await StartCardsAsync(client, collection, cards, schedule);
        await AddRandomCardsToCollection(collection.Id, cardsCount);
        
        //Assert
        var newCollection = await GetCollectionAsync(collection.Id);
        newCollection.NotStartedCards.Should().Be((short)cardsCount);
    }
    
    [Theory]
    [MemberData(nameof(TestBehaviors))]
    public async Task GetNotFinished_ShouldReturnCollectionWhenNoCardsStarted(ForgottenBehavior behavior)
    {
        //Arrange
        var (client, user) = SharedScope;
        var schedule = await CreateTestSchedule(behavior);
        var (collection, addedCards) = await CreateRandomCardsAsync(10);

        //Act
        var getNotFinishedCollectionsResponse = await client.GetAsync(
            CollectionsQuery(ApiRoutes.Collections.GetNotFinished)
            + new QueryString()
                .Add("scheduleUserId", schedule.ParentUserId)
                .Add("scheduleId", schedule.Id));
        var notFinishedCollections = getNotFinishedCollectionsResponse.ToResponseDto<GetNotFinishedResponse>();

        //Assert
        notFinishedCollections.Should().NotBeNull();
        notFinishedCollections.TotalCollections.Should().Be(1);
        notFinishedCollections.CanStartCollections.Should().HaveCount(1);
        notFinishedCollections.CanStartCollections.Single().Id.Should().Be(collection.Id);
    }
    
    [Theory]
    [MemberData(nameof(TestBehaviors))]
    public async Task GetNotFinished_ShouldReturnEmptyWhenAllCardsStarted(ForgottenBehavior behavior)
    {
        //Arrange
        var (client, user) = SharedScope;
        var schedule = await CreateTestSchedule(behavior);
        var (collection, addedCards) = await CreateRandomCardsAsync(10);
        await StartCardsAsync(client, collection, addedCards, schedule);

        //Act
        var getNotFinishedCollectionsResponse = await client.GetAsync(
            CollectionsQuery(ApiRoutes.Collections.GetNotFinished)
            + new QueryString()
                .Add("scheduleUserId", schedule.ParentUserId)
                .Add("scheduleId", schedule.Id));
        var notFinishedCollections = getNotFinishedCollectionsResponse.ToResponseDto<GetNotFinishedResponse>();

        //Assert
        notFinishedCollections.Should().NotBeNull();
        notFinishedCollections.TotalCollections.Should().Be(0);
        notFinishedCollections.CanStartCollections.Should().BeEmpty();
    }
    
    [Theory]
    [MemberData(nameof(TestMovingStepBackScenarios))]
    public async Task GetNotFinished_ShouldReturnNotEmptyWhenNewCardsAdded(Scenario scenario)
    {
        //Arrange
        var (client, user) = SharedScope;
        var schedule = await CreateTestSchedule(scenario.Behavior);
        var (collection, preAddedCards) = await CreateRandomCardsAsync(10);
        await StartCardsAsync(client, collection, preAddedCards, schedule);
        var newCards = await AddRandomCardsToCollection(collection.Id, 10);

        //Act
        var getNotFinishedCollectionsResponse = await client.GetAsync(
            CollectionsQuery(ApiRoutes.Collections.GetNotFinished)
            + new QueryString()
                .Add("scheduleUserId", schedule.ParentUserId)
                .Add("scheduleId", schedule.Id));
        var notFinishedCollections = getNotFinishedCollectionsResponse.ToResponseDto<GetNotFinishedResponse>();

        //Assert
        notFinishedCollections.Should().NotBeNull();
        notFinishedCollections.CanStartCollections.Should().ContainSingle();
        
        var canStartCollection = notFinishedCollections.CanStartCollections.Single();
        canStartCollection.Id.Should().Be(collection.Id);
        canStartCollection.NotStartedCards.Should().Be((short)newCards.Count);
    }
    
    [Theory]
    [MemberData(nameof(TestBehaviors))]
    public async Task GetNotFinished_ShouldReturnRelearningCollections(ForgottenBehavior behavior)
    {
        //Arrange
        var (client, user) = SharedScope;
        var schedule = await CreateTestSchedule(behavior);
        var (collection, addedCards) = await CreateRandomCardsAsync(10);
        foreach (var addedCard in addedCards)
            await RelearnCardAsync(client, collection, addedCard);

        //Act
        var getNotFinishedCollectionsResponse = await client.GetAsync(
            CollectionsQuery(ApiRoutes.Collections.GetNotFinished)
            + new QueryString()
                .Add("scheduleUserId", schedule.ParentUserId)
                .Add("scheduleId", schedule.Id));
        var notFinishedCollections = getNotFinishedCollectionsResponse.ToResponseDto<GetNotFinishedResponse>();

        //Assert
        notFinishedCollections.Should().NotBeNull();
        notFinishedCollections.CanRelearnCollections.Should().HaveCount(1);
        notFinishedCollections.CanRelearnCollections.Single().Id.Should().Be(collection.Id);
        notFinishedCollections.CanRelearnCollections.Single().CanRelearnCardCount.Should().Be((short)addedCards.Count);
    }
    
    [Theory]
    [MemberData(nameof(TestBehaviors))]
    public async Task GetNotFinished_ShouldReturnEmptyRelearningCollection_WhenNoCardsAdded(ForgottenBehavior behavior)
    {
        //Arrange
        var (client, user) = SharedScope;
        var schedule = await CreateTestSchedule(behavior);
        var (collection, addedCards) = await CreateRandomCardsAsync(10);

        //Act
        var getNotFinishedCollectionsResponse = await client.GetAsync(
            CollectionsQuery(ApiRoutes.Collections.GetNotFinished)
            + new QueryString()
                .Add("scheduleUserId", schedule.ParentUserId)
                .Add("scheduleId", schedule.Id));
        var notFinishedCollections = getNotFinishedCollectionsResponse.ToResponseDto<GetNotFinishedResponse>();

        //Assert
        notFinishedCollections.Should().NotBeNull();
        notFinishedCollections.CanRelearnCollections.Should().BeNullOrEmpty();
    }
    
    [Fact]
    public async Task GetRepeatingCollections_ShouldReturnStartedCollections()
    {
        //Arrange
        var (client, user) = SharedScope;
        var schedule = await CreateTestSchedule(ForgottenBehavior.MoveToPreviousStep);
        var (firstCollection, firstCards) = await CreateRandomCardsAsync(10);
        var (secondCollection, secondCards) = await CreateRandomCardsAsync(10);
        await StartCardsAsync(client, firstCollection, firstCards, schedule);
        await StartCardsAsync(client, secondCollection, secondCards, schedule);

        //Act
        var getRepeatCollectionsResponse = await client.GetAsync(
            CollectionsQuery(ApiRoutes.Collections.GetRepeatCollections));
        var repeatCollections = getRepeatCollectionsResponse.ToResponseDto<RepeatingCollectionResponse>();

        //Assert
        repeatCollections.Should().NotBeNull();
        repeatCollections.DateToRepeatingPhases.Should().NotBeNullOrEmpty();
        var allRepeatCollections = repeatCollections.DateToRepeatingPhases.Values
            .SelectMany(p => p)
            .SelectMany(p => p.RepeatingCollections)
            .Select(c => c.Collection)
            .ToList();
        var shouldBeCollections = new[] { firstCollection, secondCollection };
        allRepeatCollections.Should().BeEquivalentTo(shouldBeCollections, c =>
        {
            c.Excluding(c => c.CardsCount).Excluding(c => c.IsDeletable);
            c.ForCollection();
            return c;
        });
    }
    
    [Fact]
    public async Task GetRepeatingCollections_ShouldReturnStartedCollectionsUntilSpecifiedDate()
    {
        //Arrange
        var (client, user) = SharedScope;
        var schedule = await CreateTestSchedule(ForgottenBehavior.MoveToPreviousStep);
        var (shouldContainCollection, shouldContainCollectionCards) = await CreateRandomCardsAsync(10);
        var (notIncludedCollection, notIncludedCollectionCards) = await CreateRandomCardsAsync(10);
        await StartCardsAsync(client, shouldContainCollection, shouldContainCollectionCards, schedule);
        await StartCardsAsync(client, notIncludedCollection, notIncludedCollectionCards, schedule);
        await RememberCardsAsync(client, notIncludedCollection, notIncludedCollectionCards, schedule, 0,
            LearningScenarios.RememberedWeight);

        //Act
        var getRepeatCollectionsResponse = await client.GetAsync(
            CollectionsQuery(ApiRoutes.Collections.GetRepeatCollections) +
            new QueryString().Add(
                "untilDate",
                //after starting collection will be at the next day
                DateTime.UtcNow.AddDays(1).AddHours(1).ToString("O")));
        var repeatCollections = getRepeatCollectionsResponse.ToResponseDto<RepeatingCollectionResponse>();

        //Assert
        repeatCollections.Should().NotBeNull();
        repeatCollections.DateToRepeatingPhases.Should().NotBeNullOrEmpty();
        var allRepeatCollections = repeatCollections.DateToRepeatingPhases.Values
            .SelectMany(p => p)
            .SelectMany(p => p.RepeatingCollections)
            .Select(c => c.Collection)
            .ToList();
        var shouldBeCollections = new[] { shouldContainCollection };
        allRepeatCollections.Should().BeEquivalentTo(shouldBeCollections, c =>
        {
            c.Excluding(c => c.CardsCount).Excluding(c => c.IsDeletable);
            c.ForCollection();
            return c;
        });
    }

    [Fact]
    public async Task GetRepeatingCollectionsV2_ShouldReturnStartedCollections()
    {
        //Arrange
        var (client, user) = SharedScope;
        var schedule = await CreateTestSchedule(ForgottenBehavior.MoveToPreviousStep);
        var (firstCollection, firstCards) = await CreateRandomCardsAsync(10);
        var (secondCollection, secondCards) = await CreateRandomCardsAsync(10);
        await StartCardsAsync(client, firstCollection, firstCards, schedule);
        await StartCardsAsync(client, secondCollection, secondCards, schedule);

        //Act
        var getRepeatCollectionsResponse = await client.GetAsync(
            CollectionsQuery(ApiRoutes.Collections.GetRepeatCollectionsV2)
            + new QueryString()
                .Add("scheduleUserId", schedule.ParentUserId)
                .Add("scheduleId", schedule.Id));
        var repeatCollections = getRepeatCollectionsResponse.ToResponseDto<GetRepeatCollectionsResponseV2>();

        //Assert
        repeatCollections.Should().NotBeNull();
        repeatCollections.ScheduleId.Should().Be(schedule.Id);
        repeatCollections.ParentUserId.Should().Be(schedule.ParentUserId);

        repeatCollections.RepeatingCollections.Should().NotBeNullOrEmpty();

        var allRepeatPhaseItems = repeatCollections.RepeatingCollections
            .SelectMany(p => p.RepeatingPhaseItems)
            .ToList();

        var shouldBeCollections = new[] { firstCollection, secondCollection };
        var phase = schedule.Phases.First();

        foreach (var expectedCollection in shouldBeCollections)
        {
            var phaseItem = allRepeatPhaseItems.SingleOrDefault(phaseItem =>
                phaseItem.CollectionId == expectedCollection.Id
                && phaseItem.CollectionUserId == expectedCollection.ParentUserId);

            phaseItem.Should().NotBeNull();
            phaseItem.CardsCount.Should().Be(firstCards.Count);
            phaseItem.PhaseDurationInSeconds.Should().Be(phase.SecondsFromLastPhase);
            phaseItem.IsRepeatable.Should().BeFalse();
        }
    }
    
    [Fact]
    public async Task GetRepeatingCollectionsV2_ShouldReturnStartedCollectionsUntilSpecifiedDate()
    {
        //Arrange
        var (client, user) = SharedScope;
        var schedule = await CreateTestSchedule(ForgottenBehavior.MoveToPreviousStep);
        var (shouldContainCollection, shouldContainCollectionCards) = await CreateRandomCardsAsync(10);
        var (notIncludedCollection, notIncludedCollectionCards) = await CreateRandomCardsAsync(10);
        await StartCardsAsync(client, shouldContainCollection, shouldContainCollectionCards, schedule);
        await StartCardsAsync(client, notIncludedCollection, notIncludedCollectionCards, schedule);
        await RememberCardsAsync(client, notIncludedCollection, notIncludedCollectionCards, schedule, 0,
            LearningScenarios.RememberedWeight);

        //Act
        
        var getRepeatCollectionsResponse = await client.GetAsync(
            CollectionsQuery(ApiRoutes.Collections.GetRepeatCollectionsV2)
            + new QueryString()
                .Add("scheduleUserId", schedule.ParentUserId)
                .Add("scheduleId", schedule.Id)
                .Add(
                "untilDate",
                //after starting collection will be at the next day
                DateTime.UtcNow.AddDays(1).AddHours(1).ToString("O")));
        var repeatCollections = getRepeatCollectionsResponse.ToResponseDto<GetRepeatCollectionsResponseV2>();

        //Assert
        repeatCollections.Should().NotBeNull();
        repeatCollections.ScheduleId.Should().Be(schedule.Id);
        repeatCollections.ParentUserId.Should().Be(schedule.ParentUserId);

        repeatCollections.RepeatingCollections.Should().NotBeNullOrEmpty();

        var allRepeatCollectionIds = repeatCollections.RepeatingCollections
            .SelectMany(p => p.RepeatingPhaseItems)
            .Select(p => p.CollectionUserId + "-" + p.CollectionId)
            .ToList();

        var shouldBeCollections = new[] { shouldContainCollection.ParentUserId + "-" + shouldContainCollection.Id };
        allRepeatCollectionIds.Should().BeEquivalentTo(shouldBeCollections);
    }
    
    [Theory]
    [InlineData("Some comment")]
    [InlineData("Какой-то комментарий по русски")]
    public async Task RememberCard_ShouldSaveComments(string comment)
    {
        //Arrange
        var (client, user) = SharedScope;
        var schedule = await CreateTestSchedule(ForgottenBehavior.MoveToPreviousStep);
        var (collection, preAddedCards) = await CreateRandomCardsAsync(10);
        await StartCardsAsync(client, collection, preAddedCards, schedule);
        
        //Act
        var currentPhaseIndex = 0;
        var rememberResponse = await RememberCardsAsync(
            client,
            collection,
            preAddedCards,
            schedule,
            (short)currentPhaseIndex,
            0f,
            comment);
        
        //ASSERT
        rememberResponse.IsSuccessStatusCode.Should().BeTrue("should fail with comment");
        
        var getRepeatCollectionsResponse = await client.GetAsync(
            CollectionsQuery(ApiRoutes.Collections.GetRepeatCollections));
        var repeatCollections = getRepeatCollectionsResponse.ToResponseDto<RepeatingCollectionResponse>();
        
        var repeatingDates = repeatCollections.DateToRepeatingPhases.Keys.ToList();
        repeatingDates.Should().NotBeNullOrEmpty("should contain one date").And.HaveCount(1, "should contain one date");

        var repeatingDate = repeatingDates.Single();
        var repeatingPhases = repeatCollections.DateToRepeatingPhases[repeatingDate];
        repeatingPhases.Should().NotBeNullOrEmpty("should contain one phase").And.HaveCount(1, "should contain one phase");

        var phase = repeatingPhases.Single();
        var cardsResponse = await client.GetAsync(
            CardsQuery(collection.Id, ApiRoutes.Cards.Get_GetCardsQueue) + new QueryString()
                .Add("scheduleUserId", schedule.ParentUserId)
                .Add("scheduleId", schedule.Id)
                .Add("phaseIndex", phase.PhaseIndex.ToString())
                .Add("date", repeatingDate.ToString("O")));
        var cards = cardsResponse.ToResponseDto<List<CardDto>>();
        var remembers = cards.SelectMany(c => c.Remembers).Where(r => r.PhaseIndex == phase.PhaseIndex).ToList();
        foreach (var remember in remembers)
        {
            remember.Comment.Should().NotBeNullOrEmpty();
            remember.Comment.Should().BeEquivalentTo(comment);
        }
    }
    
    [Theory]
    [MemberData(nameof(TestOnTheStartMoveScenarios))]
    [MemberData(nameof(TestMovingStepBackScenarios))]
    [MemberData(nameof(TestOnTheLastStepScenarios))]
    public async Task RememberCard_ShouldMoveEveryStep(Scenario scenario)
    {
        //Arrange
        var (client, user) = SharedScope;
        var schedule = await CreateTestSchedule(scenario.Behavior);
        var (collection, preAddedCards) = await CreateRandomCardsAsync(10);
        await StartCardsAsync(client, collection, preAddedCards, schedule);
        
        //Act
        var currentPhaseIndex = 0;
        foreach (var step in scenario.Steps)
        {
            var rememberResponse = await RememberCardsAsync(
                client,
                collection,
                preAddedCards,
                schedule, 
                (short)currentPhaseIndex,
                step.Weight);
            
            //Assert
            rememberResponse.IsSuccessStatusCode.Should().BeTrue();
            
            var shouldBeNextPhaseIndex = Math.Max(0, currentPhaseIndex + step.NextPhaseIndexDiff);
            await AssertRememberedCardsMovedToStepWithSpecifiedPhases(
                LearningCommons.phasesDuration,
                client,
                collection,
                preAddedCards,
                schedule,
                (short)shouldBeNextPhaseIndex
            );
            
            currentPhaseIndex = shouldBeNextPhaseIndex;
        }
    }
    
    public static IEnumerable<object[]> TestShouldStepOnRepetitionScenarios = 
        LearningScenarios.ShouldStepOnRepetitionScenarios.ToMemberData();

    public static IEnumerable<object[]> TestShouldMoveAfterRepetition =
        LearningScenarios.ShouldMoveAfterRepetitionCorreclty_IfForgotten.ToMemberData();

    [Theory]
    [MemberData(nameof(TestShouldStepOnRepetitionScenarios))]
    [MemberData(nameof(TestShouldMoveAfterRepetition))]
    public async Task RememberCard_ShouldMoveEveryStep_WhenThereAreRepetitions(Scenario scenario)
    {
        //Arrange
        var (client, user) = SharedScope;
        var schedule = await CreateTestScheduleWithRepetitions(scenario.Behavior);
        var (collection, preAddedCards) = await CreateRandomCardsAsync(10);
        await StartCardsAsync(client, collection, preAddedCards, schedule);

        if (scenario.Behavior == ForgottenBehavior.MoveToPreviousStep)
        {
            
        }
        
        //Act
        var currentPhaseIndex = 0;
        foreach (var step in scenario.Steps)
        {
            var rememberResponse = await RememberCardsAsync(
                client,
                collection,
                preAddedCards,
                schedule, 
                (short)currentPhaseIndex,
                step.Weight);
            
            //Assert
            rememberResponse.IsSuccessStatusCode.Should().BeTrue();
            
            var shouldBeNextPhaseIndex = Math.Max(0, currentPhaseIndex + step.NextPhaseIndexDiff);
            await AssertRememberedCardsMovedToStepWithSpecifiedPhases(
                LearningCommons.phasesDurationWithRepetitions,
                client,
                collection,
                preAddedCards,
                schedule,
                (short)shouldBeNextPhaseIndex
            );
            
            currentPhaseIndex = shouldBeNextPhaseIndex;
        }
    }
    
    public static IEnumerable<object[]> TestShouldStepBackByIntervals = LearningScenarios.ShouldStepBackByIntervals_DuplicatedDurations.ToMemberData();
    [Theory]
    [MemberData(nameof(TestShouldStepBackByIntervals))]
    public async Task RememberCard_ShouldMoveEveryStep_WhenThereAreDuplications(Scenario scenario)
    {
        //Arrange
        var (client, user) = SharedScope;
        var schedule = await CreateTestScheduleWithDuplicateDurations(scenario.Behavior);
        var (collection, preAddedCards) = await CreateRandomCardsAsync(10);
        await StartCardsAsync(client, collection, preAddedCards, schedule);

        //Act
        var currentPhaseIndex = 0;
        foreach (var step in scenario.Steps)
        {
            var rememberResponse = await RememberCardsAsync(
                client,
                collection,
                preAddedCards,
                schedule, 
                (short)currentPhaseIndex,
                step.Weight);
            
            //Assert
            rememberResponse.IsSuccessStatusCode.Should().BeTrue();
            
            var shouldBeNextPhaseIndex = Math.Max(0, currentPhaseIndex + step.NextPhaseIndexDiff);
            await AssertRememberedCardsMovedToStepWithSpecifiedPhases(
                LearningCommons.PhasesDurationWithDuplications,
                client,
                collection,
                preAddedCards,
                schedule,
                (short)shouldBeNextPhaseIndex
            );
            
            currentPhaseIndex = shouldBeNextPhaseIndex;
        }
    }
    
    public static IEnumerable<object[]> TestShouldStepForwardWhenThereIsStartRepetition = LearningScenarios.ShouldStepForwardWhenThereIsStartRepetition.ToMemberData();
    
    [Theory]
    [MemberData(nameof(TestShouldStepForwardWhenThereIsStartRepetition))]
    public async Task RememberCard_ShouldMoveEveryStep_WhenThereIsStartRepetition(Scenario scenario)
    {
        //Arrange
        var (client, user) = SharedScope;
        var schedule = await CreateTestScheduleWithStartRepetition(scenario.Behavior);
        var (collection, preAddedCards) = await CreateRandomCardsAsync(10);
        await StartCardsAsync(client, collection, preAddedCards, schedule);

        //Act
        var currentPhaseIndex = 0;
        foreach (var step in scenario.Steps)
        {
            var rememberResponse = await RememberCardsAsync(
                client,
                collection,
                preAddedCards,
                schedule, 
                (short)currentPhaseIndex,
                step.Weight);
            
            //Assert
            rememberResponse.IsSuccessStatusCode.Should().BeTrue();
            
            var shouldBeNextPhaseIndex = Math.Max(0, currentPhaseIndex + step.NextPhaseIndexDiff);
            await AssertRememberedCardsMovedToStepWithSpecifiedPhases(
                LearningCommons.phasesDurationWithStartRepetition,
                client,
                collection,
                preAddedCards,
                schedule,
                (short)shouldBeNextPhaseIndex
            );
            
            currentPhaseIndex = shouldBeNextPhaseIndex;
        }
    }
    
    [Theory]
    [MemberData(nameof(TestOnTheStartMoveScenarios))]
    [MemberData(nameof(TestMovingStepBackScenarios))]
    [MemberData(nameof(TestOnTheLastStepScenarios))]
    public async Task RememberCard_ShouldReachExpectedResult(Scenario scenario)
    {
        //Arrange
        var (client, user) = SharedScope;
        var schedule = await CreateTestSchedule(scenario.Behavior);
        var (collection, preAddedCards) = await CreateRandomCardsAsync(10);
        await StartCardsAsync(client, collection, preAddedCards, schedule);
        
        //Act
        var currentPhaseIndex = 0;
        foreach (var step in scenario.Steps)
        {
            await RememberCardsAsync(
                client,
                collection,
                preAddedCards,
                schedule, 
                (short)currentPhaseIndex,
                step.Weight);
            
            var shouldBeNextPhaseIndex = Math.Max(0, currentPhaseIndex + step.NextPhaseIndexDiff);
            currentPhaseIndex = shouldBeNextPhaseIndex;
        }

        //Assert
        currentPhaseIndex.Should().Be(scenario.ResultStep - 1);
        await AssertRememberedCardsMovedToStepWithSpecifiedPhases(
            LearningCommons.phasesDuration,
            client,
            collection,
            preAddedCards,
            schedule,
            (short)(scenario.ResultStep - 1)
        );
    }
    
    [Theory]
    [MemberData(nameof(TestOnCompletingScenarios))]
    public async Task RememberCard_ShouldCompleteLearning(Scenario scenario)
    {
        //Arrange
        var (client, user) = SharedScope;
        var schedule = await CreateTestSchedule(scenario.Behavior);
        var (collection, preAddedCards) = await CreateRandomCardsAsync(10);
        await StartCardsAsync(client, collection, preAddedCards, schedule);
        
        //Act
        var currentPhaseIndex = 0;
        foreach (var step in scenario.Steps)
        {
            await RememberCardsAsync(
                client,
                collection,
                preAddedCards,
                schedule, 
                (short)currentPhaseIndex,
                step.Weight);
            
            var shouldBeNextPhaseIndex = Math.Max(0, currentPhaseIndex + step.NextPhaseIndexDiff);
            currentPhaseIndex = shouldBeNextPhaseIndex;
        }

        //Assert
        currentPhaseIndex.Should().Be(scenario.ResultStep - 1);
        
        var getRepeatCollectionsResponse = await client.GetAsync(
            CollectionsQuery(ApiRoutes.Collections.GetRepeatCollections));
        var repeatCollections = getRepeatCollectionsResponse.ToResponseDto<RepeatingCollectionResponse>();

        repeatCollections.Should().NotBeNull();
        repeatCollections.DateToRepeatingPhases.Should().BeEmpty();
    }
    
    [Fact]
    public async Task RememberCard_ShouldReturnCorrectNextRepeatingInfo()
    {
        //Arrange
        var (client, user) = SharedScope;
        var schedule = await CreateTestSchedule(ForgottenBehavior.MoveToPreviousStep);
        var (collection, preAddedCards) = await CreateRandomCardsAsync(12);
        await StartCardsAsync(client, collection, preAddedCards, schedule);
        
        //Act
        var currentPhaseIndex = 0;
        
        await RememberCardsAsync(client, collection, preAddedCards, schedule, (short)currentPhaseIndex,
            LearningScenarios.RememberedWeight);
        currentPhaseIndex++;
        
        var rememberResponse = await client.PatchAsJsonAsync(
            CardsQuery(collection.Id, ApiRoutes.Cards.Path_RememberCard),
            new RememberCardRequest()
            {
                PhaseIndex = (short)currentPhaseIndex,
                ScheduleId = short.Parse(schedule.Id),
                ScheduleUserId = UserId.Create(long.Parse(schedule.ParentUserId)).Value,
                RememberItems = preAddedCards.Select((c, i) => new RememberItemDto()
                {
                    CardId = short.Parse(c.Id),
                    Weight = i switch
                    {
                        < 4 => LearningScenarios.RememberedWeight,
                        >= 4 and < 8 => LearningScenarios.UnknownWeight,
                        >= 8 and < 12 => LearningScenarios.ForgottenWeight,
                    } ,
                    Comment = null,
                }).ToList(),
            });
        var rememberInfo = rememberResponse.ToResponseDto<RememberCardResponse>();

        //Assert
        rememberInfo.Should().NotBeNull();
        var now = DateTime.UtcNow;
        
        var expectedRepeatDate = now.Add(LearningCommons.phasesDuration.First());
        rememberInfo.NextRepeatDate.Should().BeCloseTo(expectedRepeatDate, TimeSpan.FromMinutes(5));
        rememberInfo.CardMovementInfos.Should().NotBeNullOrEmpty();

        var shouldContainDate = LearningCommons.phasesDuration.Take(3).Select(d => now.Add(d)).ToList();
        foreach (var moveInfo in rememberInfo.CardMovementInfos)
        {
            shouldContainDate.Should().ContainSingle(d => d.Date == moveInfo.NextRepetitionDate.Date);
            moveInfo.CardIds.Should().HaveCount(4);
        }

        rememberInfo.CardMovementInfos.SelectMany(c => c.CardIds).Should().OnlyHaveUniqueItems();
    }
    
    [Fact]
    public async Task RelearnCard_ShouldAddRelearningCards()
    {
        //Arrange
        var (client, user) = SharedScope;
        var cardsCount = 10;
        var (collection, cards) = await CreateRandomCardsAsync(cardsCount);
        
        //Act
        foreach (var card in cards)
        {
            await RelearnCardAsync(client, collection, card);
        }

        //Assert
        var relearningCards = await GetRelearningCardsAsync(client, collection);
        relearningCards.Should().NotBeNullOrEmpty();
        relearningCards.Should().BeEquivalentTo(cards, o => o.ForCard());
    }
    
    [Fact]
    public async Task RelearnCard_ShouldStopRepeatingCard()
    {
        //Arrange
        var (client, user) = SharedScope;
        var schedule = await CreateTestSchedule(ForgottenBehavior.MoveToPreviousStep);
        var cardsCount = 12;
        var (collection, cards) = await CreateRandomCardsAsync(cardsCount);
        await StartCardsAsync(client, collection, cards, schedule);
        
        //Act
        foreach (var card in cards)
        {
            await RelearnCardAsync(client, collection, card, schedule);
        }

        //Assert
        var getRepeatCollectionsResponse = await client.GetAsync(
            CollectionsQuery(ApiRoutes.Collections.GetRepeatCollections));
        var repeatCollections = getRepeatCollectionsResponse.ToResponseDto<RepeatingCollectionResponse>();

        repeatCollections.DateToRepeatingPhases.Should().BeNullOrEmpty();
    }
    
    [Theory]
    [MemberData(nameof(TestBehaviors))]
    public async Task StopRepeatingCard_ShouldStopCard(ForgottenBehavior behavior)
    {
        //Arrange
        var (client, user) = SharedScope;
        var cardsCount = 10;
        var (collection, cards) = await CreateRandomCardsAsync(cardsCount);
        var schedule = await CreateTestSchedule(behavior);
        await StartCardsAsync(client, collection, cards, schedule);
        
        //Act
        foreach (var card in cards)
        {
            await StopLearningCardAsync(client, collection, card, schedule);
        }
        
        //Assert
        var getRepeatCollectionsResponse = await client.GetAsync(
            CollectionsQuery(ApiRoutes.Collections.GetRepeatCollections));
        var repeatCollections = getRepeatCollectionsResponse.ToResponseDto<RepeatingCollectionResponse>();

        repeatCollections.DateToRepeatingPhases.Should().BeNullOrEmpty();
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(14)]
    public async Task PostponeRepeatingCard_ShouldPostponeCard(int postponeDays)
    {
        //Arrange
        var (client, user) = SharedScope;
        var cardsCount = 10;
        var (collection, cards) = await CreateRandomCardsAsync(cardsCount);
        var schedule = await CreateTestSchedule(ForgottenBehavior.MoveToNextStep);
        await StartCardsAsync(client, collection, cards, schedule);
        var getOldRepeatCollectionsResponse = await client.GetAsync(
            CollectionsQuery(ApiRoutes.Collections.GetRepeatCollections));
        var oldRepeatCollections = getOldRepeatCollectionsResponse.ToResponseDto<RepeatingCollectionResponse>();
        var oldRepeatingDate = oldRepeatCollections.DateToRepeatingPhases.Keys.First(); 

        //Act
        foreach (var card in cards)
        {
             await PostponeRepeatingCardAsync(client, collection, card, schedule, postponeDays);
        }
        
        //Assert
        var getRepeatCollectionsResponse = await client.GetAsync(
            CollectionsQuery(ApiRoutes.Collections.GetRepeatCollections));
        var repeatCollections = getRepeatCollectionsResponse.ToResponseDto<RepeatingCollectionResponse>();
        repeatCollections.DateToRepeatingPhases.Should().NotBeNullOrEmpty();
        repeatCollections.DateToRepeatingPhases.Keys.Should().HaveCount(1);
        var repeatingDate = repeatCollections.DateToRepeatingPhases.Keys.First();
        repeatingDate.Should().BeCloseTo(DateTime.UtcNow.Date.AddDays(postponeDays), TimeSpan.FromHours(1));
    }

    public static IEnumerable<object[]> ShouldMoveToStartWhenFeatureFlagEnabledScenarios = LearningScenarios.ShouldMoveToStartWhenFeatureFlagEnabled.ToMemberData();
    
    [Theory]
    [MemberData(nameof(ShouldMoveToStartWhenFeatureFlagEnabledScenarios))]
    public async Task RememberCard_ShouldMoveToStart_WhenPersonAnswerIsForgottenAndPhaseDurationNotLong(ScenarioV2 scenario)
    {
        //Arrange
        var schedule = await CreateTestScheduleWithRepetitions_MoveToStartFeature(ForgottenBehavior.MoveToPreviousStep);
        await TestScenarioV2(scenario, schedule);
    }
    
    private async Task TestScenarioV2(ScenarioV2 scenario, RepeatsScheduleDto schedule)
    {
        //Arrange
        var (client, user) = SharedScope;
        var cardsCount = 12;
        var (collection, cards) = await CreateRandomCardsAsync(cardsCount);
        var initialPhaseIndex = await StartCardsWithSkippingRepeatingAsync(client, collection, cards, schedule);
        
        //Act
        var currentPhaseIndex = initialPhaseIndex;
        var stepIndex = 0;
        foreach (var step in scenario.Steps)
        {
             await RememberCardsAsync(
                client,
                collection,
                cards,
                schedule, 
                (short)currentPhaseIndex,
                step.Weight,
                step.Move);
            
            //Assert
            var nextPhaseDiff = step.Move switch
            {
                LearningScenarios.Move.Next => 2,
                LearningScenarios.Move.Previous => -3,
                LearningScenarios.Move.ToRepeating => 1,
                LearningScenarios.Move.ToStart => -99,
                LearningScenarios.Move.Stay => -1,
                _ => throw new NotImplementedException("Unknown move"),
            };
            var shouldBeNextPhaseIndex = Math.Max(initialPhaseIndex, currentPhaseIndex + nextPhaseDiff);
            await AssertRememberedCardsMovedToStep(
                client,
                collection,
                cards,
                schedule,
                (short)shouldBeNextPhaseIndex
            );
            
            currentPhaseIndex = shouldBeNextPhaseIndex;
            stepIndex++;
        }
    }

    private Task AssertRememberedCardsMovedToStep(
        HttpClient client,
        CollectionDto collection,
        List<CardDto> cards,
        RepeatsScheduleDto schedule,
        short shouldMoveToPhaseIndex)
        => AssertRememberedCardsMovedToStepWithSpecifiedPhases(
            schedule.Phases.Select(p => TimeSpan.FromSeconds(p.SecondsFromLastPhase)).ToList(),
            client,
            collection,
            cards,
            schedule,
            shouldMoveToPhaseIndex);

    private async Task AssertRememberedCardsMovedToStepWithSpecifiedPhases(
        IReadOnlyList<TimeSpan> phases, 
        HttpClient client,
        CollectionDto collection,
        List<CardDto> cards,
        RepeatsScheduleDto schedule,
        short shouldMoveToPhaseIndex)
    {
        //ASSERT
        var getRepeatCollectionsResponse = await client.GetAsync(
            CollectionsQuery(ApiRoutes.Collections.GetRepeatCollections));
        var repeatCollections = getRepeatCollectionsResponse.ToResponseDto<RepeatingCollectionResponse>();
        
        AssertHasDate(phases, repeatCollections, schedule.Id, shouldMoveToPhaseIndex);
        AssertHasPhasesAtDate(phases, repeatCollections, schedule.Id, shouldMoveToPhaseIndex, shouldMoveToPhaseIndex);
        AssertHasCollectionsAtDatePhase(
            phases,
            repeatCollections,
            schedule.Id,
            shouldMoveToPhaseIndex,
            shouldMoveToPhaseIndex,
            new CollectionAssertion(collection.Id, cards.Count));
    }
    
    private async Task RememberCardsAsync(
        HttpClient client,
        CollectionDto collection,
        List<CardDto> cards,
        RepeatsScheduleDto schedule,
        short rememberPhaseIndex,
        LearningScenarios.Weight weight,
        LearningScenarios.Move move,
        string? comment = null)
    {
        var rememberedCards = new List<CardDto>();
        var notClearCards = new List<CardDto>();
        var forgottenCards = new List<CardDto>();

        switch (weight)
        {
            case LearningScenarios.Weight.Remember:
            {
                rememberedCards.AddRange(cards);
                break;
            }
            case LearningScenarios.Weight.Unknown:
            {
                notClearCards.AddRange(cards);
                break;
            }
            case LearningScenarios.Weight.Forgotten:
            {
                forgottenCards.AddRange(cards);
                break;
            }
            case LearningScenarios.Weight.Any:
            {
                var partCount = cards.Count / 3;
                rememberedCards.AddRange(cards.Take(partCount));
                notClearCards.AddRange(cards.Skip(partCount).Take(partCount));
                forgottenCards.AddRange(cards.Skip(partCount * 2));
                break;
            }
            case LearningScenarios.Weight.UnknownOrForgotten:
            {
                var partCount = cards.Count / 2;
                notClearCards.AddRange(cards.Take(partCount));
                forgottenCards.AddRange(cards.Skip(partCount));
                break;
            }
            default:
                throw new NotImplementedException("Unknown scenario weight");
        }
        
        HttpResponseMessage? response = null;
        
        if (rememberedCards.Count > 0)
        {
            response = await RememberCardsAsync(
                client,
                collection,
                rememberedCards,
                schedule,
                rememberPhaseIndex,
                LearningScenarios.RememberedWeight,
                comment);
            
            response.EnsureSuccessStatusCode();

            var rememberCardResponse = response.ToResponseDto<RememberCardResponse>();
            await RepeatCardIfNeeded(rememberCardResponse);
        }

        if (notClearCards.Count > 0)
        {
            response = await RememberCardsAsync(
                client,
                collection,
                notClearCards,
                schedule,
                rememberPhaseIndex,
                LearningScenarios.UnknownWeight,
                comment);
            
            response.EnsureSuccessStatusCode();
            
            var notClearCardResponse = response.ToResponseDto<RememberCardResponse>();
            await RepeatCardIfNeeded(notClearCardResponse);
        }
        
        if (forgottenCards.Count > 0)
        {
            response = await RememberCardsAsync(
                client,
                collection,
                forgottenCards,
                schedule,
                rememberPhaseIndex,
                LearningScenarios.ForgottenWeight,
                comment);
            
            response.EnsureSuccessStatusCode();
            
            var forgottenCardResponse = response.ToResponseDto<RememberCardResponse>();
            await RepeatCardIfNeeded(forgottenCardResponse);
        }

        async Task RepeatCardIfNeeded(RememberCardResponse rememberCardResponse)
        {
            if (move == LearningScenarios.Move.ToRepeating)
                return;

            var cardsToRepeat = rememberCardResponse.CardMovementInfos
                .Where(m => m.NextRepetitionDate <= DateTime.Now.AddMinutes(5))
                .SelectMany(m => cards.Where(c => m.CardIds.Contains(c.Id)))
                .ToList();
            
            if (cardsToRepeat.Count == 0)
                return;

            var repeatingResponse = await RememberCardsAsync(
                client,
                collection,
                cardsToRepeat,
                schedule,
                (short)(rememberPhaseIndex + 1),
                LearningScenarios.RememberedWeight);
            
            repeatingResponse.EnsureSuccessStatusCode();
        }
    }

    private async Task<HttpResponseMessage> RememberCardsAsync(
        HttpClient client,
        CollectionDto collection,
        List<CardDto> cards,
        RepeatsScheduleDto schedule,
        short rememberPhaseIndex,
        float rememberWeight,
        string? comment = null)
    {
        return await client.PatchAsJsonAsync(
            CardsQuery(collection.Id, ApiRoutes.Cards.Path_RememberCard),
            new RememberCardRequest()
            {
                PhaseIndex = rememberPhaseIndex,
                ScheduleId = short.Parse(schedule.Id),
                ScheduleUserId = UserId.Create(long.Parse(schedule.ParentUserId)).Value,
                RememberItems = cards.Select(c => new RememberItemDto()
                {
                    CardId = short.Parse(c.Id),
                    Weight = rememberWeight,
                    Comment = comment,
                }).ToList(),
            });
    }

    private static void AssertHasDate(
        IReadOnlyList<TimeSpan> phases, 
        RepeatingCollectionResponse repeatingCollectionResponse,
        string scheduleId,
        params int[] phaseIndexes)
    {
        var dates = repeatingCollectionResponse.DateToRepeatingPhases
            .Where((pair) => pair.Value.Any(p => p.ScheduleId == scheduleId))
            .Select(p => p.Key)
            .ToList();
        var now = DateTime.Now;
        
        foreach (var phaseIndex in phaseIndexes)
        {
            var phase = phases[phaseIndex];
            var date = now.Add(phase);
            
            dates.Should().OnlyContain(d => d.Date == date.Date, "next date is {0}", date.Date);
        }
    }
    
    private static void AssertHasPhasesAtDate(
        IReadOnlyList<TimeSpan> phases, 
        RepeatingCollectionResponse repeatingCollectionResponse,
        string scheduleId,
        int phaseDateIndex,
        params int[] shouldContainPhaseByIndexes)
    {
        var now = DateTime.Now;
        var repeatableDates = repeatingCollectionResponse.DateToRepeatingPhases.Keys.ToList();

        var phaseDate = repeatableDates.FirstOrDefault(d => d.Date == now.Add(phases[phaseDateIndex]).Date);
        var repeatablePhases = repeatingCollectionResponse.DateToRepeatingPhases[phaseDate];

        foreach (var shouldContainPhaseIndex in shouldContainPhaseByIndexes)
        {
            var duration = phases[shouldContainPhaseIndex];
            var repeatablePhase = repeatablePhases
                .Where(p => p.ScheduleId == scheduleId)
                .SingleOrDefault(p => TimeSpan.FromSeconds(p.SecondsFromLastPhase) == duration);

            var because = $"should contain {phaseDate}";
            repeatablePhase.Should().NotBeNull(because);
            repeatablePhase.RepeatingCollections.Should().NotBeEmpty(because);
        }
    }

    private record CollectionAssertion(string CollectionId, int ShouldHasCardsCount);
    
    private static void AssertHasCollectionsAtDatePhase(
        IReadOnlyList<TimeSpan> phases, 
        RepeatingCollectionResponse repeatingCollectionResponse,
        string scheduleId,
        int phaseDateIndex,
        int phaseIndex, 
        params CollectionAssertion[] shouldContainCollections)
    {
        var now = DateTime.Now;
        var repeatableDates = repeatingCollectionResponse.DateToRepeatingPhases.Keys.ToList();

        var repeatableDate = repeatableDates.FirstOrDefault(d => d.Date == now.Add(phases[phaseDateIndex]).Date);
        var repeatablePhases = repeatingCollectionResponse.DateToRepeatingPhases[repeatableDate];

        var phaseDuration = phases[phaseIndex];
        var repeatablePhase = repeatablePhases
            .Where(p => p.ScheduleId == scheduleId)
            .Single(p => TimeSpan.FromSeconds(p.SecondsFromLastPhase) == phaseDuration);
        
        repeatablePhase.Should().NotBeNull();
        repeatablePhase.RepeatingCollections.Should().NotBeEmpty();
        
        foreach (var phaseRepeatingCollection in repeatablePhase.RepeatingCollections)
        {
            var shouldContainCollectionInfo = shouldContainCollections.FirstOrDefault(c =>
                c.CollectionId.ToString() == phaseRepeatingCollection.Collection.Id);
            
            shouldContainCollectionInfo.Should().NotBeNull();
            phaseRepeatingCollection.Collection.Id.Should().Be(shouldContainCollectionInfo.CollectionId.ToString());
            phaseRepeatingCollection.CardsToRepeatCount.Should().Be(shouldContainCollectionInfo.ShouldHasCardsCount);
        }
    }
}