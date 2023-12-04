using Application.Common.Interfaces.DB.Queries.Accounts;
using Application.Common.Interfaces.DB.Queries.Accounts.RefreshTokens;
using Application.Common.Interfaces.DB.Queries.Accounts.Users;
using Application.Common.Interfaces.DB.Queries.Dictionary;
using Application.Common.Interfaces.DB.Queries.Dictionary.Languages;
using Application.Common.Interfaces.DB.Queries.Dictionary.Words;
using Application.Common.Interfaces.DB.Queries.Store;
using Application.Common.Interfaces.DB.Queries.Store.CollectionPublications;
using Application.Common.Interfaces.DB.Queries.Store.PublicCollection;
using Application.Common.Interfaces.DB.Queries.Store.PublicCollectionSubscribers;
using Application.Common.Interfaces.DB.Queries.Study;
using Application.Common.Interfaces.DB.Queries.Study.Cards;
using Application.Common.Interfaces.DB.Queries.Study.Collections;
using Application.Common.Interfaces.DB.Queries.Study.Queue;
using Application.Common.Interfaces.DB.Queries.Study.Remember;
using Application.Common.Interfaces.DB.Queries.Study.Schedule;
using Application.Common.Interfaces.DB.Queries.Study.Themes;
using Application.Common.Interfaces.DB.Repositories;
using Application.Common.Interfaces.DB.Repositories.Accounts;
using Application.Common.Interfaces.DB.Repositories.Accounts.Users;
using Application.Common.Interfaces.DB.Repositories.Store;
using Application.Common.Interfaces.DB.Repositories.Store.PublicCollections;
using Application.Common.Interfaces.DB.Repositories.Study;
using Application.Common.Interfaces.DB.Repositories.Study.CardRemembers;
using Application.Common.Interfaces.DB.Repositories.Study.Cards;
using Application.Common.Interfaces.DB.Repositories.Study.Collections;
using Application.Common.Interfaces.DB.Repositories.Study.Queue;
using Application.Common.Interfaces.DB.Repositories.Study.Schedules;
using Application.Common.Interfaces.DB.Repositories.Study.Themes;
using Application.Common.Interfaces.DB.Transactions;
using DB.Infrastructure.DomainEventResolver;
using DB.Quaries.Accounts;
using DB.Quaries.Accounts.RefreshTokens;
using DB.Quaries.Accounts.Users;
using DB.Quaries.Dictionary;
using DB.Quaries.Store;
using DB.Quaries.Study;
using DB.Repository;
using DB.Repository.Accounts;
using DB.Repository.Accounts.Users;
using DB.Repository.Store;
using DB.Repository.Store.PublicCollections;
using DB.Repository.Study;
using DB.Repository.Study.Collections;
using DB.Repository.Study.Schedules;
using DB.Repository.Study.Themes;
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
using Domain.Deprecated.DbModels;
using Domain.Queue;
using Domain.Queue.ValueObjects;
using Domain.Schedule;
using Domain.Schedule.Entities.Phase;
using Domain.Schedule.Entities.Phase.Entities;
using Domain.Schedule.Entities.Remember;
using Domain.Schedule.Entities.Remember.ValueObjects;
using Domain.Schedule.ValueObjects;
using Domain.Theme;
using Domain.Theme.ValueObjects;
using Domain.User;
using Domain.User.Entities;
using Domain.User.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DB.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsBuilder)
    {
        services.AddDbContext<ApplicationContext>(optionsBuilder);
        services.AddScoped<DomainEventDispatcher>();

        //===BoundedContextRepository===
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IAccountQueryRepository, AccountQueryRepository>();
        
        services.AddScoped<IStudyRepository, StudyRepository>();
        services.AddScoped<IStudyQueryRepository, StudyQueryRepository>();
        
        services.AddScoped<IStoreRepository, StoreRepository>();
        services.AddScoped<IStoreQueryRepository, StoreQueryRepository>();

        services.AddScoped<IDictionaryQueryRepository, DictionaryQueryRepository>();

        //Users
        services.AddScoped<IUsersQueryRepository, UsersQueryRepository>();
        services.AddScoped<IRepository<User, UserId, UserIdParams>, UsersRepository>();
        services.AddScoped<IRepository<UserPassword>, BaseRepository<UserPassword>>();
        services.AddScoped<IRepository<UserMetadata>, BaseRepository<UserMetadata>>();
        services.AddScoped<IRefreshTokensQueryRepository, RefreshTokensQueryRepository>();
        services.AddScoped<IRepository<RefreshTokenEntity>, BaseRepository<RefreshTokenEntity>>();

        //Theme
        services.AddScoped<IThemesQueryResolver, ThemesQueryResolver>();
        services.AddScoped<IRepository<Theme, ThemeId, ThemeIdParams>, ThemesRepository>();
        
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
        services.AddScoped<IRepository<RepeatsSchedule, ScheduleId, ScheduleIdParams>, ScheduleRepository>();
        
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