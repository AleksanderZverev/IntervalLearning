namespace IntervalLearningApi.Controllers;

public class RememberCardRequest
{
    public List<RememberItemDto> RememberItems { get; set; }
    public long ScheduleUserId { get; set; }
    public short ScheduleId { get; set; }
    public short PhaseIndex { get; set; }
}