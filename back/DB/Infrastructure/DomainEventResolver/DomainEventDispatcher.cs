using System.Reflection;
using Application.DomainEventHandlers;
using Domain;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;

namespace DB.Infrastructure.DomainEventResolver;

public class DomainEventDispatcher
{
    private readonly IServiceProvider provider;
    private Dictionary<Type,List<IDomainEventHandler>>? eventTypeToHandlers;

    public DomainEventDispatcher(IServiceProvider provider)
    {
        this.provider = provider;
    }

    public async Task<Result> Dispatch(IDomainEvent domainEvent)
    {
        eventTypeToHandlers ??= provider.GetServices<IDomainEventHandler>().GroupBy(h =>
                h.GetType()
                    .GetInterfaces()
                    .Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>))
                    .GenericTypeArguments
                    .Single())
            .ToDictionary(t => t.Key, h => h.ToList());
        
        if (!eventTypeToHandlers.TryGetValue(domainEvent.GetType(), out var handlers))
            return Result.Ok();

        if (handlers is not { Count: > 0 })
            return Result.Ok();
        
        foreach (var domainEventHandler in handlers)
        {
            var invokedTask = (Task<Result>)domainEventHandler
                .GetType()
                .GetMethod(nameof(IDomainEventHandler<IDomainEvent>.Handle), BindingFlags.Instance | BindingFlags.Public)
                .Invoke(domainEventHandler, new object[] {domainEvent});

            var result = await invokedTask;
            
            if (result.IsFailed)
                return result;
        }
        
        return Result.Ok();
    }
}