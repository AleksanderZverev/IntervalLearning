using Domain.Schedule;
using DomainServices.DB.Repositories.Study;

namespace DomainServices.BoundedContext.Study.CardRepeatQueueService;

public class CardRepeatQueueService
{
    private readonly IStudyRepository studyRepository;

    public CardRepeatQueueService(IStudyRepository studyRepository)
    {
        this.studyRepository = studyRepository;
    }
    
    public CardRepeatQueue Create(RepeatsSchedule scheduleWithPhases, Card card, int nextPhaseIndex, DateTime nextRepeatDate)
    {
        var queueId = studyRepository.RepeatingQueue.GetUniqueId(new(scheduleWithPhases, card)).Value;
        
        var queueItem = new CardRepeatQueue(
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