using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Domain.Collection.Events;

public record CollectionDeletedEvent(
    UserId UserId,
    CollectionId CollectionId) : IDomainEvent;