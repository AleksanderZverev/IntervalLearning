using Application.Commands.Collections.GetRepeatCollections;
using IntervalLearningApi.Controllers.Study.Collections.DTOs;
using Mapster;

namespace IntervalLearningApi.Controllers.Study.RepeatsSchedules.DTOs;

public class RepeatingPhaseDtoRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<RepeatingPhase, RepeatingPhaseDto>();

        config.NewConfig<RepeatingCollection, RepeatingCollectionDto>();
    }
}

public class RepeatingPhaseDto
{
    public string ScheduleUserId { get; set; }
    public string ScheduleId { get; set; }
    public short PhaseIndex { get; set; }
    public uint SecondsFromLastPhase { get; set; }
    public string? Description { get; set; }
    public List<RepeatingCollectionDto> RepeatingCollections { get; set; }
}

public class RepeatingCollectionDto
{
    public CollectionDto Collection { get; set; }

    public int CardsToRepeatCount { get; set; }
    public bool IsRepeatable { get; set; }
}