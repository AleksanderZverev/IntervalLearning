using System.Data;
using System.Numerics;
using DB.Infrastructure.DomainEventResolver;
using Domain;
using Domain.Card;
using Domain.Collection;
using Domain.Deprecated.DbModels;
using Domain.Dictionary.Translation;
using Domain.Dictionary.Word;
using Domain.Language;
using Domain.Queue;
using Domain.Schedule;
using Domain.Schedule.Entities.Phase;
using Domain.Schedule.Entities.Phase.Entities;
using Domain.Schedule.Entities.Remember;
using Domain.Theme;
using Domain.User;
using Domain.User.Entities;
using Microsoft.EntityFrameworkCore;
using UserMetadata = Domain.User.Entities.UserMetadata;

namespace DB
{
    public class ApplicationContext : DbContext
    {
        private readonly DomainEventDispatcher domainEventDispatcher;
        
        public ApplicationContext(
            DomainEventDispatcher domainEventDispatcher,
            DbContextOptions<ApplicationContext> options) : base(options)
        {
            this.domainEventDispatcher = domainEventDispatcher;
        }
        
        protected ApplicationContext(
            DomainEventDispatcher domainEventDispatcher, 
            DbContextOptions options) : base(options)
        {
            this.domainEventDispatcher = domainEventDispatcher;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserPassword> UsersPasswords { get; set; }
        public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }

        public DbSet<Collection> Collections { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<Remember> Remembers { get; set; }
        public DbSet<PhaseRememberEntity> PhaseRememberEntities { get; set; }
        public DbSet<Theme> Themes { get; set; }
        public DbSet<RepeatsSchedule> RepeatsSchedules { get; set; }
        public DbSet<Phase> Phases { get; set; }

        public DbSet<CardRepeatQueue> Queue { get; set; }

        public DbSet<UserMetadata> UserMetadata { get; set; }

        //Publications

        public DbSet<CollectionPublicationEntity> CollectionPublications { get; set; }
        public DbSet<PublicCollectionSubscriber> PublicCollectionSubscribers { get; set; }

        //Dictionary

        public DbSet<LanguageWord> Words { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<WordTranslation> Translations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        }

        public void EnsureSequenceCreated(string sequenceName)
        {
            var connection = Database.GetDbConnection();
            
            var isConnectionOpened = connection.State is (ConnectionState.Open or ConnectionState.Fetching);
            if (!isConnectionOpened)
            {
                connection.Open();
            }

            using var createSequenceCommand = connection.CreateCommand();
            createSequenceCommand.CommandText = $"create sequence if not exists {sequenceName}";
            createSequenceCommand.ExecuteNonQuery();

            if (!isConnectionOpened)
            {
                connection.Close();
            }
        }

        public short GetSequenceNextValue16(string sequenceName)
            => (short)GetSequenceNextValue64(sequenceName); 
        
        public int GetSequenceNextValue32(string sequenceName)
            => (int)GetSequenceNextValue64(sequenceName); 

        public long GetSequenceNextValue64(string sequenceName)
        {
            //do not dispose connection
            var connection = Database.GetDbConnection();

            var isConnectionOpened = connection.State is ConnectionState.Open or ConnectionState.Fetching;
            if (!isConnectionOpened)
            {
                connection.Open();
            }

            using var command = connection.CreateCommand();
            command.CommandText = $"select nextval('{sequenceName}')";
            
            using var reader = command.ExecuteReader();
            reader.Read();
            var nextValue = reader.GetInt64(0);
            
            if (!isConnectionOpened)
            {
                connection.Close();
            }
            return nextValue;
        }

        public override int SaveChanges()
        {
            HandleEvents().GetAwaiter().GetResult();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            await HandleEvents();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private async Task HandleEvents()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is not IEntity entity || entity.DomainEvents.Count == 0)
                    continue;

                var domainEvents = entity.DomainEvents.ToList();
                entity.ClearDomainEvents();

                foreach (var domainEvent in domainEvents)
                {
                    //TODO 1: Make repositories with unit of work (Save changes calls multiple times while dispatching handlers)
                    //TODO 2: Try to get rid of parameterless constructor
                    var result = await domainEventDispatcher.Dispatch(domainEvent);
                     if (result.IsFailed)
                         throw new NotSupportedException();
                }
            }
        }
    }
}