using DB.Models;
using DB.Models.Dictionary;
using DB.Models.Store;
using Microsoft.EntityFrameworkCore;

namespace DB
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions options) : base(options) { }

        public DbSet<UserEntity> Users { get; set; }
        public DbSet<UserPasswordsEntity> UsersPasswords { get; set; }
        public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }

        public DbSet<CollectionEntity> Collections { get; set; }
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
        public DbSet<LanguageEntity> Languages { get; set; }
        public DbSet<TranslationEntity> Translations { get; set; }
        
        //Courses

        public DbSet<CourseEntity> Courses { get; set; }
        public DbSet<UsersGroupEntity> UsersGroups { get; set; }
        public DbSet<TopicEntity> Topics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            BuildDictionaryModels(modelBuilder);
            BuildStoreModels(modelBuilder);

            // UserEntity

            modelBuilder.Entity<UserEntity>()
                .HasMany<CollectionEntity>()
                .WithOne(c => c.ParentUser)
                .HasForeignKey(c => c.ParentUserId);

            // UserPasswordsEntity

            modelBuilder.Entity<UserPasswordsEntity>()
                .HasKey(p => p.ParentUserId);

            modelBuilder.Entity<UserPasswordsEntity>()
                .HasOne(p => p.ParentUser)
                .WithOne(u => u.PasswordHash)
                .HasForeignKey<UserPasswordsEntity>(p => p.ParentUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // UserMetadataEntity

            modelBuilder.Entity<UserMetadataEntity>()
                .HasKey(c => c.ParentUserId);

            modelBuilder.Entity<UserMetadataEntity>()
                .HasOne(m => m.ParentUser)
                .WithOne()
                .HasForeignKey<UserMetadataEntity>(m => m.ParentUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserMetadataEntity>()
                .HasOne(m => m.SuggestTranslationLanguage)
                .WithMany()
                .HasForeignKey(m => m.SuggestTranslationLanguageId)
                .OnDelete(DeleteBehavior.NoAction);

            // RefreshTokenEntity

            modelBuilder.Entity<RefreshTokenEntity>()
                .HasKey(t => new {t.ParentUserId, t.Id});

            modelBuilder.Entity<RefreshTokenEntity>()
                .HasOne(t => t.ParentUser)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(t => t.ParentUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // CollectionEntity

            modelBuilder.Entity<CollectionEntity>()
                .HasKey(c => new {c.ParentUserId, c.Id});

            modelBuilder.Entity<CollectionEntity>()
                .HasOne(c => c.ParentUser)
                .WithMany(u => u.Collections)
                .HasForeignKey(c => c.ParentUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CollectionEntity>()
                .HasOne(c => c.Theme)
                .WithMany()
                .HasForeignKey(c => c.ThemeId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CollectionEntity>()
                .HasOne(c => c.CollectionPublicationEntity)
                .WithOne(p => p.ParentCollection)
                .HasForeignKey<CollectionPublicationEntity>(c => new {c.ParentUserId, c.ParentCollectionId})
                .OnDelete(DeleteBehavior.Cascade);

            // CardEntity

            modelBuilder.Entity<CardEntity>()
                .HasKey(c => new {c.ParentUserId, c.ParentCollectionId, c.Id});
            
            modelBuilder.Entity<CardEntity>()
                .HasOne(c => c.ParentUser)
                .WithMany()
                .HasForeignKey(c => c.ParentUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CardEntity>()
                .HasOne(c => c.ParentCollection)
                .WithMany(c => c.Cards)
                .HasForeignKey(c => new {c.ParentUserId, c.ParentCollectionId})
                .OnDelete(DeleteBehavior.NoAction);

            // RememberEntity

            modelBuilder.Entity<RememberEntity>()
                .HasKey(r => new
                {
                    r.ParentUserId,
                    r.ParentCollectionId,
                    r.ParentCardId,
                    r.ParentRepeatsScheduleUserId,
                    r.ParentRepeatsScheduleId,
                    r.PhaseIndex,
                    r.Id
                });

            ConfigureUserReference<RememberEntity>(modelBuilder);

            modelBuilder.Entity<RememberEntity>()
                .HasOne(r => r.ParentCollection)
                .WithMany()
                .HasForeignKey(e => new { e.ParentUserId, e.ParentCollectionId })
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<RememberEntity>()
                .HasOne(r => r.ParentCard)
                .WithMany(r => r.Remembers)
                .HasForeignKey(c => new { c.ParentUserId, c.ParentCollectionId, c.ParentCardId })
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<RememberEntity>()
                .HasOne(c => c.ParentRepeatsSchedule)
                .WithMany()
                .HasForeignKey(c => new {c.ParentRepeatsScheduleUserId, c.ParentRepeatsScheduleId})
                .OnDelete(DeleteBehavior.NoAction);

            // RepeatsScheduleEntity

            modelBuilder.Entity<RepeatsScheduleEntity>()
                .HasKey(s => new {s.ParentUserId, s.Id});

            ConfigureUserReference<RepeatsScheduleEntity>(modelBuilder);

            // PhaseEntity

            modelBuilder.Entity<PhaseEntity>()
                .HasKey(s => new { s.ParentUserId, s.ParentRepeatsScheduleId, s.Id });

            ConfigureUserReference<PhaseEntity>(modelBuilder);

            modelBuilder.Entity<PhaseEntity>()
                .HasOne(p => p.ParentRepeatsSchedule)
                .WithMany(s => s.Phases)
                .HasForeignKey(p => new {p.ParentUserId, p.ParentRepeatsScheduleId})
                .OnDelete(DeleteBehavior.NoAction);

            // CardRepeatQueueEntity

            modelBuilder.Entity<CardRepeatQueueEntity>()
                .HasKey(q => new
                {
                    q.ParentUserId,
                    q.ParentCollectionId,
                    q.ParentCardId,
                    q.ParentRepeatsScheduleUserId,
                    q.ParentRepeatsScheduleId,
                    q.Id
                });

            ConfigureUserReference<CardRepeatQueueEntity>(modelBuilder);

            modelBuilder.Entity<CardRepeatQueueEntity>()
                .HasOne(q => q.ParentCollection)
                .WithMany()
                .HasForeignKey(q => new { q.ParentUserId, q.ParentCollectionId})
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CardRepeatQueueEntity>()
                .HasOne(q => q.ParentCard)
                .WithMany()
                .HasForeignKey(q => new { q.ParentUserId, q.ParentCollectionId, q.ParentCardId })
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CardRepeatQueueEntity>()
                .HasOne(q => q.ParentRepeatsSchedule)
                .WithMany()
                .HasForeignKey(q => new {q.ParentRepeatsScheduleUserId, q.ParentRepeatsScheduleId})
                .OnDelete(DeleteBehavior.NoAction);

            // PhaseRememberEntity 

            modelBuilder.Entity<PhaseRememberEntity>()
                .HasKey(r => new {r.ParentUserId, r.ParentRepeatsScheduleId, r.ParentPhaseId, r.RepeatedUserId, r.Id});

            ConfigureUserReference<PhaseRememberEntity>(modelBuilder);

            modelBuilder.Entity<PhaseRememberEntity>()
                .HasOne(p => p.RepeatedUser)
                .WithMany()
                .HasForeignKey(p => p.RepeatedUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PhaseRememberEntity>()
                .HasOne(r => r.ParentRepeatsSchedule)
                .WithMany()
                .HasForeignKey(r => new {r.ParentUserId, r.ParentRepeatsScheduleId})
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PhaseRememberEntity>()
                .HasOne(r => r.ParentPhase)
                .WithMany()
                .HasForeignKey(r => new { r.ParentUserId, r.ParentRepeatsScheduleId, r.ParentPhaseId })
                .OnDelete(DeleteBehavior.NoAction);

            // ThemeEntity

            modelBuilder.Entity<ThemeEntity>()
                .HasOne(t => t.Language)
                .WithMany()
                .HasForeignKey(t => t.LanguageId)
                .OnDelete(DeleteBehavior.NoAction);
        }

        private void BuildStoreModels(ModelBuilder modelBuilder)
        {
            // CollectionPublicationEntity

            modelBuilder.Entity<CollectionPublicationEntity>()
                .HasKey(c => new {c.ParentUserId, c.ParentCollectionId});

            modelBuilder.Entity<CollectionPublicationEntity>()
                .HasOne(c => c.ParentUser)
                .WithMany()
                .HasForeignKey(c => c.ParentUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CollectionPublicationEntity>()
                .HasMany(c => c.Subscribers)
                .WithOne(s => s.CollectionPublication)
                .HasForeignKey(s => new {s.ParentUserId, s.ParentCollectionId})
                .OnDelete(DeleteBehavior.Cascade);

        // PublicCollectionSubscriber

        modelBuilder.Entity<PublicCollectionSubscriber>()
                .HasKey(s => new {s.ParentUserId, s.ParentCollectionId, s.SubscriberUserId});

            modelBuilder.Entity<PublicCollectionSubscriber>()
                .HasOne(s => s.ParentUser)
                .WithMany()
                .HasForeignKey(s => s.ParentUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PublicCollectionSubscriber>()
                .HasOne(c => c.ParentCollection)
                .WithMany()
                .HasForeignKey(c => new { c.ParentUserId, c.ParentCollectionId })
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PublicCollectionSubscriber>()
                .HasOne(s => s.SubscriberUser)
                .WithMany()
                .HasForeignKey(s => s.SubscriberUserId)
                .OnDelete(DeleteBehavior.NoAction);
        }

        private static void BuildDictionaryModels(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LanguageEntity>()
                .HasData(
                    new LanguageEntity() {Id = 1, Name = "English", NativeLanguageName = "English"}, 
                    new LanguageEntity() {Id = 2, Name = "Russian", NativeLanguageName = "Русский"},
                    new LanguageEntity() {Id = 3, Name = "Japanese", NativeLanguageName = "日本語"});

            // WordEntity

            modelBuilder.Entity<WordEntity>()
                .HasOne(w => w.Language)
                .WithMany()
                .HasForeignKey(w => w.LanguageId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<WordEntity>()
                .HasIndex(w => w.Word);

            // TranslationEntity

            modelBuilder.Entity<TranslationEntity>()
                .HasKey(t => new {t.WordId, t.LanguageId, t.Id});

            modelBuilder.Entity<TranslationEntity>()
                .HasOne(t => t.Word)
                .WithMany()
                .HasForeignKey(t => t.WordId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TranslationEntity>()
                .HasOne(t => t.Language)
                .WithMany()
                .HasForeignKey(t => t.LanguageId)
                .OnDelete(DeleteBehavior.NoAction);
        }

        private void ConfigureUserReference<TEntity>(
            ModelBuilder modelBuilder,
            DeleteBehavior deleteBehavior = DeleteBehavior.NoAction)
            where TEntity : class, IParentUserReference
        {
            modelBuilder.Entity<TEntity>()
                .HasOne(r => r.ParentUser)
                .WithMany()
                .HasForeignKey(e => e.ParentUserId)
                .OnDelete(deleteBehavior);
        }
    }
}