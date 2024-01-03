using Domain.Schedule;
using DomainServices.DB.Repositories.Study;

namespace DomainServices.Study.CardRepeatQueue;

public class CardRepeatQueueService
{
    private readonly IStudyRepository studyRepository;

    public CardRepeatQueueService(IStudyRepository studyRepository)
    {
        this.studyRepository = studyRepository;
    }
    
    public Domain.Queue.CardRepeatQueue Create(RepeatsSchedule scheduleWithPhases, Card card, int nextPhaseIndex, DateTime nextRepeatDate)
    {
        var queueId = studyRepository.RepeatingQueue.GetUniqueId(new(scheduleWithPhases, card)).Value;
        
        var queueItem = new Domain.Queue.CardRepeatQueue(
            scheduleWithPhases.ParentUserId,
            scheduleWithPhases.Id,
            card.ParentUserId,
            card.ParentCollectionId,
            card.Id,
            queueId,
            (short)nextPhaseIndex,
            nextRepeatDate
        );
        
        return queueItem;
    }
}