using IntervalLearningApi.Controllers.Study.RepeatsSchedules.DTOs;

namespace IntervalLearningApi.Controllers.Study.Collections.RequestModels.GetRepeatCollections;

public class RepeatingCollectionResponse
{
    public Dictionary<DateTime, List<RepeatingPhaseDto>> DateToRepeatingPhases { get; set; }

    public RepeatingCollectionResponse(Dictionary<DateTime, List<RepeatingPhaseDto>> dateToRepeatingPhases)
    {
        DateToRepeatingPhases = dateToRepeatingPhases;
    }
}