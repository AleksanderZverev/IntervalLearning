using Application.Common.Interfaces.DB.Transactions;
using Application.Common.Interfaces.Domain.Cards;
using Application.Common.Interfaces.Domain.Collections;
using Application.Common.Interfaces.Domain.Dictionary.Words;
using Application.Common.Interfaces.Domain.Languages;
using Application.Common.Interfaces.Domain.Store.PublicCollection;
using Application.Common.Interfaces.Domain.Store.PublicCollectionSubscribers;
using Application.Common.Interfaces.Domain.Study.PhaseRemember;
using Application.Common.Interfaces.Domain.Study.Queue;
using Application.Common.Interfaces.Domain.Study.Remember;
using Application.Common.Interfaces.Domain.Study.Schedule;
using Application.Common.Interfaces.Domain.Themes;
using DB.Resolvers.Cards;
using DB.Resolvers.Collections;
using DB.Resolvers.Dictionary.Words;
using DB.Resolvers.Languages;
using DB.Resolvers.Store.PublicCollection;
using DB.Resolvers.Store.PublicCollectionSubscribers;
using DB.Resolvers.Study.PhaseRemember;
using DB.Resolvers.Study.Queue;
using DB.Resolvers.Study.Remember;
using DB.Resolvers.Study.Schedule;
using DB.Resolvers.Themes;
using DB.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DB.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsBuilder)
    {
        services.AddDbContext<ApplicationContext>(optionsBuilder);
        
        //Theme
        services.AddScoped<IThemesQueryResolver, ThemesQueryResolver>();
        
        //Languages
        services.AddScoped<ILanguagesQueryResolver, LanguagesQueryResolver>();
        
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
        services.AddScoped<IRepeatingQueueMutationResolver, RepeatingQueueMutationResolver>();
        
        //Remember
        services.AddScoped<IRememberQueryResolver, RememberQueryQueryResolver>();
        services.AddScoped<IRememberMutationResolver, RememberMutationResolver>();
        
        //Schedule
        services.AddScoped<IScheduleResolver, ScheduleResolver>();
        
        //PhaseRemember
        services.AddScoped<IPhaseRememberMutationResolver, PhaseRememberMutationResolver>();
        
        //===STORE===
        
        //PublicCOllection
        services.AddScoped<IPublicCollectionQueryResolver, PublicCollectionQueryResolver>();
        
        //PublicCollectionSubscriber
        services.AddScoped<IPublicCollectionSubscriberQueryResolver, PublicCollectionSubscriberQueryResolver>();
        
        
        //===DICTIONARY===
        
        //Words
        services.AddScoped<IWordsQueryResolver, WordsQueryResolver>();

        return services;
    }
}