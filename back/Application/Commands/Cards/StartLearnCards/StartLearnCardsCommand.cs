using System.Diagnostics;
using Application.Common.Interfaces.DB.Transactions;
using Application.Common.Interfaces.Domain.Cards;
using Application.Common.Interfaces.Domain.Study.Queue;
using Application.Common.Interfaces.Domain.Study.Remember;
using Application.Common.Interfaces.Domain.Study.Schedule;
using DB.Models;
using DB.Models.ValueObjects;
using Domain.Card;
using Domain.Queue;
using Domain.Schedule;
using Domain.Schedule.Entities.Remember;
using FluentResults;
using Infrastructure.Errors;

namespace Application.Commands.Cards.StartLearnCards;

public record NextRepeatInfoResponse
{
    public DateTime? NextRepeatDate { get; init; }
    public int NextPhaseIndex { get; init; }
    public Phase? NextPhase { get; init; }
}

public class StartLearnCardsCommand : ICommand<StartLearnCardsRequest, NextRepeatInfoResponse>
{
    private readonly IScheduleResolver scheduleResolver;
    private readonly ICardsQueryResolver cardsQueryResolver;
    private readonly ICardsMutationResolver cardsMutationResolver;
    private readonly IRepeatingQueueMutationResolver queueMutationResolver;
    private readonly IRememberMutationResolver rememberMutationResolver;
    private readonly ITransactionProvider transactionProvider;

    public StartLearnCardsCommand(
        IScheduleResolver scheduleResolver,
        ICardsQueryResolver cardsQueryResolver,
        ICardsMutationResolver cardsMutationResolver,
        IRepeatingQueueMutationResolver queueMutationResolver,
        IRememberMutationResolver rememberMutationResolver,
        ITransactionProvider transactionProvider)
    {
        this.scheduleResolver = scheduleResolver;
        this.cardsQueryResolver = cardsQueryResolver;
        this.cardsMutationResolver = cardsMutationResolver;
        this.queueMutationResolver = queueMutationResolver;
        this.rememberMutationResolver = rememberMutationResolver;
        this.transactionProvider = transactionProvider;
    }

    public async Task<Result<NextRepeatInfoResponse>> Handle(StartLearnCardsRequest request)
    {
        var (userId, collectionId, scheduleUserId, scheduleId, cardIds) = request;
        
        var schedule = await scheduleResolver.FindAsync(scheduleUserId, scheduleId);

        if (schedule == null)
        {
            return new NotFoundError("Schedule");
        }

        if (schedule.Phases.Count == 0)
        {
            return new NotFoundError("Phases");
        }

        var startedCards = await cardsQueryResolver.GetRange(userId, collectionId, cardIds);

        if (startedCards.Count == 0)
        {
            return new NotFoundError(nameof(Card));
        }

        using var transaction = transactionProvider.CreateScope();

        var startedDate = DateTime.UtcNow;

        var addRemembersResult = rememberMutationResolver.AddRange(startedCards
            .Select(c => CreateRemember(schedule, c, RememberWeight.Create(1f).Value, -1, startedDate))
            .ToList());

        if (addRemembersResult.IsFailed)
        {
            return new InternalError();
        }

        var nextRepeatInfoResult = AddToQueue(startedCards, schedule);

        if (nextRepeatInfoResult.IsFailed)
        {
            return nextRepeatInfoResult;
        }

        transaction.Complete();
        return nextRepeatInfoResult.Value;
    }
    
    private Result<NextRepeatInfoResponse> AddToQueue(
        List<Card> cards,
        RepeatsSchedule scheduleWithPhases)
    {
        var closestRepeatDate = DateTime.MaxValue;
        var closestPhaseIndex = -1;
        Phase? closestPhaseInfo = null;
        var queueItems = new List<CardRepeatQueue>(cards.Count);

        foreach (var card in cards)
        {
            var (startPhase, phaseIndex) = scheduleWithPhases.FindFirstPhase();

            if (startPhase == null)
            {
                Debug.Fail("An error in algorithm work: nextPhase == null");
                return new InternalError();
            }

            var nextRepeatDate = startPhase.GetNextDate(DateTime.UtcNow);
            var nextQueueItem = GetNextQueue(
                scheduleWithPhases,
                card,
                phaseIndex,
                nextRepeatDate);
            queueItems.Add(nextQueueItem);
            
            if (nextRepeatDate <= closestRepeatDate)
            {
                closestRepeatDate = nextRepeatDate;
                closestPhaseInfo = startPhase;
                closestPhaseIndex = phaseIndex;
            }
        }

        var addedQueuesResult = queueMutationResolver.AddRange(queueItems);

        if (addedQueuesResult.IsFailed)
        {
            return new InternalError();
        }

        Debug.Assert(closestRepeatDate != DateTime.MaxValue, "closestRepeatDate != DateTime.MaxValue");
        return new NextRepeatInfoResponse
        {
            NextPhase = closestPhaseInfo,
            NextPhaseIndex = closestPhaseIndex,
            NextRepeatDate = closestRepeatDate,
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