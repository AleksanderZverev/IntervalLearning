using Application.Commands.Cards.StartLearnCards;
using Domain.Card.ValueObjects;
using Domain.Schedule.Entities.Phase;
using Domain.Schedule.Entities.Phase.Entities;
using DomainServices.BoundedContext.Study.CardRepeatQueueService;
using DomainServices.BoundedContext.Study.RememberService;
using DomainServices.DB.Repositories.Study;
using DomainServices.DB.Transactions;
using FluentResults;
using GlobalTools.Errors;

namespace Application.Commands.Cards.RememberCard;

public class RememberCardCommand : ICommand<RememberCardRequest, NextRepeatInfoResponse>
{
    private readonly IStudyRepository studyRepository;
    private readonly CardRepeatQueueService cardRepeatQueueService;
    private readonly RememberService rememberService;
    private readonly ITransactionProvider transactionProvider;

    public RememberCardCommand(
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

        var nextRepeatingDateToInfo = new Dictionary<DateTime, CardMovementInfo>();
        
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
            
            var remember = rememberService.Create(schedule, card, weight, queueItem.PhaseIndex, now, rememberItem.Comment);
            studyRepository.CardRemembers.Add(remember);
            studyRepository.RepeatingQueue.Delete(queueItem);
            
            var currentPhase = schedule.GetPhaseByIndex(queueItem.PhaseIndex);
            
            var phaseRemember = new PhaseRememberEntity(
                schedule.ParentUserId,
                schedule.Id,
                currentPhase.Id,
                userId,
                weight);

            studyRepository.PhaseRemembers.Add(phaseRemember);
            
            card.Remembers.Add(remember);
            var nextPhase = schedule.GetNextPhase(card);

            if (nextPhase == null)
            {
                var finishDate = DateTime.MinValue;
                nextRepeatingDateToInfo.TryAdd(finishDate, new CardMovementInfo(new List<CardId>(), finishDate));
                nextRepeatingDateToInfo[finishDate.Date].CardIds.Add(cardId);
                continue;
            }

            var nextPhaseIndex = schedule.IndexOf(nextPhase);
            var nextRepeatDate = nextPhase.GetNextDate(now);
            var newQueueItem = cardRepeatQueueService.Create(schedule, card, nextPhaseIndex, nextRepeatDate);

            studyRepository.RepeatingQueue.Add(newQueueItem);

            nextRepeatingDateToInfo.TryAdd(nextRepeatDate.Date, new CardMovementInfo(new List<CardId>(), nextRepeatDate));
            var repeatingInfo = nextRepeatingDateToInfo[nextRepeatDate.Date];
            repeatingInfo.CardIds.Add(cardId);

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
            CardMovementInfos = nextRepeatingDateToInfo.Values.ToList(),
        };
    }
}