using Domain.RelearningCard;
using DomainServices.BoundedContext.Study.CardRepeatQueueService;
using DomainServices.DB.Repositories.Study;
using DomainServices.DB.Transactions;
using FluentResults;
using GlobalTools.Errors;

namespace Application.Commands.Cards.RelearnCard;

public class RelearnCardCommand : ICommand<RelearnCardCommandRequest>
{
    private readonly CardRepeatQueueService cardRepeatQueueService;
    private readonly IStudyRepository studyRepository;
    private readonly ITransactionProvider transactionProvider;

    public RelearnCardCommand(
        CardRepeatQueueService cardRepeatQueueService,
        IStudyRepository studyRepository, 
        ITransactionProvider transactionProvider)
    {
        this.cardRepeatQueueService = cardRepeatQueueService;
        this.studyRepository = studyRepository;
        this.transactionProvider = transactionProvider;
    }

    public async Task<Result> Handle(RelearnCardCommandRequest request)
    {
        var (userId, collectionId, cardId, scheduleUserId, scheduleId) = request;
        var collection = await studyRepository.Query.Collections.Find(userId, collectionId);

        if (collection == null)
        {
            return new NotFoundError("Collection");
        }

        var card = await studyRepository.Query.Cards.Find(userId, collectionId, cardId);

        if (card == null)
        {
            return new NotFoundError("Card");
        }

        using var transaction = transactionProvider.CreateScope();

        if (scheduleUserId != null && scheduleId != null)
        {
            var schedule = await studyRepository.Query.Schedules.Find(scheduleUserId, scheduleId);

            if (schedule == null)
                return new BadRequestError("Schedule is not found");

            var stoppingRepeatingResult = await cardRepeatQueueService.StopRepeatingCard(card, schedule);

            if (stoppingRepeatingResult.IsFailed)
                return stoppingRepeatingResult;
        }
        
        var existingRelearnCard = await studyRepository.Query.RelearningCards.Find(userId, collectionId, cardId);

        if (existingRelearnCard != null)
        {
            transaction.Complete();
            return Result.Ok();
        }

        var addingResult = studyRepository.RelearnCards.AddAndSave(new RelearningCard(userId, collectionId, cardId));

        if (addingResult.IsFailed)
            return addingResult.ToResult();
        
        transaction.Complete();
        return Result.Ok();
    }
}