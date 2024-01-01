using Application.Common.Interfaces.DB.Repositories.Study;
using Domain.RelearningCard;
using FluentResults;
using Infrastructure.Errors;

namespace Application.Commands.Cards.RelearnCard;

public class RelearnCardCommand : ICommand<RelearnCardCommandRequest>
{
    private readonly IStudyRepository studyRepository;

    public RelearnCardCommand(IStudyRepository studyRepository)
    {
        this.studyRepository = studyRepository;
    }

    public async Task<Result> Handle(RelearnCardCommandRequest request)
    {
        var (userId, collectionId, cardId) = request;
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

        var existingRelearnCard = await studyRepository.Query.RelearningCards.Find(userId, collectionId, cardId);

        if (existingRelearnCard != null)
            return Result.Ok();

        return studyRepository.RelearnCards
            .AddAndSave(new RelearningCard(userId, collectionId, cardId))
            .ToResult();
    }
}