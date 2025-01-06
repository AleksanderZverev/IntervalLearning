using Domain.Card;
using DomainServices.DB.Repositories.Study;
using DomainServices.DB.Repositories.Study.Cards;
using FluentResults;

namespace Application.Commands.Cards.CreateCard;

public class CreateCardCommand : ICommand<CreateCardRequest, Card>
{
    private readonly IStudyRepository studyRepository;

    public CreateCardCommand(IStudyRepository studyRepository)
    {
        this.studyRepository = studyRepository;
    }

    public Task<Result<Card>> Handle(CreateCardRequest request)
    {
        return Task.FromResult(
            Result.Ok()
                .Bind(
                    () => studyRepository.Cards.GetUniqueId(
                        new CardIdParams(request.ParentUserId, request.ParentCollectionId)))
                .Bind(
                    cardId =>
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

                        if (request.Tags is { Count: > 0 })
                        {
                            card.Tags = request.Tags;
                        }

                        return Result.Ok(card);
                    })
                .Bind(card => studyRepository.Cards.AddAndSave(card)));
    }
}