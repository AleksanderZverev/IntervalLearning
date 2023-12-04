using Application.Commands.Cards.StartLearnCards;
using Application.Common.Interfaces.DB.Repositories.Study;
using Application.Common.Interfaces.DB.Transactions;
using DB.Models;
using DB.Models.ValueObjects;
using Domain.Card;
using Domain.Card.ValueObjects;
using Domain.Queue;
using Domain.Schedule;
using Domain.Schedule.Entities.Remember;
using FluentResults;
using Infrastructure.Errors;

namespace Application.Commands.Cards.RememberCard;

public class RememberCardCommand : ICommand<RememberCardRequest, NextRepeatInfoResponse>
{
    private readonly IStudyRepository studyRepository;
    private readonly ITransactionProvider transactionProvider;

    public RememberCardCommand(
        ITransactionProvider transactionProvider, 
        IStudyRepository studyRepository)
    {
        this.transactionProvider = transactionProvider;
        this.studyRepository = studyRepository;
    }

    public async Task<Result<NextRepeatInfoResponse>> Handle(RememberCardRequest request)
    {
        var (userId, collectionId, scheduleUserId, scheduleId, phaseIndex, rememberItems, allowRepeatingInFuture) = request;

        var cardIds = rememberItems.Select(c => c.CardId).Distinct().ToList();

        var collection = await studyRepository.Query.Collections.Find(userId, collectionId);

        if (collection == null)
        {
            return new NotFoundError("Card's collection");
        }

        var queueItems = await studyRepository.Query.RepeatingQueue.GetForCards(
            userId, collectionId, scheduleUserId, scheduleId, phaseIndex, cardIds);

        if (queueItems.Count == 0 || queueItems.Count != cardIds.Count)
            return new BadRequestError();

        var schedule = await studyRepository.Query.Schedules.Find(scheduleUserId, scheduleId);

        if (schedule == null)
        {
            return new NotFoundError("Schedule");
        }
        
        using var transaction = transactionProvider.CreateScope();
        
        var closestRepeatDate = DateTime.MaxValue;
        var closestPhaseIndex = -1;
        Phase? closestPhaseInfo = null;

        var now = DateTime.UtcNow;
        var forbidDate = now.Date.AddDays(1);

        foreach (var rememberItem in rememberItems)
        {
            var weight = rememberItem.Weight;
            var cardId = CardId.Create(rememberItem.CardId).Value;
            var card = await studyRepository.Query.Cards.Find(userId, collectionId, cardId);

            if (card == null)
            {
                return new InternalError();
            }

            var queueItem = queueItems.Single(q => q.ParentCardId == cardId);

            if (!allowRepeatingInFuture && queueItem.Date.Date >= forbidDate)
            {
                // logger.LogInformation("Unable to remember. Not time!");
                return new BadRequestError("It's too early to repeat now");
            }
            
            var remember = CreateRemember(schedule, card, weight, queueItem.PhaseIndex, now);
            studyRepository.CardRemembers.Add(remember);
            studyRepository.RepeatingQueue.Delete(queueItem);
            
            var currentPhase = schedule.GetPhase(queueItem.PhaseIndex);
            
            var phaseRemember = new PhaseRememberEntity(
                schedule.ParentUserId,
                schedule.Id,
                currentPhase.Id,
                userId,
                weight);

            studyRepository.PhaseRemembers.Add(phaseRemember);

            var (nextPhaseIndex, nextPhase) = schedule.GetNextPhase(card, remember);

            if (nextPhase == null)
                continue;

            var nextRepeatDate = nextPhase.GetNextDate(now);
            var newQueueItem = CreateNextQueueItem(schedule, card, nextPhaseIndex, nextRepeatDate);
            
            studyRepository.RepeatingQueue.Add(newQueueItem);

            if (nextRepeatDate < closestRepeatDate)
            {
                closestRepeatDate = nextRepeatDate;
                closestPhaseInfo = nextPhase;
                closestPhaseIndex = nextPhaseIndex;
            }
        }

        var saveResult = await studyRepository.SaveChangesAsync();

        if (saveResult.IsFailed)
            return saveResult;

        transaction.Complete();

        return new NextRepeatInfoResponse
        {
            NextPhase = closestPhaseInfo,
            NextPhaseIndex = closestPhaseIndex,
            NextRepeatDate = closestRepeatDate == DateTime.MaxValue ? null : closestRepeatDate,
        };
    }
    
    private CardRepeatQueue CreateNextQueueItem(RepeatsSchedule scheduleWithPhases, Card card, int nextPhaseIndex, DateTime nextRepeatDate)
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
    
    private Remember CreateRemember(RepeatsSchedule schedule, Card card, RememberWeight weight, int phaseIndex, DateTime date)
    {
        var rememberId = studyRepository.CardRemembers.GetUniqueId(new(schedule, card)).Value;
        
        return new Remember(
            schedule.ParentUserId, 
            schedule.Id,
            card.ParentUserId,
            card.ParentCollectionId,
            card.Id,
            rememberId,
            weight, 
            (short)phaseIndex,
            date);
    }
}