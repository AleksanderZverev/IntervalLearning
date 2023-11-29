using Application.Common.Interfaces.DB.Repositories.Study;
using Application.Common.Interfaces.Domain.Cards;
using Domain.Card;
using FluentResults;
using FluentResults.Extensions;
using Infrastructure.Errors;
using Infrastructure.Extensions;

namespace Application.Commands.Cards.UpdateCard;

public class UpdateCardCommand : ICommand<UpdateCardRequest, Card>
{
    private readonly IStudyRepository studyRepository;

    public UpdateCardCommand(
        IStudyRepository studyRepository)
    {
        this.studyRepository = studyRepository;
    }

    public async Task<Result<Card>> Handle(UpdateCardRequest request)
    {
        return await studyRepository.Query.Cards
            .Find(request.ParentUserId, request.ParentCollectionId, request.CardId)
            .ToResultAsync()
            .ErrorIfNull(new NotFoundError(nameof(Card)))
            .Bind(card =>
            {
                card.MeaningText = request.MeaningText;
                card.RememberingText = request.RememberingText;
                card.PromptText = request.PromptText;
                card.Description = request.Description;
                card.Examples = request.Examples;
                return card.ToResult();
            })
            .Bind(updatedCard => studyRepository.Cards.Update(updatedCard));
    }
}