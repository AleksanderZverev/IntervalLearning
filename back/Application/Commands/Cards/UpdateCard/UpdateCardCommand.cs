using Domain.Card;
using DomainServices.DB.Repositories.Study;
using FluentResults;
using FluentResults.Extensions;
using GlobalTools.Errors;
using GlobalTools.Extensions;

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
                card.Tags = request.Tags;
                return card.ToResult();
            })
            .Bind(updatedCard => studyRepository.Cards.UpdateAndSave(updatedCard));
    }
}