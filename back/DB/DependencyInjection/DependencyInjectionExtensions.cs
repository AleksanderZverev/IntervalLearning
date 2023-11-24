using Application.Common.Interfaces.Domain.Cards;
using Application.Common.Interfaces.Domain.Study.Queue;
using Application.Common.Interfaces.Domain.Study.Remember;
using DB.Resolvers.Cards;
using DB.Resolvers.Study.Queue;
using DB.Resolvers.Study.Remember;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DB.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsBuilder)
    {
        services.AddDbContext<ApplicationContext>(optionsBuilder);

        //Cards
        services.AddScoped<ICardsQueryResolver, CardsQueryResolver>();
        services.AddScoped<ICardsMutationResolver, CardMutationResolver>();
        
        //Queue
        services.AddScoped<IRepeatingQueueResolver, RepeatingQueueResolver>();
        
        //Remember
        services.AddScoped<IRememberResolver, RememberResolver>();

        return services;
    }
}