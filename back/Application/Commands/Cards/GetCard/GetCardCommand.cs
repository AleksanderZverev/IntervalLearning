using Application.Common.Interfaces.DB.Repositories.Study;
using Application.Common.Interfaces.Domain.Cards;
using Domain.Card;
using FluentResults;
using Infrastructure.Errors;
using Infrastructure.Extensions;

namespace Application.Commands.Cards;

public class GetCardCommand : ICommand<GetCardRequest, Card>
{
    private readonly IStudyQueryRepository studyQueryRepository;

    public GetCardCommand(IStudyQueryRepository studyQueryRepository)
    {
        this.studyQueryRepository = studyQueryRepository;
    }

    public async Task<Result<Card>> Handle(GetCardRequest request)
    {
        return await studyQueryRepository.Cards.Find(request.UserId, request.CollectionId, request.CardId)
            .ToResultAsync()
            .ErrorIfNull(new NotFoundError("Card"));
    }
}