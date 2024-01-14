using DomainServices.BoundedContext.Study.CardRepeatQueueService;
using DomainServices.BoundedContext.Study.RememberService;
using Microsoft.Extensions.DependencyInjection;

namespace DomainServices.DI;

public static class ServiceCollectionExtensions
{
    public static void AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<CardRepeatQueueService>();
        services.AddScoped<RememberService>();
    }
}