
using IntervalLearningApi.Controllers.Study.RepeatsSchedules.DTOs;

namespace IntervalLearningApi.Controllers.Study.Cards.Requests.StartCards;

public class StartCardsResponse
{
    public DateTime? NextRepeatDate { get; }
    public PhaseDto? NextRepeatPhase { get; }
    public int NextPhaseIndex { get; }

    public StartCardsResponse(DateTime? nextRepeatDate, PhaseDto? nextRepeatPhase, int nextPhaseIndex)
    {
        NextRepeatDate = nextRepeatDate;
        NextRepeatPhase = nextRepeatPhase;
        NextPhaseIndex = nextPhaseIndex;
    }
}