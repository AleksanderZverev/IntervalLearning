using DB.Models;
using IntervalLearningApi.Models.ByUser;
using IntervalLearningApi.Models.RepeatsSchedule;
using IntervalLearningApi.Services;

namespace IntervalLearningApi.IntegrationTests.Learning;

public class CardAndCollectionsControllerTests : SharedApiTests
{
    private static IReadOnlyList<TimeSpan> phasesDuration = new List<TimeSpan>()
    {
        TimeSpan.FromDays(1),
            
        TimeSpan.FromDays(3),
            
        TimeSpan.FromDays(7),
            
        TimeSpan.FromDays(14),
        TimeSpan.FromDays(1),
            
        TimeSpan.FromDays(28),
            
        TimeSpan.FromDays(28),
            
        TimeSpan.FromDays(40),
    };
    
    private const float RememberedWeight = 1f;
    private const float ForgottenWeight = 0f;
    private const float UnknownWeight = 0.5f;
    
    public CardAndCollectionsControllerTests(IntervalLearningApiFactory apiFactory) : base(apiFactory)
    {
    }
    
    public static List<ForgottenBehavior> Behaviors = new()
    {
        ForgottenBehavior.StartFromFirstStep,
        ForgottenBehavior.StayOnCurrentStep,
        ForgottenBehavior.MoveToNextStep,
        ForgottenBehavior.MoveToPreviousStep,
    };

    public static IEnumerable<object[]> TestBehaviors =
        Behaviors.Select(behavior => new object[] { behavior });

    private async Task<Schedule> CreateTestSchedule(ForgottenBehavior forgottenBehavior)
    {
        var createSchedule = await CreateSchedule(new RepeatsScheduleController.CreateScheduleRequest()
        {
            Title = "[For tests] Test schedule",
            Description = "Only for tests",
            ForgottenBehavior = (int)forgottenBehavior,
            CardsCountPerPhase = 10,
            Phases = phasesDuration.Select((d, i) => new PhaseInfo()
            {
                Id = (short)(i + 1),
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
    

    private async Task<StartCardResponse?> StartCardsAsync(
        HttpClient client,
        Collection collection,
        List<Card> cards, 
        Schedule schedule)
    {
        var startCardsResponse = await client.PostAsJsonAsync(
            CardsQuery(collection.Id, ApiRoutes.Cards.Post_StartCards),
            new CardsItem()
            {
                CardIds = cards.Select(c => short.Parse(c.Id)).ToList(),
                ScheduleId = short.Parse(schedule.Id),
                ScheduleUserId = long.Parse(schedule.ParentUserId),
            }
        );
        var startCards = startCardsResponse.ToResponseDto<StartCardResponse>();
        return startCards;
    }

    public record ScenarioStep(float Weight, int NextPhaseIndexDiff);

    public record Scenario(ForgottenBehavior Behavior, List<ScenarioStep> Steps, int ResultStep);
    
    public static List<Scenario> TestOnTheStartScenarios = new()
    {
        new Scenario(ForgottenBehavior.MoveToNextStep, new List<ScenarioStep>()
        {
            new(ForgottenWeight, 1),
        }, ResultStep: 2),
        new Scenario(ForgottenBehavior.MoveToPreviousStep, new List<ScenarioStep>()
        {
            new(ForgottenWeight, -1),
        }, ResultStep: 1),
        new Scenario(ForgottenBehavior.StartFromFirstStep, new List<ScenarioStep>()
        {
            new(ForgottenWeight, -99),
        }, ResultStep: 1),
        new Scenario(ForgottenBehavior.StayOnCurrentStep, new List<ScenarioStep>()
        {
            new(ForgottenWeight, 0),
        }, ResultStep: 1),
    };
    
    public static IEnumerable<object[]> TestOnTheStartMoveScenarios = 
        TestOnTheStartScenarios.Select(s => new object[] { s });
    

    public static List<Scenario> MoveScenarios = new()
    {
        new Scenario(ForgottenBehavior.MoveToNextStep, new List<ScenarioStep>()
        {
            new(RememberedWeight, 1),
            new(UnknownWeight, 1),
            new(ForgottenWeight, 1),
        }, ResultStep: 4),
        new Scenario(ForgottenBehavior.MoveToPreviousStep, new List<ScenarioStep>()
        {
            new(RememberedWeight, 1),
            new(UnknownWeight, 1),
            new(ForgottenWeight, -1),
        }, ResultStep: 2),
        new Scenario(ForgottenBehavior.StartFromFirstStep, new List<ScenarioStep>()
        {
            new(RememberedWeight, 1),
            new(UnknownWeight, 1),
            new(ForgottenWeight, -99),
        }, ResultStep: 1),
        new Scenario(ForgottenBehavior.StayOnCurrentStep, new List<ScenarioStep>()
        {
            new(RememberedWeight, 1),
            new(UnknownWeight, 1),
            new(ForgottenWeight, 0),
        }, ResultStep: 3),
    };

    public static IEnumerable<object[]> TestMoveScenarios = 
        MoveScenarios.Select(s => new object[] { s });

    private static List<ScenarioStep> stepsToTheLastStep = phasesDuration
        .Select(_ => new ScenarioStep(RememberedWeight, 1))
        .Skip(1)
        .ToList();

    public static List<Scenario> ReachedEndScenarios = new()
    {
        new Scenario(ForgottenBehavior.MoveToPreviousStep, new List<ScenarioStep>(stepsToTheLastStep)
        {
            new(ForgottenWeight, -1),
        }, ResultStep: phasesDuration.Count - 1),
        new Scenario(ForgottenBehavior.StartFromFirstStep, new List<ScenarioStep>(stepsToTheLastStep)
        {
            new(ForgottenWeight, -99),
        }, ResultStep: 1),
        new Scenario(ForgottenBehavior.StayOnCurrentStep, new List<ScenarioStep>(stepsToTheLastStep)
        {
            new(ForgottenWeight, 0),
        }, ResultStep: phasesDuration.Count),
    };
    
    public static IEnumerable<object[]> TestOnTheLastStepScenarios = 
        ReachedEndScenarios.Select(s => new object[] { s });
    
    public static List<Scenario> OnCompletingScenarios = new()
    {
        new Scenario(ForgottenBehavior.MoveToNextStep, new List<ScenarioStep>(stepsToTheLastStep)
        {
            new(RememberedWeight, 1),
        }, ResultStep: phasesDuration.Count + 1),
        new Scenario(ForgottenBehavior.MoveToPreviousStep, new List<ScenarioStep>(stepsToTheLastStep)
        {
            new(RememberedWeight, 1),
        }, ResultStep: phasesDuration.Count + 1),
        new Scenario(ForgottenBehavior.StartFromFirstStep, new List<ScenarioStep>(stepsToTheLastStep)
        {
            new(RememberedWeight, 1),
        }, ResultStep: phasesDuration.Count + 1),
        new Scenario(ForgottenBehavior.StayOnCurrentStep, new List<ScenarioStep>(stepsToTheLastStep)
        {
            new(RememberedWeight, 1),
        }, ResultStep: phasesDuration.Count + 1),
    };
    
    public static IEnumerable<object[]> TestOnCompletingScenarios = 
        OnCompletingScenarios.Select(s => new object[] { s });

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
        var expectedRepeatDate = startDate.Add(phasesDuration.First());
        startCards.NextRepeatDate.Should().BeCloseTo(
            expectedRepeatDate, TimeSpan.FromMinutes(5));
        startCards.NextPhaseIndex.Should().Be(0);
        TimeSpan.FromSeconds(startCards.NextRepeatPhase.SecondsFromLastPhase).Should()
            .Be(phasesDuration.First());
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
        AssertHasDate(repeatCollections, schedule.Id, 0);
        AssertHasPhasesAtDate(repeatCollections, schedule.Id, 0, 0);
        AssertHasCollectionsAtDatePhase(repeatCollections, schedule.Id, 0, 0,
            new CollectionAssertion(collection.Id, cards.Count));
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

    public async Task Controller_Method_Should()
    {
        
    }

    private async Task AssertRememberedCardsMovedToStep(
        HttpClient client,
        Collection collection,
        List<Card> cards,
        Schedule schedule,
        short shouldMoveToPhaseIndex)
    {
        //ASSERT
        var getRepeatCollectionsResponse = await client.GetAsync(
            CollectionsQuery(ApiRoutes.Collections.GetRepeatCollections));
        var repeatCollections = getRepeatCollectionsResponse.ToResponseDto<RepeatingCollectionResponse>();
        
        AssertHasDate(repeatCollections, schedule.Id, shouldMoveToPhaseIndex);
        AssertHasPhasesAtDate(repeatCollections, schedule.Id, shouldMoveToPhaseIndex, shouldMoveToPhaseIndex);
        AssertHasCollectionsAtDatePhase(
            repeatCollections,
            schedule.Id,
            shouldMoveToPhaseIndex,
            shouldMoveToPhaseIndex,
            new CollectionAssertion(collection.Id, cards.Count));
    }

    private async Task<HttpResponseMessage> RememberCardsAsync(
        HttpClient client,
        Collection collection,
        List<Card> cards,
        Schedule schedule,
        short rememberPhaseIndex,
        float rememberWeight)
    {
        return await client.PatchAsJsonAsync(
            CardsQuery(collection.Id, ApiRoutes.Cards.Path_RememberCard),
            new RememberRequest()
            {
                PhaseIndex = rememberPhaseIndex,
                ScheduleId = short.Parse(schedule.Id),
                ScheduleUserId = long.Parse(schedule.ParentUserId),
                RememberItems = cards.Select(c => new RememberItem()
                {
                    CardId = short.Parse(c.Id),
                    Weight = rememberWeight,
                }).ToList(),
            });
    }

    private void AssertHasDate(
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
            var phase = phasesDuration[phaseIndex];
            var date = now.Add(phase);
            
            dates.Should().OnlyContain(d => d.Date == date.Date);
        }
    }
    
    private void AssertHasPhasesAtDate(
        RepeatingCollectionResponse repeatingCollectionResponse,
        string scheduleId,
        int phaseDateIndex,
        params int[] shouldContainPhaseByIndexes)
    {
        var now = DateTime.Now;
        var repeatableDates = repeatingCollectionResponse.DateToRepeatingPhases.Keys.ToList();

        var phaseDate = repeatableDates.FirstOrDefault(d => d.Date == now.Add(phasesDuration[phaseDateIndex]).Date);
        var repeatablePhases = repeatingCollectionResponse.DateToRepeatingPhases[phaseDate];

        foreach (var shouldContainPhaseIndex in shouldContainPhaseByIndexes)
        {
            var duration = phasesDuration[shouldContainPhaseIndex];
            var repeatablePhase = repeatablePhases
                .Where(p => p.ScheduleId == scheduleId)
                .SingleOrDefault(p => TimeSpan.FromSeconds(p.SecondsFromLastPhase) == duration);
            
            repeatablePhase.Should().NotBeNull();
            repeatablePhase.RepeatingCollections.Should().NotBeEmpty();
        }
    }

    private record CollectionAssertion(string CollectionId, int ShouldHasCardsCount);
    
    private void AssertHasCollectionsAtDatePhase(
        RepeatingCollectionResponse repeatingCollectionResponse,
        string scheduleId,
        int phaseDateIndex,
        int phaseIndex, 
        params CollectionAssertion[] shouldContainCollections)
    {
        var now = DateTime.Now;
        var repeatableDates = repeatingCollectionResponse.DateToRepeatingPhases.Keys.ToList();

        var repeatableDate = repeatableDates.FirstOrDefault(d => d.Date == now.Add(phasesDuration[phaseDateIndex]).Date);
        var repeatablePhases = repeatingCollectionResponse.DateToRepeatingPhases[repeatableDate];

        var phaseDuration = phasesDuration[phaseIndex];
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