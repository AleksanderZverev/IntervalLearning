namespace Domain.UnitTests.Schedule;

[TestFixture]
public class ScheduleUnitTests
{
    private static List<TimeSpan> DefaultPhases = new()
    {
        TimeSpan.FromDays(1),
        TimeSpan.FromDays(3),
        TimeSpan.FromDays(7),
        TimeSpan.FromDays(14),
        TimeSpan.FromDays(28),
        TimeSpan.FromDays(56),
        TimeSpan.FromDays(56),
        TimeSpan.FromDays(56)
    };

    private static Phase GetPhaseByDuration(RepeatsSchedule schedule, TimeSpan duration)
        => schedule.Phases.First(p => p.GetDurationToNextPhase() == duration);

    private static int GetPhaseIndexByDuration(RepeatsSchedule schedule, TimeSpan duration)
        => schedule.Phases.FindIndex(p => p.GetDurationToNextPhase() == duration);

    private static RepeatsSchedule GetDefaultSchedule()
        => GetSchedule(ForgottenBehavior.MoveToPreviousStep, DefaultPhases);

    private static RepeatsSchedule GetSchedule(ForgottenBehavior forgottenBehavior, List<TimeSpan> phases)
    {
        var faker = new Faker();
        var userId = new FakeUserId().Generate();
        var scheduleId = new FakeScheduleId().Generate();
        return new RepeatsSchedule(userId, scheduleId)
        {
            Title = ScheduleTitle.Create(faker.Company.CompanyName()).Value,
            ForgottenBehavior = forgottenBehavior,
            CardsCountPerPhase = 100,
            Phases = phases.Select(
                (duration, i) =>
                {
                    return new Phase(scheduleId, userId, PhaseId.Create((short)(i + 1)).Value)
                    {
                        SecondsFromLastPhase = (uint)duration.TotalSeconds,
                    };
                }).ToList(),
        };
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(-99)]
    public void CanRepeat_ShouldReturnTrue_WhenThereRepeatingIsTodayOrPassed(int daysTillRepeating)
    {
        //Arrange
        var schedule = GetDefaultSchedule();

        foreach (var phaseDuration in DefaultPhases)
        {
            //Act
            var phaseIndex = GetPhaseIndexByDuration(schedule, phaseDuration);
            var now = DateTime.UtcNow;
            var repeatingDate = now.AddDays(daysTillRepeating);
            var canRepeatResult = schedule.CanRepeat(phaseIndex, repeatingDate, new DateTimeProviderMock(now));

            //Assert
            canRepeatResult.IsSuccess.Should().BeTrue();
            canRepeatResult.Value.Should().BeTrue();
        }
    }

    [TestCase(1, 1, false)]
    [TestCase(3, 1, false)]
    [TestCase(7, 1, true)]
    [TestCase(7, 2, false)]
    [TestCase(14, 2, true)]
    [TestCase(14, 3, false)]
    [TestCase(28, 4, true)]
    [TestCase(28, 5, false)]
    [TestCase(56, 8, true)]
    [TestCase(56, 9, false)]
    public void CanRepeat_ShouldReturnCorrectValue_WhenThereRepeatingIsInTheFuture(
        int phaseDurationInDays,
        int daysTillRepeating,
        bool expectedCanRepeat)
    {
        //Arrange
        var schedule = GetDefaultSchedule();

        //Act
        var phaseIndex = GetPhaseIndexByDuration(schedule, TimeSpan.FromDays(phaseDurationInDays));
        var now = DateTime.UtcNow;
        var repeatingDate = now.AddDays(daysTillRepeating);
        var canRepeatResult = schedule.CanRepeat(phaseIndex, repeatingDate, new DateTimeProviderMock(now));

        //Assert
        canRepeatResult.IsSuccess.Should().BeTrue();
        canRepeatResult.Value.Should().Be(expectedCanRepeat);
    }
}