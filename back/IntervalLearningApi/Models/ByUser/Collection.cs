using System.ComponentModel.DataAnnotations;
using DB.Models;
using DB.Models.Store;
using Mapster;
using Newtonsoft.Json;

namespace IntervalLearningApi.Models.ByUser;

public class CollectionsRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CollectionEntity, Collection>()
            .Map(d => d.Publication, s => s.CollectionPublicationEntity);
    }
}

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
    public bool IsPublic { get; }
    public CollectionPublication? Publication { get; }

    public Collection(
        long parentUserId,
        short id,
        string title,
        DateTime createdAt,
        short themeId,
        short cardsCount,
        short notStartedCards,
        bool isPublic,
        CollectionPublication? publication)
    {
        ParentUserId = parentUserId.ToString();
        Id = id.ToString();
        Title = title;
        CreatedAt = createdAt;
        ThemeId = themeId;
        CardsCount = cardsCount;
        NotStartedCards = notStartedCards;
        IsPublic = isPublic;
        Publication = publication;
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
