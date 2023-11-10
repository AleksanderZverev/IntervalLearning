using System.ComponentModel.DataAnnotations;
using DB.Models;
using Mapster;
using Newtonsoft.Json;

namespace IntervalLearningApi.Models.ByUser;

public class CollectionsRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CollectionEntity, Collection>()
            .Map(d => d.Publication, s => s.CollectionPublicationEntity)
            .Map(d => d.CreatedAt, s => s.CreatedDate)
            .Map(d => d.NotStartedCards, s => s.NotStartedCardsCount);
    }
}

public class Collection
{
    [JsonProperty("userId")] 
    public string ParentUserId { get; set; }
    public string Id { get; set; }
    public string Title { get; set; }
    public DateTime CreatedAt { get; set; }
    public short ThemeId { get; set; }

    public short CardsCount { get; set; }
    public short NotStartedCards { get; set; }
    public bool IsPublic { get; set; }
    public CollectionPublication? Publication { get; set; }
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
    public int TotalCollections { get; set; }
    public List<Collection> CanStartCollections { get; set; }

    public GetNotFinishedResponse(int totalCollections, List<Collection> canStartCollections)
    {
        this.TotalCollections = totalCollections;
        CanStartCollections = canStartCollections;
    }
}

public class RepeatingCollectionResponse
{
    public Dictionary<DateTime, List<RepeatingPhaseDto>> DateToRepeatingPhases { get; set; }

    public RepeatingCollectionResponse(Dictionary<DateTime, List<RepeatingPhaseDto>> dateToRepeatingPhases)
    {
        DateToRepeatingPhases = dateToRepeatingPhases;
    }
}
