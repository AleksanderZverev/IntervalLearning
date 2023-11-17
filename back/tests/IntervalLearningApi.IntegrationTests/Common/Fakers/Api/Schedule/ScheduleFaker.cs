using Bogus;

namespace IntervalLearningApi.IntegrationTests.Common.Fakers.Api;

public class ScheduleFaker : Faker<RepeatsScheduleDto>
{
    public ScheduleFaker()
    {
        CustomInstantiator(f =>
        {
            return new RepeatsScheduleDto()
            {
                ParentUserId = "1",
                Id = "1",
                Title = f.Company.CompanyName(),
                Description = f.Address.StreetName(),
                ForgottenBehavior = 1,
                CardsCountPerPhase = f.Random.Short(1, 30),
                Phases = Enumerable
                    .Range(0, 4)
                    .Select(_ => TimeSpan.FromDays(f.Random.Int(1, 5)))
                    .Select((duration, index) => new PhaseDto()
                    {
                        Id = (index + 1).ToString(),
                        ParentUserId = "1",
                        ParentRepeatsScheduleId = "1",
                        SecondsFromLastPhase = (uint)duration.TotalSeconds,
                    })
                    .ToList(),
            };
        });
    }
}

public class CreateScheduleRequestFaker : Faker<CreateScheduleRequest>
{
    public override CreateScheduleRequest Generate(string ruleSets = null)
    {
        var scheduleInfo = new ScheduleFaker().Generate();
        return new CreateScheduleRequest()
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
        };
    }
}