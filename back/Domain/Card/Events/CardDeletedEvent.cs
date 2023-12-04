namespace Domain.Card.Events;

public record CardDeletedEvent(Card Card) : IDomainEvent;