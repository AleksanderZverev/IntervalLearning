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
    DateTimeOffset UserCurrentDate,
    DateTime? UntilDate);

public record GetRepeatCollectionsCommandResponseV2
{
    public ComplexScheduleId ScheduleId { get; init; }
    
    public List<RepeatingCollectionInfo> LateCollections { get; init; }
    public List<RepeatingCollectionInfo> RepeatingForgottenWordsCollections { get; init; }
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
    public DateTime OldestDateToRepeat { get; private set; }

    public void IncrementCardsCount()
    {
        CardsCount++;
    }

    public void OnQueueItemFound(DateTime date)
    {
        date = date.Date;
        CardsCount++;
        EarliestDateToRepeat = EarliestDateToRepeat == DateTime.MinValue || EarliestDateToRepeat > date
            ? date
            : EarliestDateToRepeat;
        OldestDateToRepeat = OldestDateToRepeat == DateTime.MinValue || OldestDateToRepeat < date
            ? date
            : OldestDateToRepeat;
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
        var (scheduleId, userId, userCurrentDate, untilDate) = request;

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

        Dictionary<ComplexCollectionId, RepeatingCollectionInfo> lateCollections = [];
        Dictionary<ComplexCollectionId, RepeatingCollectionInfo> repeatingForgottenWordsCollections = [];
        Dictionary<DateTime, RepeatingInfoByDate> dateToCollectionItem = [];

        foreach (var queueItem in queueItems)
        {
            var dateWithUserOffset = queueItem.Date.Add(userCurrentDate.Offset).Date;
            
            var complexCollectionId = ComplexCollectionId.Create(queueItem.ParentUserId, queueItem.ParentCollectionId).Value;
            var collection = collectionIdToCollection[queueItem.ParentCollectionId];
            var phase = schedule.FindPhase(queueItem.PhaseIndex);

            var isRepeatingForgottenWordsPhase = phase.IsRepeat();
            var isRepeatable = schedule.CanRepeat(queueItem.PhaseIndex, queueItem.Date, userCurrentDate, dateTimeProvider).Value;

            if (isRepeatingForgottenWordsPhase)
            {
                if (!repeatingForgottenWordsCollections.TryGetValue(complexCollectionId, out var repeatingCollection))
                {
                    repeatingCollection = new RepeatingCollectionInfo(
                        complexCollectionId,
                        collection.Title,
                        isRepeatingForgottenWordsPhase,
                        isRepeatable,
                        collection.ThemeId);
                    repeatingForgottenWordsCollections[complexCollectionId] = repeatingCollection;
                }
                
                repeatingCollection.OnQueueItemFound(dateWithUserOffset);
                continue;
            }
            
            var isOldDate = dateWithUserOffset < userCurrentDate.Date;

            if (isOldDate)
            {
                if (!lateCollections.TryGetValue(complexCollectionId, out var lateCollection))
                {
                    lateCollection = new RepeatingCollectionInfo(
                        complexCollectionId,
                        collection.Title,
                        isRepeatingForgottenWordsPhase,
                        isRepeatable,
                        collection.ThemeId);
                    lateCollections[complexCollectionId] = lateCollection;
                }

                lateCollection.OnQueueItemFound(dateWithUserOffset);
                continue;
            }

            if (!dateToCollectionItem.TryGetValue(dateWithUserOffset, out var collectionItem))
            {
                collectionItem = new RepeatingInfoByDate(dateWithUserOffset, new List<RepeatingCollectionInfo>());
                dateToCollectionItem[dateWithUserOffset] = collectionItem;
            }

            var repeatingPhaseItem = collectionItem.RepeatingCollections
                .FirstOrDefault(p => p.CollectionId == collection.ComplexId);

            if (repeatingPhaseItem == null)
            {
                repeatingPhaseItem = new RepeatingCollectionInfo(
                    complexCollectionId,
                    collection.Title,
                    phase.IsRepeat(),
                    isRepeatable,
                    collection.ThemeId);
                
                collectionItem.RepeatingCollections.Add(repeatingPhaseItem);
            }

            repeatingPhaseItem.OnQueueItemFound(dateWithUserOffset);
        }

        return new GetRepeatCollectionsCommandResponseV2()
        {
            ScheduleId = scheduleId,
            LateCollections = lateCollections.Values.ToList(),
            RepeatingForgottenWordsCollections = repeatingForgottenWordsCollections.Values.ToList(),
            RepeatingInfosByDate = dateToCollectionItem.Values.ToList(),
        };
    }
}