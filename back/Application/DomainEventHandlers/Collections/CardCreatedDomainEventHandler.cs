using Application.Common.Interfaces.DB.Repositories.Study;
using Domain.Card.Events;
using FluentResults;
using Infrastructure.Errors;

namespace Application.DomainEventHandlers.Collections;

public class CardCreatedDomainEventHandler : IDomainEventHandler<CardCreatedEvent>
{
    private readonly IStudyRepository studyRepository;

    public CardCreatedDomainEventHandler(IStudyRepository studyRepository)
    {
        this.studyRepository = studyRepository;
    }

    public async Task<Result> Handle(CardCreatedEvent domainEvent)
    {
        var card = domainEvent.Card;
        var collection = await studyRepository.Query.Collections.Find(card.ParentUserId, card.ParentCollectionId);

        if (collection == null)
        {
            return new NotFoundError("Collection");
        }

        collection.CardsCount.Increment();

        studyRepository.Collections.Update(collection);
        return Result.Ok();
    }
}