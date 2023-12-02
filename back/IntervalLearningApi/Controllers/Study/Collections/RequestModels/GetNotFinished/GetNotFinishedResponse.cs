namespace IntervalLearningApi.Models.ByUser;

public class GetNotFinishedResponse
{
    public int TotalCollections { get; set; }
    public List<CollectionDto> CanStartCollections { get; set; }

    public GetNotFinishedResponse(int totalCollections, List<CollectionDto> canStartCollections)
    {
        this.TotalCollections = totalCollections;
        CanStartCollections = canStartCollections;
    }
}