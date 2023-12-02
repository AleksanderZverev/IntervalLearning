using IntervalLearningApi.Models.RepeatsSchedule;

namespace IntervalLearningApi.Controllers;

public class RememberCardResponse
{
    public DateTime? NextRepeatDate { get; }
    public PhaseDto? NextRepeatPhase { get; }
    public int NextPhaseIndex { get; }

    public RememberCardResponse(DateTime? nextRepeatDate, PhaseDto? nextRepeatPhase, int nextPhaseIndex)
    {
        NextRepeatDate = nextRepeatDate;
        NextRepeatPhase = nextRepeatPhase;
        NextPhaseIndex = nextPhaseIndex;
    }
}