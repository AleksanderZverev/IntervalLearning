using DomainServices.Study.CardRepeatQueue;
using DomainServices.Study.Remember;
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