using Microsoft.Extensions.DependencyInjection;

namespace Application.DI;

public static class ServiceCollectionExtensions
{
    public static void AddApplication(this IServiceCollection services)
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
}