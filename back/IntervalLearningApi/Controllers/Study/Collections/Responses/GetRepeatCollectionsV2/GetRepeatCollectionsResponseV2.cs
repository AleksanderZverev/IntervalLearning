using Application.Commands.Collections.v2.GetRepeatCollections;
using Domain.Collection.ValueObjects;
using Domain.Schedule.ValueObjects;
using Domain.Theme.ValueObjects;
using Mapster;

namespace IntervalLearningApi.Controllers.Study.Collections.Responses.GetRepeatCollectionsV2;

public class GetRepeatCollectionsResponseV2
{
    public string ParentUserId { get; init; }
    public string ScheduleId { get; init; }
    public List<RepeatingCollectionInfo> LateCollections { get; init; }
    public List<RepeatingCollectionInfo> RepeatingForgottenWordsCollections { get; init; }
    public List<RepeatingInfoByDate> RepeatingInfosByDate { get; init; }

    public record RepeatingInfoByDate(DateTime Date, List<RepeatingCollectionInfo> RepeatingCollections);

    public record RepeatingCollectionInfo(
        string CollectionId,
        string CollectionUserId,
        string CollectionTitle,
        bool IsRepeatingForgottenWords,
        bool IsRepeatable,
        int CardsCount,
        DateTime EarliestDateToRepeat,
        DateTime OldestDateToRepeat,
        short ThemeId);
}

public class GetRepeatCollectionsResponseV2Register : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ThemeId, short>()
            .MapWith(t => t.Value);

        config.NewConfig<GetRepeatCollectionsCommandResponseV2, GetRepeatCollectionsResponseV2>()
            .Map(r => r.ScheduleId, a => a.ScheduleId.Id.ToString())
            .Map(r => r.ParentUserId, a => a.ScheduleId.ParentUserId.ToString());

        config.NewConfig<RepeatingInfoByDate, GetRepeatCollectionsResponseV2.RepeatingInfoByDate>();

        config.NewConfig<RepeatingCollectionInfo, GetRepeatCollectionsResponseV2.RepeatingCollectionInfo>()
            .Map(r => r.CollectionId, a => a.CollectionId.Id.ToString())
            .Map(r => r.CollectionUserId, a => a.CollectionId.UserId.ToString());
    }
}