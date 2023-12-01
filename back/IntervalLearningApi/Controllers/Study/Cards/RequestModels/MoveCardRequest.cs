namespace IntervalLearningApi.Controllers;

public class MoveCardRequest
{
    public short DestinationCollectionId { get; set; }
    public short CardId { get; set; }
}