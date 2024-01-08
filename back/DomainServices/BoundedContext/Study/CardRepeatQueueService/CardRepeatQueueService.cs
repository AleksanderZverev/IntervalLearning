using Domain.Card;
using Domain.Queue;
using Domain.Schedule;
using DomainServices.DB.Repositories.Study;
using FluentResults;

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

    public async Task<Result<CardRepeatQueue>> StartRepeatingCard(Card card, RepeatsSchedule repeatsSchedule)
    {
        var startPhase = repeatsSchedule.GetFirstPhase();
        var phaseIndex = repeatsSchedule.IndexOf(startPhase);
        
        var nextRepeatDate = startPhase.GetNextDate(DateTime.UtcNow);
        var nextQueueItem = Create(
            repeatsSchedule,
            card,
            phaseIndex,
            nextRepeatDate);
        studyRepository.RepeatingQueue.Add(nextQueueItem);
        var saveResult = await studyRepository.SaveChangesAsync();
        
        return saveResult.IsSuccess
            ? nextQueueItem
            : saveResult;
    }

    public async Task<Result> StopRepeatingCard(Card card, RepeatsSchedule repeatsSchedule)
    {
        var existingCardQueues = await studyRepository.Query.RepeatingQueue.GetAllForCard(
            card.ParentUserId,
            card.ParentCollectionId,
            card.Id,
            repeatsSchedule.ParentUserId,
            repeatsSchedule.Id);

        if (existingCardQueues.Count > 0)
        {
            studyRepository.RepeatingQueue.DeleteRange(existingCardQueues);
            return await studyRepository.SaveChangesAsync();
        }
        
        return Result.Ok();
    }
}