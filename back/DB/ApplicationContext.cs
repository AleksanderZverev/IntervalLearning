using System.Data;
using System.Numerics;
using DB.Models;
using DB.Models.Dictionary;
using DB.Models.Store;
using Domain.Card;
using Domain.Collection;
using Domain.Language;
using Domain.Theme;
using Domain.User;
using Microsoft.EntityFrameworkCore;

namespace DB
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }
        protected ApplicationContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserPasswordsEntity> UsersPasswords { get; set; }
        public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }

        public DbSet<Collection> Collections { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<RememberEntity> Remembers { get; set; }
        public DbSet<PhaseRememberEntity> PhaseRememberEntities { get; set; }
        public DbSet<Theme> Themes { get; set; }
        public DbSet<RepeatsScheduleEntity> RepeatsSchedules { get; set; }
        public DbSet<PhaseEntity> Phases { get; set; }

        public DbSet<CardRepeatQueueEntity> Queue { get; set; }

        public DbSet<UserMetadataEntity> UserMetadata { get; set; }

        //Publications

        public DbSet<CollectionPublicationEntity> CollectionPublications { get; set; }
        public DbSet<PublicCollectionSubscriber> PublicCollectionSubscribers { get; set; }

        //Dictionary

        public DbSet<WordEntity> Words { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<TranslationEntity> Translations { get; set; }

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
    }
}