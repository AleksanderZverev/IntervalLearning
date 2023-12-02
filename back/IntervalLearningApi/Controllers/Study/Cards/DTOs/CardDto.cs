using Domain.Card;
using IntervalLearningApi.Controllers.Study.RepeatsSchedules.DTOs;
using Mapster;
using Newtonsoft.Json;

namespace IntervalLearningApi.Controllers.Study.Cards.DTOs;

public class CardRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Card, CardDto>()
            .Map(d => d.BackSideText, s => s.MeaningText)
            .Map(d => d.FrontSideText, s => s.RememberingText);
    }
}

public class CardDto
{
    [JsonProperty("userId")]
    public string ParentUserId { get; set; }
    [JsonProperty("collectionId")]
    public string ParentCollectionId { get; set; }
    public string Id { get; set; }
    public string BackSideText { get; set; }
    public string PromptText { get; set; }
    public string FrontSideText { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? Description { get; set; }
    public List<string>? Examples { get; set; }
    public List<RememberDto>? Remembers { get; set; }
}