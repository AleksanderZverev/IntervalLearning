using System.Net.Http.Json;
using DB.Models;
using FluentAssertions;
using FluentAssertions.Extensions;
using IntervalLearningApi.Constants;
using IntervalLearningApi.Controllers;
using IntervalLearningApi.IntegrationTests.Collections;
using IntervalLearningApi.IntegrationTests.Common;
using IntervalLearningApi.IntegrationTests.Common.Attributes;
using IntervalLearningApi.IntegrationTests.Common.Constants;
using IntervalLearningApi.IntegrationTests.Common.Extensions;
using IntervalLearningApi.Models.ByUser;
using IntervalLearningApi.Models.RepeatsSchedule;
using IntervalLearningApi.Services;
using Microsoft.AspNetCore.Http;

namespace IntervalLearningApi.IntegrationTests.Learning;

[UseDefaultTestUser]
public class CardAndCollectionsControllerTests : BaseTests
{
    private IReadOnlyList<Card> AllTestCards;
    private List<Card> AddedCards = new();
    private IReadOnlyList<Collection> TestCollections;
    private IReadOnlyList<Schedule> Schedules;

    private List<TimeSpan> phasesDuration;
    private const int MaxCard = 30;

    private const float RememberedWeight = 1f;
    private const float ForgottenWeight = 0f;
    private const float UnknownWeight = 0.5f;
    
    public static List<ForgottenBehavior> Behaviors = new()
    {
        ForgottenBehavior.StartFromFirstStep,
        ForgottenBehavior.StayOnCurrentStep,
        ForgottenBehavior.MoveToNextStep,
        ForgottenBehavior.MoveToPreviousStep,
    };

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        var fistCollection = await AddRandomCollection();
        var secondCollection = await AddRandomCollection();
        TestCollections = new List<Collection>()
        {
            fistCollection,
            secondCollection
        };
        
        var addCards = await AddRandomCardsToCollection(TestConstants.Collection.Id);

        AllTestCards = addCards.ToList();

        phasesDuration = new List<TimeSpan>()
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

        var allSchedules = new List<Schedule>();
        foreach (var behavior in Behaviors)
        {
            var schedule = await CreateTestSchedule(behavior);
            if (schedule == null)
                throw new InvalidOperationException();
            allSchedules.Add(schedule);
        }
        
        Schedules = allSchedules.AsReadOnly();
    }

    private async Task<List<Card>> AddRandomCardsToCollection(short collectionId)
    {
        var cards = new CardEntityFaker().Generate(MaxCard);

        var addCards = new List<Card>();

        foreach (var card in cards)
        {
            var createdCard = await CreateCardAsync(collectionId, new CreateCardItem()
            {
                BackText = card.BackSideText,
                FrontText = card.FrontSideText,
                PromptText = card.PromptText,
                Description = card.Description,
                Examples = card.Examples,
            });

            if (createdCard == null)
                throw new InvalidOperationException();

            addCards.Add(createdCard);
        }

        return addCards;
    }

    private async Task<Collection> AddRandomCollection()
    {
        var collection = new CollectionEntityFaker().Generate();
        var addedCollection = await CreateCollectionAsync(new CreateCollectionItem()
        {
            Title = collection.Title,
            IsDefaultBackSide = false,
            ThemeId = TestConstants.Theme.TestId,
        });
        
        return addedCollection ?? throw new InvalidOperationException();
    }

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

    private string CardsQuery(string path)
        => AbsoluteQuery(
            ApiRoutes.Cards.GetBasePath(TestConstants.Collection.Id),
            path);
    
    private string CollectionsQuery(string path)
        => AbsoluteQuery(
            ApiRoutes.Collections.BasePath,
            path);

    private Schedule GetSchedule(ForgottenBehavior forgottenBehavior)
        => Schedules.Single(s => s.ForgottenBehavior == (int)forgottenBehavior);

    [Order(1)]
    [TestCaseSource(nameof(Behaviors))]
    public async Task CardsController_StartCards_ShouldAddRememberEntity(ForgottenBehavior forgottenBehavior)
    {
        var schedule = GetSchedule(forgottenBehavior);
        
        var startDate = DateTime.UtcNow;
        var startCardsResponse = await client.PostAsJsonAsync(
            CardsQuery(ApiRoutes.Cards.Post_StartCards),
            new CardsItem()
            {
                CardIds = AllTestCards.Select(c => short.Parse(c.Id)).ToList(),
                ScheduleId = short.Parse(schedule.Id),
                ScheduleUserId = long.Parse(schedule.ParentUserId),
            }
        );
        var startCards = startCardsResponse.ToResponseDto<StartCardResponse>();
        
        startCards.Should().NotBeNull();
        var expectedRepeatDate = startDate.Add(phasesDuration.First());
        startCards.NextRepeatDate.Should().BeCloseTo(
            expectedRepeatDate, TimeSpan.FromMinutes(5));
        startCards.NextPhaseIndex.Should().Be(0);
        TimeSpan.FromSeconds(startCards.NextRepeatPhase.SecondsFromLastPhase).Should()
            .Be(phasesDuration.First());
        startCards.NextRepeatPhase.Id.Should().Be("1");
    }

    public record ScenarioStep(float Weight, int NextPhaseIndexDiff);

    public record Scenario(ForgottenBehavior Behavior, List<ScenarioStep> Steps);

    public static object[] Scenarios =
    {
        new Scenario(ForgottenBehavior.MoveToNextStep, new List<ScenarioStep>()
        {
            new(RememberedWeight, 1),
            new(ForgottenWeight, 1),
            new(UnknownWeight, 1)
        }),
        new Scenario(ForgottenBehavior.MoveToPreviousStep, new List<ScenarioStep>()
        {
            new(RememberedWeight, 1),
            new(ForgottenWeight, -1),
            new(UnknownWeight, 1),
        }),
        new Scenario(ForgottenBehavior.StartFromFirstStep, new List<ScenarioStep>()
        {
            new(RememberedWeight, 1),
            new(ForgottenWeight, -99),
            new(UnknownWeight, 1),
        }),
        new Scenario(ForgottenBehavior.StayOnCurrentStep, new List<ScenarioStep>()
        {
            new(RememberedWeight, 1),
            new(ForgottenWeight, 0),
            new(UnknownWeight, 1),
        }),
    };
    
    [Order(2)]
    [TestCaseSource(nameof(Scenarios))]
    public async Task CollectionsController_GetRepeatCollections_ShouldReturnAllCollectionsAfterStartingCards(Scenario scenario)
    {
        var schedule = GetSchedule(scenario.Behavior);
        
        var getRepeatCollectionsResponse = await client.GetAsync(
            CollectionsQuery(ApiRoutes.Collections.GetRepeatCollections));
        var repeatCollections = getRepeatCollectionsResponse.ToResponseDto<RepeatingCollectionResponse>();

        repeatCollections.DateToRepeatingPhases.Should().NotBeNull().And.NotBeEmpty();
        repeatCollections.DateToRepeatingPhases.Keys.Should().HaveCount(1);
        AssertHasDate(repeatCollections, schedule.Id, 0);
        AssertHasPhasesAtDate(repeatCollections, schedule.Id, 0, 0);
        AssertHasCollectionsAtDatePhase(repeatCollections, schedule.Id, 0, 0,
            new CollectionAssertion(TestConstants.Collection.Id, AllTestCards.Count));
    }
    
    [Order(2)]
    [TestCaseSource(nameof(Scenarios))]
    public async Task CollectionsController_GetNotFinished_ShouldReturnEmptyWhenNoCardsLeft(Scenario scenario)
    {
        var schedule = GetSchedule(scenario.Behavior);
        
        var getNotFinishedCollectionsResponse = await client.GetAsync(
            CollectionsQuery(ApiRoutes.Collections.GetNotFinished)
            + new QueryString()
                .Add("scheduleUserId", schedule.ParentUserId)
                .Add("scheduleId", schedule.Id));
        var notFinishedCollections = getNotFinishedCollectionsResponse.ToResponseDto<GetNotFinishedResponse>();

        notFinishedCollections.Should().NotBeNull();
        notFinishedCollections.TotalCollections.Should().Be(0);
        notFinishedCollections.CanStartCollections.Should().BeEmpty();
    }
    
    [Order(3)]
    [TestCaseSource(nameof(Scenarios))]
    public async Task CollectionsController_GetNotFinished_ShouldReturnNotEmptyWhenNewCardsAdded(Scenario scenario)
    {
        var schedule = GetSchedule(scenario.Behavior);
        var collectionId = TestConstants.Collection.Id;
        if (AddedCards.Count == 0)
        {
            var newAddedCards = await AddRandomCardsToCollection(collectionId);
            AddedCards.AddRange(newAddedCards);
        }

        var getNotFinishedCollectionsResponse = await client.GetAsync(
            CollectionsQuery(ApiRoutes.Collections.GetNotFinished)
            + new QueryString()
                .Add("scheduleUserId", schedule.ParentUserId)
                .Add("scheduleId", schedule.Id));
        var notFinishedCollections = getNotFinishedCollectionsResponse.ToResponseDto<GetNotFinishedResponse>();

        notFinishedCollections.Should().NotBeNull();
        notFinishedCollections.CanStartCollections.Should().ContainSingle();
        var canStartCollection = notFinishedCollections.CanStartCollections.Single();
        canStartCollection.Id.Should().Be(collectionId.ToString());
        canStartCollection.NotStartedCards.Should().Be((short)AddedCards.Count);
    }

    [Order(3)]
    [TestCaseSource(nameof(Scenarios))]
    public async Task CardsController_RememberCard_ShouldAssertAllScenarios(Scenario scenario)
    {
        var schedule = GetSchedule(scenario.Behavior);
        var currentPhaseIndex = 0;
        foreach (var step in scenario.Steps)
        {
            var shouldBeNextPhaseIndex = Math.Max(0, currentPhaseIndex + step.NextPhaseIndexDiff);
            await Assert_CardsController_RememberCard_MovesToRightStep(
                schedule,
                (short)currentPhaseIndex,
                (short)shouldBeNextPhaseIndex,
                step.Weight
            );
            
            currentPhaseIndex = shouldBeNextPhaseIndex;
        }
    }

    public async Task Assert_CardsController_RememberCard_MovesToRightStep(
        Schedule schedule,
        short rememberPhaseIndex,
        short shouldMoveToPhaseIndex,
        float rememberWeight)
    {
        //ACTION
        var rememberCardResponse = await client.PatchAsJsonAsync(
            CardsQuery(ApiRoutes.Cards.Path_RememberCard),
            new RememberRequest()
            {
                PhaseIndex = rememberPhaseIndex,
                ScheduleId = short.Parse(schedule.Id),
                ScheduleUserId = long.Parse(schedule.ParentUserId),
                RememberItems = AllTestCards.Select(c => new RememberItem()
                {
                    CardId = short.Parse(c.Id),
                    Weight = rememberWeight,
                }).ToList(),
            });
        
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
            new CollectionAssertion(TestConstants.Collection.Id, AllTestCards.Count));
    }
    
    // [Test, Order(4)]
    // public async Task CardsController_RememberCard_ShouldStayOnForgetting()
    // {
    //     //ARRANGE
    //     const short rememberPhaseIndex = 1;
    //     var nextPhaseIndex = rememberPhaseIndex;
    //     
    //     //ACTION
    //     var rememberCardResponse = await client.PatchAsJsonAsync(
    //         CardsQuery(ApiRoutes.Cards.Path_RememberCard),
    //         new RememberRequest()
    //         {
    //             PhaseIndex = rememberPhaseIndex,
    //             ScheduleId = short.Parse(schedule.Id),
    //             ScheduleUserId = long.Parse(schedule.ParentUserId),
    //             RememberItems = AllTestCards.Select(c => new RememberItem()
    //             {
    //                 CardId = short.Parse(c.Id),
    //                 Weight = ForgottenWeight,
    //             }).ToList(),
    //         });
    //     
    //     //ASSERT
    //     var getRepeatCollectionsResponse = await client.GetAsync(
    //         CollectionsQuery(ApiRoutes.Collections.GetRepeatCollections));
    //     var repeatCollections = getRepeatCollectionsResponse.ToResponseDto<RepeatingCollectionResponse>();
    //     
    //     AssertHasDate(repeatCollections, nextPhaseIndex);
    //     AssertHasPhasesAtDate(repeatCollections, nextPhaseIndex, nextPhaseIndex);
    //     AssertHasCollectionsAtDatePhase(
    //         repeatCollections,
    //         nextPhaseIndex,
    //         nextPhaseIndex,
    //         new CollectionAssertion(TestConstants.Collection.Id, AllTestCards.Count));
    // }
    
    [Test]
    public async Task Test()
    {
        
    }

    private void AssertHasDate(RepeatingCollectionResponse repeatingCollectionResponse, string scheduleId, params int[] phaseIndexes)
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
            var diff = TimeSpan.FromHours(1);
            
            dates.Should().OnlyContain(d => d.Date == date.Date);
        }
    }
    
    private void AssertHasPhasesAtDate(RepeatingCollectionResponse repeatingCollectionResponse, string scheduleId, int phaseDateIndex, params int[] phaseIndexes)
    {
        var now = DateTime.Now;
        var dates = repeatingCollectionResponse.DateToRepeatingPhases.Keys.ToList();

        var targetDate = dates.FirstOrDefault(d => d.Date == now.Add(phasesDuration[phaseDateIndex]).Date);
        var phases = repeatingCollectionResponse.DateToRepeatingPhases[targetDate];

        foreach (var shouldContainPhaseIndex in phaseIndexes)
        {
            var duration = phasesDuration[shouldContainPhaseIndex];
            var phase = phases
                .Where(p => p.ScheduleId == scheduleId)
                .SingleOrDefault(p => TimeSpan.FromSeconds(p.SecondsFromLastPhase) == duration);
            
            phase.Should().NotBeNull();
            phase.RepeatingCollections.Should().NotBeEmpty();
        }
    }

    private record CollectionAssertion(short CollectionId, int ShouldHasCardsCount);
    
    private void AssertHasCollectionsAtDatePhase(
        RepeatingCollectionResponse repeatingCollectionResponse,
        string scheduleId,
        int phaseDateIndex,
        int phaseIndex, 
        params CollectionAssertion[] collectionAssertions)
    {
        var now = DateTime.Now;
        var dates = repeatingCollectionResponse.DateToRepeatingPhases.Keys.ToList();

        var targetDate = dates.FirstOrDefault(d => d.Date == now.Add(phasesDuration[phaseDateIndex]).Date);
        var phases = repeatingCollectionResponse.DateToRepeatingPhases[targetDate];

        var phaseDuration = phasesDuration[phaseIndex];
        var phase = phases
            .Where(p => p.ScheduleId == scheduleId)
            .Single(p => TimeSpan.FromSeconds(p.SecondsFromLastPhase) == phaseDuration);
        
        phase.Should().NotBeNull();
        phase.RepeatingCollections.Should().NotBeEmpty();
        
        foreach (var phaseRepeatingCollection in phase.RepeatingCollections)
        {
            var collectionAssert = collectionAssertions.FirstOrDefault(c =>
                c.CollectionId.ToString() == phaseRepeatingCollection.Collection.Id);
            
            collectionAssert.Should().NotBeNull();
            phaseRepeatingCollection.CardsToRepeatCount.Should().Be(collectionAssert.ShouldHasCardsCount);
        }
    }
}