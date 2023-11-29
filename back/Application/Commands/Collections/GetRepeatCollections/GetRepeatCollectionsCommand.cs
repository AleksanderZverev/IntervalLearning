using Application.Common.Interfaces.DB.Repositories.Study;
using Application.Common.Interfaces.Domain.Collections;
using Application.Common.Interfaces.Domain.Study.Queue;
using FluentResults;

namespace Application.Commands.Collections.GetRepeatCollections;

public class GetRepeatCollectionsCommand : ICommand<GetRepeatCollectionsRequest, Dictionary<DateTime, List<RepeatingPhase>>>
{
    private readonly IStudyQueryRepository studyQueryRepository;

    public GetRepeatCollectionsCommand(
        IStudyQueryRepository studyQueryRepository)
    {
        this.studyQueryRepository = studyQueryRepository;
    }

    public async Task<Result<Dictionary<DateTime, List<RepeatingPhase>>>> Handle(GetRepeatCollectionsRequest request)
    {
        var userId = request.UserId;
        
        var queueItems = await studyQueryRepository.RepeatingQueue.GetAll(userId);

        var collectionIds = queueItems
            .Select(q => q.ParentCollectionId)
            .Distinct()
            .ToList();

        var collections = await studyQueryRepository.Collections.GetRange(userId, collectionIds);
        var collectionIdToCollection = collections.ToDictionary(c => c.Id);

        var result = new Dictionary<DateTime, List<RepeatingPhase>>();

        foreach (var queueItem in queueItems)
        {
            var date = queueItem.Date.Date;
            var schedule = queueItem.ParentRepeatsSchedule;
            var phase = schedule.GetPhase(queueItem.PhaseIndex);

            if (!result.ContainsKey(date))
            {
                result.Add(date, new List<RepeatingPhase>());
            }

            var repeatingPhasesList = result[date];

            var repeatingPhase = repeatingPhasesList.SingleOrDefault(r =>
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
                repeatingPhase.RepeatingCollections.SingleOrDefault(q =>
                    q.Collection.Id == queueItem.ParentCollectionId);

            if (repeatingCollection == null)
            {
                repeatingCollection = new RepeatingCollection(collection);
                repeatingPhase.RepeatingCollections.Add(repeatingCollection);
            }

            repeatingCollection.CardsToRepeatCount++;
        }

        return result;
    }
}