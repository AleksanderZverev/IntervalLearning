using System.Diagnostics;
using DomainServices.DB.Queries.Study;
using FluentResults;
using GlobalTools;

namespace Application.Commands.Collections.GetRepeatCollections;

[Obsolete("Look V2 version")]
public class GetRepeatCollectionsCommand : ICommand<GetRepeatCollectionsCommandRequest,
    Dictionary<DateTime, List<RepeatingPhase>>>
{
    private readonly IDateTimeProvider dateTimeProvider;
    private readonly IStudyQueryRepository studyQueryRepository;

    public GetRepeatCollectionsCommand(
        IDateTimeProvider dateTimeProvider,
        IStudyQueryRepository studyQueryRepository)
    {
        this.dateTimeProvider = dateTimeProvider;
        this.studyQueryRepository = studyQueryRepository;
    }

    public async Task<Result<Dictionary<DateTime, List<RepeatingPhase>>>> Handle(
        GetRepeatCollectionsCommandRequest request)
    {
        var (userId, untilDate) = request;

        var queueItems = await studyQueryRepository.RepeatingQueue.GetAll(userId);

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

        var dateToPhases = new Dictionary<DateTime, List<RepeatingPhase>>();

        foreach (var queueItem in queueItems)
        {
            var date = queueItem.Date.Date;
            var schedule = queueItem.ParentRepeatsSchedule;
            var phase = schedule.GetPhaseByIndex(queueItem.PhaseIndex);

            if (!dateToPhases.ContainsKey(date))
            {
                dateToPhases.Add(date, new List<RepeatingPhase>());
            }

            var repeatingPhasesList = dateToPhases[date];

            var repeatingPhase = repeatingPhasesList.SingleOrDefault(
                r =>
                    r.ScheduleUserId == queueItem.ParentRepeatsScheduleUserId
                    && r.ScheduleId == queueItem.ParentRepeatsScheduleId
                    && r.PhaseIndex == queueItem.PhaseIndex);

            if (repeatingPhase == null)
            {
                repeatingPhase = new RepeatingPhase(
                    queueItem.ParentRepeatsScheduleUserId,
                    queueItem.ParentRepeatsScheduleId,
                    queueItem.PhaseIndex,
                    phase.SecondsFromLastPhase,
                    phase.OnLearnDescription,
                    new());

                repeatingPhasesList.Add(repeatingPhase);
            }

            var collection = collectionIdToCollection[queueItem.ParentCollectionId];

            var repeatingCollection =
                repeatingPhase.RepeatingCollections.SingleOrDefault(
                    q =>
                        q.Collection.Id == queueItem.ParentCollectionId);

            if (repeatingCollection == null)
            {
                repeatingCollection = new RepeatingCollection(collection);
                repeatingPhase.RepeatingCollections.Add(repeatingCollection);
            }

            repeatingCollection.CardsToRepeatCount++;

            var isRepeatableResult = schedule.CanRepeat(queueItem.PhaseIndex, date, DateTimeOffset.UtcNow);
            Debug.Assert(isRepeatableResult.IsSuccess);
            repeatingCollection.IsRepeatable = isRepeatableResult.ValueOrDefault;
        }

        return dateToPhases;
    }
}