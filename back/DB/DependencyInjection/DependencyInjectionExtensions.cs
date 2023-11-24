using Application.Common.Interfaces.DB.Transactions;
using Application.Common.Interfaces.Domain.Cards;
using Application.Common.Interfaces.Domain.Collections;
using Application.Common.Interfaces.Domain.Study.Queue;
using Application.Common.Interfaces.Domain.Study.Remember;
using Application.Common.Interfaces.Domain.Study.Schedule;
using DB.Resolvers.Cards;
using DB.Resolvers.Collections;
using DB.Resolvers.Study.Queue;
using DB.Resolvers.Study.Remember;
using DB.Resolvers.Study.Schedule;
using DB.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DB.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsBuilder)
    {
        services.AddDbContext<ApplicationContext>(optionsBuilder);
        
        //Transactions
        services.AddScoped<ITransactionProvider, TransactionProvider>();
        
        //Collections
        services.AddScoped<ICollectionQueryResolver, CollectionQueryResolver>();
        services.AddScoped<ICollectionMutationResolver, CollectionMutationResolver>();

        //Cards
        services.AddScoped<ICardsQueryResolver, CardsQueryResolver>();
        services.AddScoped<ICardsMutationResolver, CardMutationResolver>();
        
        //Queue
        services.AddScoped<IRepeatingQueueResolver, RepeatingQueueResolver>();
        
        //Remember
        services.AddScoped<IRememberResolver, RememberResolver>();
        
        //Schedule
        services.AddScoped<IScheduleResolver, ScheduleResolver>();

        return services;
    }
}