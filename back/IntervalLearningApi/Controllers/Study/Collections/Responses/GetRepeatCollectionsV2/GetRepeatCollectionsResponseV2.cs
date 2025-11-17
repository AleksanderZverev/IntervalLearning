using Application.Commands.Collections.v2.GetRepeatCollections;
using Domain.Collection.ValueObjects;
using Domain.Schedule.ValueObjects;
using Mapster;

namespace IntervalLearningApi.Controllers.Study.Collections.Responses.GetRepeatCollectionsV2;

public class GetRepeatCollectionsResponseV2
{
    
    public string ParentUserId { get; init; }
    public string ScheduleId { get; init; }
    public List<RepeatingCollectionItem> RepeatingCollections { get; init; }
    
    public record RepeatingCollectionItem(DateTime Date, List<RepeatingPhaseItem> RepeatingPhaseItems);

    public record RepeatingPhaseItem(
        string CollectionId,
        string CollectionUserId,
        string CollectionTitle,
        long PhaseDurationInSeconds,
        bool IsRepeatable,
        int CardsCount,
        DateTime EarliestDateToRepeat);
}

public class GetRepeatCollectionsResponseV2Register : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<GetRepeatCollectionsCommandResponseV2, GetRepeatCollectionsResponseV2>()
            .Map(r => r.ScheduleId, a => a.ScheduleId.Id.ToString())
            .Map(r => r.ParentUserId, a => a.ScheduleId.ParentUserId.ToString());
        
        config.NewConfig<RepeatingCollectionItem, GetRepeatCollectionsResponseV2.RepeatingCollectionItem>();

        config.NewConfig<RepeatingPhaseItem, GetRepeatCollectionsResponseV2.RepeatingPhaseItem>()
            .Map(r => r.CollectionId, a => a.CollectionId.Id.ToString())
            .Map(r => r.CollectionUserId, a => a.CollectionId.UserId.ToString())
            .Map(r => r.PhaseDurationInSeconds, a => a.PhaseDuration.TotalSeconds);
    }
}

