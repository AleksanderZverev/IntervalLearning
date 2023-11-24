using Application.Common.Interfaces.Domain.Cards;
using DB.Resolvers.Cards;
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

        return services;
    }
}