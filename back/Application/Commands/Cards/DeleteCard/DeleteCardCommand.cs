using Application.Common.Interfaces.DB.Repositories.Study;
using Domain.Card;
using FluentResults;
using FluentResults.Extensions;
using Infrastructure.Errors;
using Infrastructure.Extensions;

namespace Application.Commands.Cards.DeleteCard;

public class DeleteCardCommand : ICommand<DeleteCardRequest, Card>
{
    private readonly IStudyRepository studyRepository;

    public DeleteCardCommand(
        IStudyRepository studyRepository)
    {
        this.studyRepository = studyRepository;
    }

    public async Task<Result<Card>> Handle(DeleteCardRequest request)
    {
        return await studyRepository.Query.Cards
            .Find(request.UserId, request.CollectionId, request.CardId)
            .ToResultAsync()
            .ErrorIfNull(new NotFoundError(nameof(Card)))
            .Bind(c => studyRepository.Cards.DeleteAndSave(c));
    }
}