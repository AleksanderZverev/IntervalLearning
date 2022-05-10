using Newtonsoft.Json;

namespace IntervalLearningApi.Models.RepeatsSchedule;

public class Phase
{
    [JsonProperty("userId")]
    public string ParentUserId { get; }

    [JsonProperty("scheduleId")]
    public string ParentRepeatsScheduleId { get; }
    public string Id { get; }
    public uint SecondsFromLastPhase { get; }
    public string? ShortDescription { get; }
    public string? Description { get; }

    public Phase(
        long parentUserId,
        short parentRepeatsScheduleId,
        short id,
        uint secondsFromLastPhase,
        string? shortDescription,
        string? description)
    {
        ParentUserId = parentUserId.ToString();
        ParentRepeatsScheduleId = parentRepeatsScheduleId.ToString();
        Id = id.ToString();
        SecondsFromLastPhase = secondsFromLastPhase;
        ShortDescription = shortDescription;
        Description = description;
    }
}