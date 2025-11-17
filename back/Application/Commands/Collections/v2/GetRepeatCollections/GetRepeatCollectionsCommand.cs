using Domain.Collection.ValueObjects;
using Domain.Schedule.ValueObjects;
using Domain.User.ValueObjects;
using DomainServices.DB.Queries.Study;
using FluentResults;
using GlobalTools;

namespace Application.Commands.Collections.v2.GetRepeatCollections;

public record GetRepeatCollectionsCommandRequestV2(
    ComplexScheduleId ScheduleId,
    UserId UserId,
    DateTime? UntilDate);

public record GetRepeatCollectionsCommandResponseV2
{
    public ComplexScheduleId ScheduleId { get; init; }
    public List<RepeatingCollectionItem> RepeatingCollections { get; init; }
}

public record RepeatingCollectionItem(DateTime Date, List<RepeatingPhaseItem> RepeatingPhaseItems);

public record RepeatingPhaseItem(
    ComplexCollectionId CollectionId,
    CollectionTitle CollectionTitle,
    TimeSpan PhaseDuration,
    bool IsRepeatable)
{
    public int CardsCount { get; private set; }
    public DateTime EarliestDateToRepeat { get; private set; }

    public void IncrementCardsCount()
    {
        CardsCount++;
    }

    public void OnQueueItemFound(DateTime date)
    {
        date = date.Date;
        CardsCount++;
        EarliestDateToRepeat = EarliestDateToRepeat > date || EarliestDateToRepeat == DateTime.MinValue
            ? date
            : EarliestDateToRepeat;
    }
}

public class GetRepeatCollectionsCommandV2
    : ICommand<GetRepeatCollectionsCommandRequestV2, GetRepeatCollectionsCommandResponseV2>
{
    private readonly IDateTimeProvider dateTimeProvider;
    private readonly IStudyQueryRepository studyQueryRepository;

    public GetRepeatCollectionsCommandV2(
        IDateTimeProvider dateTimeProvider,
        IStudyQueryRepository studyQueryRepository)
    {
        this.dateTimeProvider = dateTimeProvider;
        this.studyQueryRepository = studyQueryRepository;
    }

    public async Task<Result<GetRepeatCollectionsCommandResponseV2>> Handle(
        GetRepeatCollectionsCommandRequestV2 request)
    {
        var (scheduleId, userId, untilDate) = request;

        var schedule = await studyQueryRepository.Schedules.Find(scheduleId.ParentUserId, scheduleId.Id);

        if (schedule == null)
            return Result.Fail("Schedule is not found");

        var queueItems = await studyQueryRepository.RepeatingQueue.GetAllBySchedule(userId, scheduleId);

        if (untilDate != null)
        {
            queueItems = queueItems
                .Where(i => i.Date.Date <= untilDate.Value.Date)
                .ToList();
        }

        var collectionIds = queueItems
            .Select(q => q.ParentCollectionId)
            .Distinct()
            .ToList();

        var collections = await studyQueryRepository.Collections.GetRange(userId, collectionIds);
        var collectionIdToCollection = collections.ToDictionary(c => c.Id);

        Dictionary<DateTime, RepeatingCollectionItem> dateToCollectionItem = new();

        foreach (var queueItem in queueItems)
        {
            var date = queueItem.Date.Date;

            if (!dateToCollectionItem.TryGetValue(date, out var collectionItem))
            {
                collectionItem = new RepeatingCollectionItem(date, new List<RepeatingPhaseItem>());
                dateToCollectionItem[date] = collectionItem;
            }

            var collection = collectionIdToCollection[queueItem.ParentCollectionId];
            var phase = schedule.FindPhase(queueItem.PhaseIndex);
            var isRepeatable = schedule.CanRepeat(queueItem.PhaseIndex, date, dateTimeProvider).Value;

            var repeatingPhaseItem = collectionItem.RepeatingPhaseItems
                .FirstOrDefault(p => p.CollectionId == collection.ComplexId
                                     && p.PhaseDuration == phase.GetDurationToNextPhase());

            if (repeatingPhaseItem == null)
            {
                repeatingPhaseItem = new RepeatingPhaseItem(
                    ComplexCollectionId.Create(collection.ParentUserId, collection.Id).Value,
                    collection.Title,
                    phase.GetDurationToNextPhase(),
                    isRepeatable);
                
                collectionItem.RepeatingPhaseItems.Add(repeatingPhaseItem);
            }

            repeatingPhaseItem.OnQueueItemFound(date);
        }

        return new GetRepeatCollectionsCommandResponseV2()
        {
            ScheduleId = scheduleId,
            RepeatingCollections = dateToCollectionItem.Values.ToList(),
        };
    }
}