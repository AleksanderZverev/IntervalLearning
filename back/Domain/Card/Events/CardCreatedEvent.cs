using Domain.Collection.ValueObjects;

namespace Domain.Card.Events;

public class CardCreatedEvent : IDomainEvent
{
    public readonly Card card;

    public CardCreatedEvent(Card card)
    {
        this.card = card;
    }
}