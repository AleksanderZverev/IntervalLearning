using IntervalLearningApi.Controllers.Study.Collections.DTOs;

namespace IntervalLearningApi.Controllers.Study.Collections.RequestModels.GetNotFinished;

public class GetNotFinishedResponse
{
    public int TotalCollections { get; set; }
    public List<CollectionDto> CanStartCollections { get; set; }
    public List<CollectionDto> CanRelearnCollections { get; set; }
}