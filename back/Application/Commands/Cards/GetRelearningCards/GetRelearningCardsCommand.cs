using Application.Common.Interfaces.DB.Queries.Study;
using Domain.Card;
using FluentResults;

namespace Application.Commands.Cards.GetRelearningCards;

public class GetRelearningCardsCommand : ICommand<GetRelearningCardsCommandRequest, List<Card>>
{
    private readonly IStudyQueryRepository studyQueryRepository;

    public GetRelearningCardsCommand(IStudyQueryRepository studyQueryRepository)
    {
        this.studyQueryRepository = studyQueryRepository;
    }

    public async Task<Result<List<Card>>> Handle(GetRelearningCardsCommandRequest request)
    {
        var (userId, collectionId, count) = request;
        var relearningCards = await studyQueryRepository.RelearningCards.GetAllFor(userId, collectionId);
        return await studyQueryRepository.Cards.GetRange(
            userId,
            collectionId,
            relearningCards.Select(c => c.CardId).Take(count).ToList()
        );
    }
}