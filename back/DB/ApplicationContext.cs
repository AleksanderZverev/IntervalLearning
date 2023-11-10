using DB.Models;
using DB.Models.Dictionary;
using DB.Models.Store;
using Domain.Collection;
using Domain.Language;
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
        public DbSet<CardEntity> Cards { get; set; }
        public DbSet<RememberEntity> Remembers { get; set; }
        public DbSet<PhaseRememberEntity> PhaseRememberEntities { get; set; }
        public DbSet<ThemeEntity> Themes { get; set; }
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

        public long GetSequenceNextValue(string sequenceName)
        {
            //do not dispose connection
            var connection = Database.GetDbConnection();
            
            using var command = connection.CreateCommand();
            command.CommandText = $"select nextval('{sequenceName}')";
            
            connection.Open();
            
            using var reader = command.ExecuteReader();
            reader.Read();
            var nextValue = reader.GetInt64(0);
            
            connection.Close();
            return nextValue;
        }
    }
}