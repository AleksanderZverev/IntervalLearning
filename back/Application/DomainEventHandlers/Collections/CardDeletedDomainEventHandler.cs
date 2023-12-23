using Application.Common.Interfaces.DB.Repositories.Study;
using Domain.Card.Events;
using FluentResults;
using FluentResults.Extensions;
using Infrastructure.Errors;
using Infrastructure.Extensions;

namespace Application.DomainEventHandlers.Collections;

public class CardDeletedDomainEventHandler : IDomainEventHandler<CardDeletedEvent>
{
    private readonly IStudyRepository studyRepository;

    public CardDeletedDomainEventHandler(IStudyRepository studyRepository)
    {
        this.studyRepository = studyRepository;
    }

    public async Task<Result> Handle(CardDeletedEvent domainEvent)
    {
        var card = domainEvent.Card;
        return await studyRepository.Query.Collections.Find(card.ParentUserId, card.ParentCollectionId)
            .ToResultAsync()
            .ErrorIfNull(new NotFoundError("Collection"))
            .Bind(collection =>
            {
                collection.CardsCount.Decrement();
                studyRepository.Collections.Update(collection);
                return Result.Ok();
            });
    }
}