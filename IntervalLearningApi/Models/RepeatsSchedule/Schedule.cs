using DB.Models;
using Newtonsoft.Json;

namespace IntervalLearningApi.Models.RepeatsSchedule;

public class Schedule
{
    [JsonProperty("UserId")]
    public string ParentUserId { get; }
    public short Id { get; }
    public string Title { get; }
    public short CardsCountPerPhase { get; }
    public string? Description { get; }
    public ForgottenBehavior ForgottenBehavior { get; }
    public List<Phase> Phases { get; }

    public Schedule(string parentUserId, short id, string title, short cardsCountPerPhase, string? description,
        ForgottenBehavior forgottenBehavior, List<Phase> phases)
    {
        ParentUserId = parentUserId;
        Id = id;
        Title = title;
        CardsCountPerPhase = cardsCountPerPhase;
        Description = description;
        ForgottenBehavior = forgottenBehavior;
        Phases = phases;
    }
}