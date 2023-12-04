using Domain;
using FluentResults;

namespace Application.DomainEventHandlers;

public interface IDomainEventHandler
{
}

public interface IDomainEventHandler<in T> : IDomainEventHandler
    where T : IDomainEvent
{
    public Task<Result> Handle(T domainEvent);
}