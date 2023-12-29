using Application.Common.Interfaces.DB.Repositories.Study;
using FluentResults;
using Infrastructure;
using Infrastructure.Errors;

namespace Application.Commands.Cards.PostponeRepeatingCard;

public class PostponeRepeatingCardCommand : ICommand<PostponeRepeatingCardCommandRequest>
{
    private readonly IStudyRepository studyRepository;
    private readonly IDateTimeProvider dateTimeProvider;

    public PostponeRepeatingCardCommand(IStudyRepository studyRepository, IDateTimeProvider dateTimeProvider)
    {
        this.studyRepository = studyRepository;
        this.dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> Handle(PostponeRepeatingCardCommandRequest request)
    {
        var (userId, collectionId, cardId, scheduleUserId, scheduleId, postponeDays, allowPostponeFutureRepetitions) = request;

        if (postponeDays < 1 || postponeDays > 14)
        {
            return new BadRequestError(postponeDays < 1
                ? "You can postpone card minimum on 1 day"
                : "You can't postpone card for more than 14 days"
            );
        }

        var queueItems = await studyRepository.Query.RepeatingQueue
            .GetAllForCard(userId, collectionId, cardId, scheduleUserId, scheduleId);

        if (queueItems.Count == 0)
        {
            return new BadRequestError("No repeating dates was found");
        }

        if (queueItems.Count > 1)
        {
            return new InternalError("Card has too many repeating dates");
        }

        var queueItem = queueItems.Single();
        var now = dateTimeProvider.UtcNow;
        
        if (!allowPostponeFutureRepetitions && queueItem.Date.Date > now.Date)
        {
            return new BadRequestError("You can't postpone future repetitions");
        }
        
        queueItem.PostponeOnDays(dateTimeProvider, postponeDays);
        return studyRepository.RepeatingQueue.UpdateAndSave(queueItem).ToResult();
    }
}