using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace IntervalLearningApi.Models.ByUser;

public class Collection
{
    [JsonProperty("userId")]
    public string ParentUserId { get; }
    public string Id { get; }
    public string Title { get; }
    public DateTime CreatedAt { get; }
    public short ThemeId { get; }

    public short CardsCount { get; }
    public short NotStartedCards { get; set; }

    public Collection(
        long parentUserId,
        short id,
        string title,
        DateTime createdAt,
        short themeId,
        short cardsCount,
        short notStartedCards)
    {
        ParentUserId = parentUserId.ToString();
        Id = id.ToString();
        Title = title;
        CreatedAt = createdAt;
        ThemeId = themeId;
        CardsCount = cardsCount;
        NotStartedCards = notStartedCards;
    }
}

public class CreateCollectionItem
{
    public short? CollectionId { get; set; }
    [Required]
    public short ThemeId { get; set; }
    [Required]
    [StringLength(100)]
    public string Title { get; set; }
    public bool IsDefaultBackSide { get; set; }
}

public class GetNotFinishedResponse
{
    public int TotalCollections { get; }
    public List<Collection> CanStartCollections { get; }

    public GetNotFinishedResponse(int totalCollections, List<Collection> canStartCollections)
    {
        this.TotalCollections = totalCollections;
        CanStartCollections = canStartCollections;
    }
}

public class RepeatingCollectionResponse
{
    public Dictionary<DateTime, List<RepeatingPhaseDto>> DateToRepeatingPhases { get; }

    public RepeatingCollectionResponse(Dictionary<DateTime, List<RepeatingPhaseDto>> dateToRepeatingPhases)
    {
        DateToRepeatingPhases = dateToRepeatingPhases;
    }
}

public class RepeatingPhaseDto
{
    public string ScheduleUserId { get; }
    public string ScheduleId { get; }
    public short PhaseIndex { get; }
    public uint SecondsFromLastPhase { get; }
    public string? Description { get; }
    public List<RepeatingCollectionDto> RepeatingCollections { get; set; }

    public RepeatingPhaseDto(
        long scheduleUserId,
        long scheduleId,
        short phaseIndex,
        uint secondsFromLastPhase,
        string? description, 
        List<RepeatingCollectionDto> repeatingCollections)
    {
        ScheduleUserId = scheduleUserId.ToString();
        ScheduleId = scheduleId.ToString();
        PhaseIndex = phaseIndex;
        SecondsFromLastPhase = secondsFromLastPhase;
        Description = description;
        RepeatingCollections = repeatingCollections;
    }
}

public class RepeatingCollectionDto
{
    public Collection Collection { get; }

    public int CardsToRepeatCount { get; }

    public RepeatingCollectionDto(Collection collection, int cardsToRepeatCount)
    {
        Collection = collection;
        CardsToRepeatCount = cardsToRepeatCount;
    }
}
