using Domain.Schedule.ValueObjects;
using Domain.User.ValueObjects;
using IntervalLearningApi.Controllers.Study.Cards.DTOs;
using IntervalLearningApi.Controllers.Study.Cards.Requests.RememberCard;
using IntervalLearningApi.Controllers.Study.Cards.Requests.StartCards;
using IntervalLearningApi.Controllers.Study.Collections.DTOs;
using IntervalLearningApi.Controllers.Study.Collections.RequestModels.GetNotFinished;
using IntervalLearningApi.Controllers.Study.Collections.RequestModels.GetRepeatCollections;
using IntervalLearningApi.Controllers.Study.RepeatsSchedules.DTOs;
using IntervalLearningApi.Controllers.Study.RepeatsSchedules.Requests.CreateSchedule;
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

    private string CardsQuery(string collectionId, string path)
        => AbsoluteQuery(
            ApiRoutes.Cards.GetBasePath(short.Parse(collectionId)),
            path);
    
    private string CollectionsQuery(string path)
        => AbsoluteQuery(
            ApiRoutes.Collections.BasePath,
            path);
    

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
        CardDto card)
    {
        var relearnCardResponse = await client.PatchAsync(
            CardsQuery(collection.Id, ApiRoutes.Cards.Patch_RelearnCard) + new QueryString().Add("cardId", card.Id), 
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

    public record Scenario(ForgottenBehavior Behavior, List<ScenarioStep> Steps, int ResultStep)
    {
        public override string ToString()
        {
            return $"{Behavior}: " + string.Join(" → ", Steps.Select(s => $"w:{s.Weight}({s.NextPhaseIndexDiff})")) + $" = {ResultStep}";
        }
    };
    
    public static IEnumerable<object[]> TestOnTheStartMoveScenarios = LearningScenarios.TestOnTheStartScenarios.ToMemberData();
    public static IEnumerable<object[]> TestMoveScenarios = LearningScenarios.MoveScenarios.ToMemberData();
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
        startCards.NextRepeatDate.Should().BeCloseTo(
            expectedRepeatDate, TimeSpan.FromMinutes(5));
        startCards.NextPhaseIndex.Should().Be(0);
        TimeSpan.FromSeconds(startCards.NextRepeatPhase.SecondsFromLastPhase).Should()
            .Be(LearningCommons.phasesDuration.First());
        startCards.NextRepeatPhase.Id.Should().Be("1");
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
    [MemberData(nameof(TestMoveScenarios))]
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
    
    [Theory]
    [MemberData(nameof(TestOnTheStartMoveScenarios))]
    [MemberData(nameof(TestMoveScenarios))]
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
            await AssertRememberedCardsMovedToStep(
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
        LearningScenarios.ShouldMoveAfterRepetition.ToMemberData();

    [Theory]
    [MemberData(nameof(TestShouldStepOnRepetitionScenarios))]
    [MemberData(nameof(TestShouldMoveAfterRepetition))]
    public async Task RememberCard_ShouldMoveEveryStep_AfterRepetition(Scenario scenario)
    {
        //Arrange
        var (client, user) = SharedScope;
        var schedule = await CreateTestScheduleWithRepetitions(scenario.Behavior);
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
            await AssertRememberedCardsMovedToStep(
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
    
    [Theory]
    [MemberData(nameof(TestOnTheStartMoveScenarios))]
    [MemberData(nameof(TestMoveScenarios))]
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
        await AssertRememberedCardsMovedToStep(
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

    public async Task Controller_Method_Should()
    {
        
    }

    private async Task AssertRememberedCardsMovedToStep(
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

    private async Task<HttpResponseMessage> RememberCardsAsync(
        HttpClient client,
        CollectionDto collection,
        List<CardDto> cards,
        RepeatsScheduleDto schedule,
        short rememberPhaseIndex,
        float rememberWeight)
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