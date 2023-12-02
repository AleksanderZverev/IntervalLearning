namespace IntervalLearningApi.Models.ByUser;

public class RepeatingCollectionResponse
{
    public Dictionary<DateTime, List<RepeatingPhaseDto>> DateToRepeatingPhases { get; set; }

    public RepeatingCollectionResponse(Dictionary<DateTime, List<RepeatingPhaseDto>> dateToRepeatingPhases)
    {
        DateToRepeatingPhases = dateToRepeatingPhases;
    }
}