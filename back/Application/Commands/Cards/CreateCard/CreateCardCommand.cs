using Application.Common.Interfaces.DB.Repositories.Cards;
using Application.Common.Interfaces.DB.Repositories.Study;
using Domain.Card;
using FluentResults;

namespace Application.Commands.Cards.CreateCard;

public class CreateCardCommand : ICommand<CreateCardRequest, Card>
{
    private readonly IStudyRepository studyRepository;

    public CreateCardCommand(IStudyRepository studyRepository)
    {
        this.studyRepository = studyRepository;
    }

    public async Task<Result<Card>> Handle(CreateCardRequest request)
    {
        return Result.Ok()
            .Bind(() => studyRepository.Cards.GetUniqueId(new CardIdParams(request.ParentUserId, request.ParentCollectionId)))
            .Bind(cardId =>
            {
                var card = new Card(request.ParentUserId, request.ParentCollectionId, cardId)
                {
                    MeaningText = request.MeaningText,
                    RememberingText = request.RememberingText,
                    PromptText = request.PromptText,
                    Description = request.Description,
                };

                if (request.Examples is { Count: > 0 })
                {
                    card.Examples = request.Examples;
                }

                return Result.Ok(card);
            })
            .Bind(card => studyRepository.Cards.Add(card));
    }
}