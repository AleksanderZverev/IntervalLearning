using Bogus;
using IntervalLearningApi.IntegrationTests.Common.Fakers.Api;

namespace IntervalLearningApi.IntegrationTests.Schedule;

[UseBasePath(ApiRoutes.Schedule.BasePath)]
public class SchedulesControllerTests : SharedApiTests
{
    public SchedulesControllerTests(SharedDockerIntervalLearningApiFactory apiFactory) : base(apiFactory)
    {
    }
    
    [Fact]
    public async Task CreateSchedule_ShouldCreateScheduleWithPhases()
    {
        //Arrange
        var (client, scope) = SharedScope;

        //Act
        var (createdSchedule, scheduleInfo) = await CreateRandomSchedule();

        //Assert
        createdSchedule.Should().NotBeNull();
        createdSchedule.ParentUserId.Should().Be(scope.Id);;
        createdSchedule.Id.Should().NotBeEmpty().And.NotBe("0");
        createdSchedule.Title.Should().Be(scheduleInfo.Title);
        createdSchedule.ForgottenBehavior.Should().Be(scheduleInfo.ForgottenBehavior);
        createdSchedule.CardsCountPerPhase.Should().Be(scheduleInfo.CardsCountPerPhase);
        createdSchedule.Phases.Should().HaveCount(scheduleInfo.Phases.Count);

        AssertPhasesEqual(createdSchedule.Phases, scheduleInfo.Phases);
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllSchedules()
    {
        //Arrange
        var (client, scope) = SharedScope;
        var (createdSchedule, scheduleInfo) = await CreateRandomSchedule();

        //Act
        var allSchedulesResult = await client.GetAsync(ApiRoutes.Schedule.Get_GetAll);
        var allSchedules = allSchedulesResult.ToResponseDto<List<RepeatsScheduleDto>>();

        //Assert
        allSchedules.Should().NotBeNull().And.NotBeEmpty();
        var schedule = allSchedules.First();
        schedule.ParentUserId.Should().NotBeNullOrEmpty().And.NotBe("0");
        schedule.Id.Should().NotBeNullOrEmpty().And.NotBe("0");
        schedule.Title.Should().NotBeNullOrEmpty();
        schedule.ForgottenBehavior.Should().NotBe(0);
        AssertPhasesEqual(schedule.Phases, createdSchedule.Phases);
    }
    
    [Fact]
    public async Task GetSchedule_ShouldFindOwnSchedule()
    {
        //Arrange
        var (client, scope) = SharedScope;
        var (createdSchedule, scheduleInfo) = await CreateRandomSchedule();

        //Act
        var myScheduleResult = await client.GetAsync(ApiRoutes.Schedule.GetGetMySchedulePath(createdSchedule.Id));
        var mySchedule = myScheduleResult.ToResponseDto<RepeatsScheduleDto>();

        //Assert
        mySchedule.Should().NotBeNull();
        mySchedule.ParentUserId.Should().NotBeNullOrEmpty().And.Be(scope.Id);
        mySchedule.Id.Should().NotBeNullOrEmpty().And.Be(createdSchedule.Id);
        mySchedule.Title.Should().Be(createdSchedule.Title);
        mySchedule.ForgottenBehavior.Should().Be(createdSchedule.ForgottenBehavior);
        AssertPhasesEqual(mySchedule.Phases, createdSchedule.Phases);
    }

    [Fact]
    public async Task UpdateSchedule_ShouldUpdateWithoutPhases()
    {
        //Arrange
        var (client, scope) = SharedScope;
        var (createdSchedule, _) = await CreateRandomSchedule();

        //Act
        var updateScheduleInfo = new ScheduleFaker().Generate();
        var updateScheduleResult = await client.PatchAsJsonAsync(
            ApiRoutes.Schedule.GetEditSchedulePath(createdSchedule.Id),
            new UpdateScheduleRequest()
            {
                Title = updateScheduleInfo.Title,
                Description = updateScheduleInfo.Description,
                CardsCountPerPhase = updateScheduleInfo.CardsCountPerPhase,
                Phases = null,
            });
        var updatedSchedule = updateScheduleResult.ToResponseDto<RepeatsScheduleDto>();

        //Assert
        updatedSchedule.Should().NotBeNull();
        updatedSchedule.ParentUserId.Should().Be(scope.Id);
        updatedSchedule.Id.Should().NotBeEmpty().And.NotBe("0");
        updatedSchedule.Title.Should().Be(updateScheduleInfo.Title);
        updatedSchedule.ForgottenBehavior.Should().Be(updateScheduleInfo.ForgottenBehavior);
        updatedSchedule.CardsCountPerPhase.Should().Be(updateScheduleInfo.CardsCountPerPhase);
        updatedSchedule.Phases.Should().HaveCount(updateScheduleInfo.Phases.Count);
        
        AssertPhasesEqual(updatedSchedule.Phases, createdSchedule.Phases);
    }
    
    [Fact]
    public async Task UpdateSchedule_ShouldUpdateScheduleAndPhases()
    {
        //Arrange
        var (client, scope) = SharedScope;
        var (createdSchedule, _) = await CreateRandomSchedule();

        //Act
        var updateScheduleInfo = new ScheduleFaker().Generate();
        var faker = new Faker();
        var updatePhases = updateScheduleInfo.Phases.Select(p => new UpdatePhaseInfo()
        {
            Id = short.Parse(p.Id),
            Description = faker.Person.Company.Name,
            ShortDescription = faker.Person.Email,
            IsDefaultValueSide = p.IsDefaultValueSide,
        }).ToList();
        
        var updateScheduleResult = await client.PatchAsJsonAsync(
            ApiRoutes.Schedule.GetEditSchedulePath(createdSchedule.Id),
            new UpdateScheduleRequest()
            {
                Title = updateScheduleInfo.Title,
                Description = updateScheduleInfo.Description,
                CardsCountPerPhase = updateScheduleInfo.CardsCountPerPhase,
                Phases = updatePhases,
            });
        var updatedSchedule = updateScheduleResult.ToResponseDto<RepeatsScheduleDto>();

        //Assert
        updatedSchedule.Should().NotBeNull();
        updatedSchedule.ParentUserId.Should().Be(scope.Id);
        updatedSchedule.Id.Should().NotBeEmpty().And.NotBe("0");
        updatedSchedule.Title.Should().Be(updateScheduleInfo.Title);
        updatedSchedule.ForgottenBehavior.Should().Be(updateScheduleInfo.ForgottenBehavior);
        updatedSchedule.CardsCountPerPhase.Should().Be(updateScheduleInfo.CardsCountPerPhase);
        updatedSchedule.Phases.Should().HaveCount(updateScheduleInfo.Phases.Count);
        
        AssertPhasesEqual(updatedSchedule.Phases, updatePhases.Select(updatePhaseInfo =>
        {
            var createdPhase = createdSchedule.Phases.FirstOrDefault(
                cp => cp.Id == updatePhaseInfo.Id.ToString());
            
            return new PhaseDto()
            {
                ParentRepeatsScheduleId = updatedSchedule.Id,
                ParentUserId = updatedSchedule.ParentUserId,
                
                Id = updatePhaseInfo.Id.ToString(),
                IsDefaultValueSide = updatePhaseInfo.IsDefaultValueSide,
                Description = updatePhaseInfo.Description,
                ShortDescription = updatePhaseInfo.ShortDescription,
                
                SecondsFromLastPhase = createdPhase.SecondsFromLastPhase,
            };
        }).ToList());
    }

    private void AssertPhasesEqual(
        List<PhaseDto> actual,
        List<PhaseDto> expected)
    {
        if (actual.Count != expected.Count)
            Assert.Fail($"Phases are not equals. Count expected to be {expected.Count} but was {actual.Count}");

        foreach (var actualPhase in actual)
        {
            var expectedPhase = expected.FirstOrDefault(f => f.Id == actualPhase.Id);
            
            if (expectedPhase == null)
                Assert.Fail($"Found unknown phase with id [{actualPhase.ParentUserId}{actualPhase.ParentRepeatsScheduleId}{actualPhase.Id}]");

            actualPhase.Description.Should().Be(expectedPhase.Description);
            actualPhase.ShortDescription.Should().Be(expectedPhase.ShortDescription);
            actualPhase.IsDefaultValueSide.Should().Be(expectedPhase.IsDefaultValueSide);
            actualPhase.SecondsFromLastPhase.Should().Be(expectedPhase.SecondsFromLastPhase);
        }
    }
}