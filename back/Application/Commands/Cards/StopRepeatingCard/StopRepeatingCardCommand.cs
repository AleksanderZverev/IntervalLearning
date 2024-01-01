using Application.Common.Interfaces.DB.Repositories.Study;
using FluentResults;

namespace Application.Commands.Cards.StopRepeatingCard;

public class StopRepeatingCardCommand : ICommand<StopRepeatingCardCommandRequest>
{
    private readonly IStudyRepository studyRepository;

    public StopRepeatingCardCommand(IStudyRepository studyRepository)
    {
        this.studyRepository = studyRepository;
    }

    public async Task<Result> Handle(StopRepeatingCardCommandRequest request)
    {
        var (userId, collectionId, cardId, scheduleUserId, scheduleId) = request;
        var queueItems = await studyRepository.Query.RepeatingQueue
            .GetAllForCard(userId, collectionId, cardId, scheduleUserId, scheduleId);

        if (queueItems.Count == 0)
        {
            return Result.Ok();
        }

        studyRepository.RepeatingQueue.DeleteRange(queueItems);
        return await studyRepository.SaveChangesAsync();
    }
}