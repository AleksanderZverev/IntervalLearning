using Newtonsoft.Json;

namespace IntervalLearningApi.Models.RepeatsSchedule;

public class Phase
{
    [JsonProperty("userId")]
    public string ParentUserId { get; }

    [JsonProperty("scheduleId")]
    public short ParentRepeatsScheduleId { get; }
    public byte Id { get; }
    public uint SecondsFromLastPhase { get; }
    public string? Description { get; }

    public Phase(string parentUserId, short parentRepeatsScheduleId, byte id, uint secondsFromLastPhase, string? description)
    {
        ParentUserId = parentUserId;
        ParentRepeatsScheduleId = parentRepeatsScheduleId;
        Id = id;
        SecondsFromLastPhase = secondsFromLastPhase;
        Description = description;
    }
}