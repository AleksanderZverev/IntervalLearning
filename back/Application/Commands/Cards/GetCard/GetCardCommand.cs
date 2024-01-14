using Domain.Card;
using DomainServices.DB.Queries.Study;
using FluentResults;
using GlobalTools.Errors;
using GlobalTools.Extensions;

namespace Application.Commands.Cards.GetCard;

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