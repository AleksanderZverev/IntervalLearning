using Application.DomainEventHandlers;
using Application.Services.Study.Remember;
using Domain.Queue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Application.DI;

public static class ServiceCollectionExtensions
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddCommands();
        services.AddServices();
        services.AddEventHandlers();
    }
    
    private static void AddCommands(this IServiceCollection services)
    {
        var commands = typeof(ICommand<>).Assembly.DefinedTypes
            .Where(t => t
                .GetInterfaces()
                .Any(i => i.IsGenericType 
                          && (i.GetGenericTypeDefinition() == typeof(ICommand<>)
                              || i.GetGenericTypeDefinition() == typeof(ICommand<,>)
                          )))
            .ToList();

        foreach (var command in commands)
        {
            services.AddScoped(command);
        }
    }

    private static void AddServices(this IServiceCollection services)
    {
        //Study
        services.AddScoped<CardRepeatQueueService>();
        services.AddScoped<RememberService>();
    }
    
    private static void AddEventHandlers(this IServiceCollection services)
    {
        var handlerInfos = typeof(IDomainEventHandler).Assembly.DefinedTypes
            .Where(t => t
                .GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)))
            .Select(t => (
                handler: t,
                interfaceType: t.GetInterfaces().Single(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>))
            ))
            .ToList();

        var interfaceType = typeof(IDomainEventHandler);
        foreach (var tuple in handlerInfos)
        {
            var descriptor = new ServiceDescriptor(interfaceType, tuple.handler, ServiceLifetime.Scoped);
            services.Add(descriptor);
        }
    }
}