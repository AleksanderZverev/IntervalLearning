using System.Diagnostics;
using Domain.Card;
using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.Schedule;
using Domain.Schedule.Entities.Phase;
using Domain.Schedule.Entities.Remember.ValueObjects;
using Domain.User.ValueObjects;
using DomainServices.BoundedContext.Study.CardRepeatQueueService;
using DomainServices.BoundedContext.Study.RememberService;
using DomainServices.DB.Repositories.Study;
using DomainServices.DB.Transactions;
using FluentResults;
using GlobalTools.Errors;

namespace Application.Commands.Cards.StartLearnCards;

public class StartLearnCardsCommand : ICommand<StartLearnCardsRequest, NextRepeatInfoResponse>
{
    private readonly IStudyRepository studyRepository;
    private readonly CardRepeatQueueService cardRepeatQueueService;
    private readonly RememberService rememberService;
    private readonly ITransactionProvider transactionProvider;

    public StartLearnCardsCommand(
        ITransactionProvider transactionProvider, 
        IStudyRepository studyRepository,
        CardRepeatQueueService cardRepeatQueueService,
        RememberService rememberService)
    {
        this.transactionProvider = transactionProvider;
        this.studyRepository = studyRepository;
        this.cardRepeatQueueService = cardRepeatQueueService;
        this.rememberService = rememberService;
    }

    public async Task<Result<NextRepeatInfoResponse>> Handle(StartLearnCardsRequest request)
    {
        var (userId, collectionId, scheduleUserId, scheduleId, cardIds) = request;
        
        var schedule = await studyRepository.Query.Schedules.Find(scheduleUserId, scheduleId);

        if (schedule == null)
        {
            return new NotFoundError("Schedule");
        }

        if (schedule.Phases.Count == 0)
        {
            return new NotFoundError("Phases");
        }

        var startedCards = await studyRepository.Query.Cards.GetRange(userId, collectionId, cardIds);

        if (startedCards.Count == 0)
        {
            return new NotFoundError(nameof(Card));
        }

        using var transaction = transactionProvider.CreateScope();

        var startedDate = DateTime.UtcNow;

        studyRepository.CardRemembers.AddRange(startedCards
            .Select(c => rememberService.CreateLearnedRemember(schedule, c, RememberWeight.Create(1f).Value, startedDate))
            .ToList());

        var addRemembersResult = await studyRepository.SaveChangesAsync();
        if (addRemembersResult.IsFailed)
        {
            return new InternalError();
        }

        var nextRepeatInfoResult = AddToQueue(startedCards, schedule);

        if (nextRepeatInfoResult.IsFailed)
        {
            return nextRepeatInfoResult;
        }

        var relearningCardIdsDeletedResult = await DeleteRelearningItem(userId, collectionId, cardIds);

        if (relearningCardIdsDeletedResult.IsFailed)
            return relearningCardIdsDeletedResult;

        var startingCardsResult = await studyRepository.SaveChangesAsync();
        if (startingCardsResult.IsFailed)
            return startingCardsResult;
        
        transaction.Complete();
        return nextRepeatInfoResult.Value;
    }

    private async Task<Result> DeleteRelearningItem(UserId userId, CollectionId collectionId, List<CardId> cardIds)
    {
        var relearningCards = await studyRepository.Query.RelearningCards.GetAllFor(userId, collectionId);
        var relearningToDelete = relearningCards.Where(c => cardIds.Contains(c.CardId)).ToList();
        studyRepository.RelearnCards.DeleteRange(relearningToDelete);
        return await studyRepository.SaveChangesAsync();
    }
    
    private Result<NextRepeatInfoResponse> AddToQueue(
        List<Card> cards,
        RepeatsSchedule scheduleWithPhases)
    {
        var closestRepeatDate = DateTime.MaxValue;
        var closestPhaseIndex = -1;
        Phase? closestPhaseInfo = null;

        var dateToRepeatingInfo = new Dictionary<DateTime, CardMovementInfo>();

        foreach (var card in cards)
        {
            var stopRepeatingResult = cardRepeatQueueService.StopRepeatingCard(card, scheduleWithPhases).GetAwaiter().GetResult();

            if (stopRepeatingResult.IsFailed)
                return stopRepeatingResult;
            
            var startRepeatingResult = cardRepeatQueueService.StartRepeatingCard(card, scheduleWithPhases).GetAwaiter().GetResult();

            if (startRepeatingResult.IsFailed)
                return startRepeatingResult.ToResult();

            var nextRepeatDateQueue = startRepeatingResult.Value;
            var nextRepeatDate = nextRepeatDateQueue.Date;

            dateToRepeatingInfo.TryAdd(nextRepeatDate.Date, new CardMovementInfo(new List<CardId>(), nextRepeatDate));
            var repeatingInfo = dateToRepeatingInfo[nextRepeatDate.Date];
            repeatingInfo.CardIds.Add(card.Id);

            if (nextRepeatDate <= closestRepeatDate)
            {
                closestRepeatDate = nextRepeatDate;
                closestPhaseInfo = scheduleWithPhases.GetPhaseByIndex(nextRepeatDateQueue.PhaseIndex);
                closestPhaseIndex = nextRepeatDateQueue.PhaseIndex;
            }
        }
        
        var addedQueuesResult = studyRepository.SaveChanges();
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
            CardMovementInfos = dateToRepeatingInfo.Values.ToList(),
        };
    }
}