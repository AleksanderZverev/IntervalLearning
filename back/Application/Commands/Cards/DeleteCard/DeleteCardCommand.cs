using Application.Common.Interfaces.DB.Repositories.Study;
using Application.Common.Interfaces.Domain.Cards;
using Domain.Card;
using FluentResults;
using FluentResults.Extensions;
using Infrastructure.Errors;
using Infrastructure.Extensions;

namespace Application.Commands.Cards.DeleteCard;

public class DeleteCardCommand : ICommand<DeleteCardRequest, Card>
{
    private readonly ICardsQueryResolver cardsQueryResolver;
    private readonly IStudyRepository studyRepository;

    public DeleteCardCommand(
        ICardsQueryResolver cardsQueryResolver, 
        IStudyRepository studyRepository)
    {
        this.cardsQueryResolver = cardsQueryResolver;
        this.studyRepository = studyRepository;
    }

    public async Task<Result<Card>> Handle(DeleteCardRequest request)
    {
        return await cardsQueryResolver
            .Find(request.UserId, request.CollectionId, request.CardId)
            .ToResultAsync()
            .ErrorIfNull(new NotFoundError(nameof(Card)))
            .Bind(c => studyRepository.Cards.Delete(c));
    }
}