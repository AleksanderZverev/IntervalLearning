using DB.Models;
using Mapster;
using Newtonsoft.Json;

namespace IntervalLearningApi.Models.RepeatsSchedule;

public class ScheduleRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<RepeatsScheduleEntity, Schedule>()
            .Map(d => d.Description, s => s.OnStartLearningDescription);
    }
}

public class Schedule
{
    [JsonProperty("userId")]
    public string ParentUserId { get; }
    public string Id { get; }
    public string Title { get; }
    public short CardsCountPerPhase { get; }

    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? DefaultPhaseShortDescription { get; set; }
    public string? DefaultPhaseDescription { get; set; }
    public string? DefaultRepeatPhaseShortDescription { get; set; }
    public string? DefaultRepeatPhaseDescription { get; set; }

    public bool IsRecommended { get; }
    public int ForgottenBehavior { get; }
    
    public List<Phase> Phases { get; }

    public Schedule(
        long parentUserId,
        short id,
        string title,
        short cardsCountPerPhase,
        string? shortDescription,
        string? description,
        ForgottenBehavior forgottenBehavior,
        bool isRecommended,
        List<Phase> phases,
        string? defaultPhaseShortDescription,
        string? defaultPhaseDescription,
        string? defaultRepeatPhaseShortDescription,
        string? defaultRepeatPhaseDescription)
    {
        ParentUserId = parentUserId.ToString();
        Id = id.ToString();
        Title = title;
        CardsCountPerPhase = cardsCountPerPhase;
        Description = description;
        IsRecommended = isRecommended;
        ForgottenBehavior = (int)forgottenBehavior;
        Phases = phases;
        ShortDescription = shortDescription;
        DefaultPhaseShortDescription = defaultPhaseShortDescription;
        DefaultPhaseDescription = defaultPhaseDescription;
        DefaultRepeatPhaseShortDescription = defaultRepeatPhaseShortDescription;
        DefaultRepeatPhaseDescription = defaultRepeatPhaseDescription;
    }
}