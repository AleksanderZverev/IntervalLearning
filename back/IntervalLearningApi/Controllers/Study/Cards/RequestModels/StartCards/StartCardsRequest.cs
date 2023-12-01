namespace IntervalLearningApi.Controllers;

public class StartCardsRequest
{
    public long ScheduleUserId { get; set; }
    public short ScheduleId { get; set; }
    public List<short> CardIds { get; set; }
}