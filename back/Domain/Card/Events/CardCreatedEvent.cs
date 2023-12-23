namespace Domain.Card.Events;

public record CardCreatedEvent(Card Card) : IDomainEvent;