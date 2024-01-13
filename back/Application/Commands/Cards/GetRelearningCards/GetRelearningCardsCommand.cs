using Domain.Card;
using DomainServices.DB.Queries.Study;
using DomainServices.DB.Repositories.Study;
using FluentResults;

namespace Application.Commands.Cards.GetRelearningCards;

public class GetRelearningCardsCommand : ICommand<GetRelearningCardsCommandRequest, List<Card>>
{
    private readonly IStudyRepository studyQueryRepository;

    public GetRelearningCardsCommand(IStudyRepository studyQueryRepository)
    {
        this.studyQueryRepository = studyQueryRepository;
    }

    public async Task<Result<List<Card>>> Handle(GetRelearningCardsCommandRequest request)
    {
        var (userId, collectionId, count) = request;
        var relearningCardItems = await studyQueryRepository.Query.RelearningCards.GetAllFor(userId, collectionId);
        var searchingCardIds = relearningCardItems.Select(c => c.CardId).Take(count).ToList();
        
        var foundCards = await studyQueryRepository.Query.Cards.GetRange(
            userId,
            collectionId,
            searchingCardIds
        );

        var notFoundCardIds = searchingCardIds.Except(foundCards.Select(c => c.Id)).ToList();

        if (notFoundCardIds is { Count: > 0 })
        {
            var notFoundCardsRelearningItems = relearningCardItems
                .Where(i => notFoundCardIds.Contains(i.CardId))
                .ToList();
            
            studyQueryRepository.RelearnCards.DeleteRange(notFoundCardsRelearningItems);
            studyQueryRepository.SaveChangesAsync();
        }

        return foundCards;
    }
}