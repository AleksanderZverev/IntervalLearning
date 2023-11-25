using Application.Commands.Cards.StartLearnCards;
using Application.Common.Interfaces.DB.Transactions;
using Application.Common.Interfaces.Domain.Cards;
using Application.Common.Interfaces.Domain.Collections;
using Application.Common.Interfaces.Domain.Study.PhaseRemember;
using Application.Common.Interfaces.Domain.Study.Queue;
using Application.Common.Interfaces.Domain.Study.Remember;
using Application.Common.Interfaces.Domain.Study.Schedule;
using DB.Models;
using DB.Models.ValueObjects;
using Domain.Card;
using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.Queue;
using Domain.Schedule;
using Domain.Schedule.Entities.Remember;
using Domain.User.ValueObjects;
using FluentResults;
using Infrastructure.Errors;

namespace Application.Commands.Cards.RememberCard;

public class RememberItem
{
    public required CardId CardId { get; init; }
    public required RememberWeight  Weight { get; init; }
}

public record RememberCardRequest(
    UserId UserId,
    CollectionId CollectionId,
    UserId ScheduleUserId,
    ScheduleId ScheduleId,
    short PhaseIndex,
    List<RememberItem> RememberItems,
    bool AllowRepeatingInFuture
);

public class RememberCardCommand : ICommand<RememberCardRequest, NextRepeatInfoResponse>
{
    private readonly ICardsQueryResolver cardsQueryResolver;
    private readonly ICollectionQueryResolver collectionQueryResolver;
    private readonly IRepeatingQueueResolver queueResolver;
    private readonly IScheduleResolver scheduleResolver;
    private readonly IRememberMutationResolver rememberMutationResolver;
    private readonly IRepeatingQueueMutationResolver queueMutationResolver;
    private readonly IPhaseRememberMutationResolver phaseRememberMutationResolver;
    private readonly ITransactionProvider transactionProvider;

    public RememberCardCommand(
        ICardsQueryResolver cardsQueryResolver,
        ICollectionQueryResolver collectionQueryResolver,
        IRepeatingQueueResolver queueResolver,
        IScheduleResolver scheduleResolver,
        IRememberMutationResolver rememberMutationResolver,
        IRepeatingQueueMutationResolver queueMutationResolver,
        IPhaseRememberMutationResolver phaseRememberMutationResolver,
        ITransactionProvider transactionProvider)
    {
        this.cardsQueryResolver = cardsQueryResolver;
        this.collectionQueryResolver = collectionQueryResolver;
        this.queueResolver = queueResolver;
        this.scheduleResolver = scheduleResolver;
        this.rememberMutationResolver = rememberMutationResolver;
        this.queueMutationResolver = queueMutationResolver;
        this.phaseRememberMutationResolver = phaseRememberMutationResolver;
        this.transactionProvider = transactionProvider;
    }

    public async Task<Result<NextRepeatInfoResponse>> Handle(RememberCardRequest request)
    {
        var (userId, collectionId, scheduleUserId, scheduleId, phaseIndex, rememberItems, allowRepeatingInFuture) = request;

        var cardIds = rememberItems.Select(c => c.CardId).Distinct().ToList();

        var collection = await collectionQueryResolver.Find(userId, collectionId);

        if (collection == null)
        {
            return new NotFoundError("Card's collection");
        }

        //GetForCards
        var queueItems = await queueResolver.GetForCards(
            userId, collectionId, scheduleUserId, scheduleId, phaseIndex, cardIds);

        if (queueItems.Count == 0 || queueItems.Count != cardIds.Count)
            return new BadRequestError();

        var schedule = await scheduleResolver.FindAsync(scheduleUserId, scheduleId);

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
            var card = await cardsQueryResolver.Find(userId, collectionId, cardId);

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
            var addRememberResult = rememberMutationResolver.Add(remember);
            var removeQueueResult = queueMutationResolver.Delete(queueItem);

            if (addRememberResult.IsFailed || removeQueueResult.IsFailed)
                return new InternalError();
            
            var currentPhase = schedule.GetPhase(queueItem.PhaseIndex);
            
            var phaseRemember = new PhaseRememberEntity(
                schedule.ParentUserId,
                schedule.Id,
                currentPhase.Id,
                userId,
                weight);

            var phaseRememberAddResult = phaseRememberMutationResolver.Add(phaseRemember);

            if (phaseRememberAddResult.IsFailed)
            {
                //LOG WARN
            }

            var (nextPhaseIndex, nextPhase) = schedule.GetNextPhase(card, remember);

            if (nextPhase == null)
                continue;

            var nextRepeatDate = nextPhase.GetNextDate(now);
            var newQueueItem = GetNextQueue(schedule, card, nextPhaseIndex, nextRepeatDate);
            
            var addNewQueueResult = queueMutationResolver.Add(newQueueItem);

            if (addNewQueueResult.IsFailed)
                return new InternalError();

            if (nextRepeatDate < closestRepeatDate)
            {
                closestRepeatDate = nextRepeatDate;
                closestPhaseInfo = nextPhase;
                closestPhaseIndex = nextPhaseIndex;
            }
        }

        // if (!await db.SoftSaveChangesAsync())
        //     return new InternalError();

        transaction.Complete();

        return new NextRepeatInfoResponse
        {
            NextPhase = closestPhaseInfo,
            NextPhaseIndex = closestPhaseIndex,
            NextRepeatDate = closestRepeatDate == DateTime.MaxValue ? null : closestRepeatDate,
        };
    }
    
    private CardRepeatQueue GetNextQueue(RepeatsSchedule scheduleWithPhases, Card card, int nextPhaseIndex, DateTime nextRepeatDate)
    {
        var queueId = queueMutationResolver.GetUniqueId(scheduleWithPhases, card).Value;
        
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
        var rememberId = rememberMutationResolver.GetUniqueId(schedule, card).Value;
        
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