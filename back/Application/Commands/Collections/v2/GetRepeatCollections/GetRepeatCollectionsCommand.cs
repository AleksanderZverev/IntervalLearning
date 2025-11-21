using Domain.Collection.ValueObjects;
using Domain.Schedule.ValueObjects;
using Domain.Theme.ValueObjects;
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
    public List<RepeatingInfoByDate> RepeatingInfosByDate { get; init; }
}

public record RepeatingInfoByDate(DateTime Date, List<RepeatingCollectionInfo> RepeatingCollections);

public record RepeatingCollectionInfo(
    ComplexCollectionId CollectionId,
    CollectionTitle CollectionTitle,
    bool IsRepeatingForgottenWords,
    bool IsRepeatable,
    ThemeId ThemeId)
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

        Dictionary<DateTime, RepeatingInfoByDate> dateToCollectionItem = new();

        foreach (var queueItem in queueItems)
        {
            var date = queueItem.Date.Date;

            if (!dateToCollectionItem.TryGetValue(date, out var collectionItem))
            {
                collectionItem = new RepeatingInfoByDate(date, new List<RepeatingCollectionInfo>());
                dateToCollectionItem[date] = collectionItem;
            }

            var collection = collectionIdToCollection[queueItem.ParentCollectionId];
            var phase = schedule.FindPhase(queueItem.PhaseIndex);
            var isRepeatable = schedule.CanRepeat(queueItem.PhaseIndex, date, dateTimeProvider).Value;

            var repeatingPhaseItem = collectionItem.RepeatingCollections
                .FirstOrDefault(p => p.CollectionId == collection.ComplexId);

            if (repeatingPhaseItem == null)
            {
                repeatingPhaseItem = new RepeatingCollectionInfo(
                    ComplexCollectionId.Create(collection.ParentUserId, collection.Id).Value,
                    collection.Title,
                    phase.IsRepeat(),
                    isRepeatable,
                    collection.ThemeId);
                
                collectionItem.RepeatingCollections.Add(repeatingPhaseItem);
            }

            repeatingPhaseItem.OnQueueItemFound(date);
        }

        return new GetRepeatCollectionsCommandResponseV2()
        {
            ScheduleId = scheduleId,
            RepeatingInfosByDate = dateToCollectionItem.Values.ToList(),
        };
    }
}