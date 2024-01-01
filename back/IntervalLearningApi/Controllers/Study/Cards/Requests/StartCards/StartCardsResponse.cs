
using IntervalLearningApi.Controllers.Study.RepeatsSchedules.DTOs;

namespace IntervalLearningApi.Controllers.Study.Cards.Requests.StartCards;

public class StartCardsResponse
{
    public DateTime? NextRepeatDate { get; init; }
    public PhaseDto? NextRepeatPhase { get; init; }
    public int NextPhaseIndex { get; init; }
    public required List<CardMovementInfoDto> CardMovementInfos { get; init; }
}