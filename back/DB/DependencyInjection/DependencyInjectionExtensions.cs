using DB.Infrastructure.DomainEventResolver;
using DB.Quaries.Accounts;
using DB.Quaries.Accounts.RefreshTokens;
using DB.Quaries.Accounts.Users;
using DB.Quaries.Dictionary;
using DB.Quaries.Store;
using DB.Quaries.Study;
using DB.Quaries.Study.RelearningCard;
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
using Domain.RelearningCard;
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
using DomainServices.DB.Queries.Accounts;
using DomainServices.DB.Queries.Accounts.RefreshTokens;
using DomainServices.DB.Queries.Accounts.Users;
using DomainServices.DB.Queries.Dictionary;
using DomainServices.DB.Queries.Dictionary.Languages;
using DomainServices.DB.Queries.Dictionary.Words;
using DomainServices.DB.Queries.Store;
using DomainServices.DB.Queries.Store.CollectionPublications;
using DomainServices.DB.Queries.Store.PublicCollection;
using DomainServices.DB.Queries.Store.PublicCollectionSubscribers;
using DomainServices.DB.Queries.Study;
using DomainServices.DB.Queries.Study.Cards;
using DomainServices.DB.Queries.Study.Collections;
using DomainServices.DB.Queries.Study.Queue;
using DomainServices.DB.Queries.Study.RelearningCards;
using DomainServices.DB.Queries.Study.Remember;
using DomainServices.DB.Queries.Study.Schedule;
using DomainServices.DB.Queries.Study.Themes;
using DomainServices.DB.Repositories;
using DomainServices.DB.Repositories.Accounts;
using DomainServices.DB.Repositories.Accounts.Users;
using DomainServices.DB.Repositories.Store;
using DomainServices.DB.Repositories.Store.PublicCollections;
using DomainServices.DB.Repositories.Study;
using DomainServices.DB.Repositories.Study.CardRemembers;
using DomainServices.DB.Repositories.Study.Cards;
using DomainServices.DB.Repositories.Study.Collections;
using DomainServices.DB.Repositories.Study.Queue;
using DomainServices.DB.Repositories.Study.Schedules;
using DomainServices.DB.Repositories.Study.Themes;
using DomainServices.DB.Transactions;
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
        
        //RelearningCard
        services.AddScoped<IRelearningCardsResolver, RelearningCardsResolver>();
        services.AddScoped<IRepository<RelearningCard>, BaseRepository<RelearningCard>>();
        
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