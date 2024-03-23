namespace Domain.UnitTests.Common.Bogus.Schedule;

public class FakeScheduleId : Faker<ScheduleId>
{
    public FakeScheduleId()
    {
        CustomInstantiator(
            f => ScheduleId.Create(f.Random.Short(min: 0)).Value);
    }
}