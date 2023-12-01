
using IntervalLearningApi.Models.RepeatsSchedule;

namespace IntervalLearningApi.Controllers;

public class StartCardResponse
{
    public DateTime? NextRepeatDate { get; }
    public PhaseDto? NextRepeatPhase { get; }
    public int NextPhaseIndex { get; }

    public StartCardResponse(DateTime? nextRepeatDate, PhaseDto? nextRepeatPhase, int nextPhaseIndex)
    {
        NextRepeatDate = nextRepeatDate;
        NextRepeatPhase = nextRepeatPhase;
        NextPhaseIndex = nextPhaseIndex;
    }
}