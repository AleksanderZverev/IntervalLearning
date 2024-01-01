using System.Data;
using System.Data.Common;
using DB.Infrastructure.DomainEventResolver;
using Domain;
using Domain.Card;
using Domain.Collection;
using Domain.Deprecated.DbModels;
using Domain.Dictionary.Translation;
using Domain.Dictionary.Word;
using Domain.Language;
using Domain.Queue;
using Domain.RelearningCard;
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
        public DbSet<RelearningCard> RelearningCards { get; set; }

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

        public void EnsureSequenceCreated(string sequenceName, int ensureStartValue = 1)
        {
            var connection = Database.GetDbConnection();
            
            var isConnectionOpened = connection.State is (ConnectionState.Open or ConnectionState.Fetching);
            if (!isConnectionOpened)
            {
                connection.Open();
            }

            void OnReturn()
            {
                if (!isConnectionOpened)
                {
                    connection.Close();
                }
            }

            using var checkSequenceExistsCommand = connection.CreateCommand();
            checkSequenceExistsCommand.CommandText =
                $"SELECT relname FROM pg_class WHERE relkind = 'S' AND relname = '{sequenceName}'";
            var reader = checkSequenceExistsCommand.ExecuteReader();
            var canRead = reader.Read();
            var foundSequence = canRead ? reader.GetString(0) : string.Empty;
            reader.Close();

            if (!string.IsNullOrEmpty(foundSequence) && foundSequence == sequenceName)
            {
                OnReturn();
                return;
            }
            
            using var createSequenceCommand = connection.CreateCommand();
            createSequenceCommand.CommandText = $"create sequence {sequenceName}";
            createSequenceCommand.ExecuteNonQuery();

            if (ensureStartValue < 2)
            {
                OnReturn();
                return;
            }
            
            SetSequenceValue(sequenceName, ensureStartValue, connection);
            OnReturn();
        }

        private static void SetSequenceValue(string sequenceName, int ensureStartValue, DbConnection connection)
        {
            using var setValueCommand = connection.CreateCommand();
            setValueCommand.CommandText = $"select setval('{sequenceName}', {ensureStartValue})";
            setValueCommand.ExecuteNonQuery();
        }

        public short GetSequenceNextValue16(string sequenceName, int ensureStartValue = 1)
            => (short)GetSequenceNextValue64(sequenceName, ensureStartValue); 
        
        public int GetSequenceNextValue32(string sequenceName, int ensureStartValue = 1)
            => (int)GetSequenceNextValue64(sequenceName, ensureStartValue); 

        public long GetSequenceNextValue64(string sequenceName, int ensureStartValue = 1)
        {
            //do not dispose connection
            var connection = Database.GetDbConnection();

            var isConnectionOpened = connection.State is ConnectionState.Open or ConnectionState.Fetching;
            if (!isConnectionOpened)
            {
                connection.Open();
            }

            var nextValue = GetSequenceValue(sequenceName, connection);

            if (nextValue < ensureStartValue)
            {
                SetSequenceValue(sequenceName, ensureStartValue, connection);
                nextValue = GetSequenceValue(sequenceName, connection);
            }
            
            if (!isConnectionOpened)
            {
                connection.Close();
            }
            return nextValue;
        }

        private static long GetSequenceValue(string sequenceName, DbConnection connection)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"select nextval('{sequenceName}')";

            using var reader = command.ExecuteReader();
            reader.Read();
            var nextValue = reader.GetInt64(0);
            reader.Close();
            
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