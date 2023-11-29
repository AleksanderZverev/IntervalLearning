using Application.Common.Interfaces.DB.Queries.Store;
using Application.Common.Interfaces.DB.Repositories;
using Application.Common.Interfaces.DB.Repositories.Cards;
using Application.Common.Interfaces.DB.Repositories.Store;
using Application.Common.Interfaces.DB.Repositories.Store.PublicCollections;
using Application.Common.Interfaces.DB.Repositories.Study;
using Application.Common.Interfaces.DB.Repositories.Study.CardRemembers;
using Application.Common.Interfaces.DB.Repositories.Study.Collections;
using Application.Common.Interfaces.DB.Repositories.Study.Queue;
using Application.Common.Interfaces.DB.Transactions;
using Application.Common.Interfaces.Domain.Cards;
using Application.Common.Interfaces.Domain.Collections;
using Application.Common.Interfaces.Domain.Dictionary.Words;
using Application.Common.Interfaces.Domain.Languages;
using Application.Common.Interfaces.Domain.Store.CollectionPublications;
using Application.Common.Interfaces.Domain.Store.PublicCollection;
using Application.Common.Interfaces.Domain.Store.PublicCollectionSubscribers;
using Application.Common.Interfaces.Domain.Study.Queue;
using Application.Common.Interfaces.Domain.Study.Remember;
using Application.Common.Interfaces.Domain.Study.Schedule;
using Application.Common.Interfaces.Domain.Themes;
using DB.Models;
using DB.Models.Store;
using DB.Models.ValueObjects;
using DB.Quaries.Store;
using DB.Quaries.Study;
using DB.Repository;
using DB.Repository.Store;
using DB.Repository.Store.PublicCollections;
using DB.Repository.Study;
using DB.Repository.Study.Collections;
using DB.Resolvers.Cards;
using DB.Resolvers.Collections;
using DB.Resolvers.Dictionary.Words;
using DB.Resolvers.Languages;
using DB.Resolvers.Store.CollectionPublications;
using DB.Resolvers.Store.PublicCollection;
using DB.Resolvers.Store.PublicCollectionSubscribers;
using DB.Resolvers.Study.Queue;
using DB.Resolvers.Study.Remember;
using DB.Resolvers.Study.Schedule;
using DB.Resolvers.Themes;
using DB.Transactions;
using Domain.Card;
using Domain.Card.ValueObjects;
using Domain.Collection;
using Domain.Collection.ValueObjects;
using Domain.Queue;
using Domain.Schedule;
using Domain.Schedule.Entities.Remember;
using Domain.Theme;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DB.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsBuilder)
    {
        services.AddDbContext<ApplicationContext>(optionsBuilder);
        
        //BoundedContextRepository
        services.AddScoped<IStudyRepository, StudyRepository>();
        services.AddScoped<IStudyQueryRepository, StudyQueryRepository>();
        services.AddScoped<IStoreRepository, StoreRepository>();
        services.AddScoped<IStoreQueryRepository, StoreQueryRepository>();
        
        //Theme
        services.AddScoped<IThemesQueryResolver, ThemesQueryResolver>();
        services.AddScoped<IRepository<Theme>, BaseRepository<Theme>>();
        
        //Languages
        services.AddScoped<ILanguagesQueryResolver, LanguagesQueryResolver>();
        
        //Transactions
        services.AddScoped<ITransactionProvider, TransactionProvider>();
        
        //Collections
        services.AddScoped<ICollectionQueryResolver, CollectionQueryResolver>();
        services.AddScoped<IRepository<Collection, CollectionId, CollectionIdParams>, CollectionRepository>();

        //Cards
        services.AddScoped<ICardsQueryResolver, CardsQueryResolver>();
        services.AddScoped<IRepository<Card, CardId, CardIdParams>, CardsRepository>();
        
        //Queue
        services.AddScoped<IRepeatingQueueResolver, RepeatingQueueResolver>();
        services.AddScoped<IRepository<CardRepeatQueue, QueueId, RepeatingQueueIdParams>, RepeatingQueueRepository>();
        
        //Remember
        services.AddScoped<IRememberQueryResolver, RememberQueryQueryResolver>();
        services.AddScoped<IRepository<Remember, RememberId, RememberIdParams>, RememberRepository>();
        
        //Schedule
        services.AddScoped<IScheduleResolver, ScheduleResolver>();
        services.AddScoped<IRepository<RepeatsSchedule>, BaseRepository<RepeatsSchedule>>();
        
        //Phase
        services.AddScoped<IRepository<Phase>, BaseRepository<Phase>>();
        
        //PhaseRemember
        services.AddScoped<IRepository<PhaseRememberEntity>, BaseRepository<PhaseRememberEntity>>();
        
        //===STORE===
        
        //PublicCOllection
        services.AddScoped<IPublicCollectionQueryResolver, PublicCollectionQueryResolver>();
        services.AddScoped<IPublicCollectionRepository, PublicCollectionRepository>();

        //PublicCollectionSubscriber
        services.AddScoped<IPublicCollectionSubscriberQueryResolver, PublicCollectionSubscriberQueryResolver>();
        services.AddScoped<IRepository<PublicCollectionSubscriber>, BaseRepository<PublicCollectionSubscriber>>();
        
        //CollectionPublicationEntity
        services.AddScoped<ICollectionPublicationQueryResolver, CollectionPublicationQueryResolver>();
        services.AddScoped<IRepository<CollectionPublicationEntity>, BaseRepository<CollectionPublicationEntity>>();

        //===DICTIONARY===
        
        //Words
        services.AddScoped<IWordsQueryResolver, WordsQueryResolver>();

        return services;
    }
}