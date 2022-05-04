using DB.Models;
using Newtonsoft.Json;

namespace IntervalLearningApi.Models.RepeatsSchedule;

public class Schedule
{
    [JsonProperty("userId")]
    public string ParentUserId { get; }
    public string Id { get; }
    public string Title { get; }
    public short CardsCountPerPhase { get; }
    public string? Description { get; }
    public bool IsRecommended { get; }
    public int ForgottenBehavior { get; }
    
    public List<Phase> Phases { get; }

    public Schedule(
        long parentUserId,
        short id,
        string title,
        short cardsCountPerPhase,
        string? description,
        ForgottenBehavior forgottenBehavior,
        bool isRecommended,
        List<Phase> phases)
    {
        ParentUserId = parentUserId.ToString();
        Id = id.ToString();
        Title = title;
        CardsCountPerPhase = cardsCountPerPhase;
        Description = description;
        IsRecommended = isRecommended;
        ForgottenBehavior = (int)forgottenBehavior;
        Phases = phases;
    }
}