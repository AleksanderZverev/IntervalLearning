using DB.Models;
using Microsoft.EntityFrameworkCore;

namespace DB
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions options) : base(options) { }

        public DbSet<UserEntity> Users { get; set; }
        public DbSet<UserPasswordsEntity> UsersPasswords { get; set; }

        public DbSet<CollectionEntity> Collections { get; set; }
        public DbSet<CardEntity> Cards { get; set; }
        public DbSet<RememberEntity> Remembers { get; set; }
        public DbSet<ThemeEntity> Themes { get; set; }
        public DbSet<RepeatsScheduleEntity> RepeatsSchedules { get; set; }
        public DbSet<PhaseEntity> Phases { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

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
                .HasForeignKey<UserPasswordsEntity>(p => p.ParentUserId);

            // CollectionEntity

            modelBuilder.Entity<CollectionEntity>()
                .HasKey(c => new {c.ParentUserId, c.Id});

            modelBuilder.Entity<CollectionEntity>()
                .HasOne(c => c.ParentUser)
                .WithMany(u => u.Collections)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CollectionEntity>()
                .HasOne(c => c.Theme)
                .WithMany()
                .HasForeignKey(c => c.ThemeId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CollectionEntity>()
                .HasOne(c => c.DefaultRepeatsSchedule)
                .WithMany()
                .HasForeignKey(c => new {c.ParentUserId, c.DefaultRepeatsScheduleId})
                .OnDelete(DeleteBehavior.NoAction);

            // CardEntity

            modelBuilder.Entity<CardEntity>()
                .HasKey(c => new { c.ParentUserId, c.ParentCollectionId, c.Id });

            ConfigureUserReference<CardEntity>(modelBuilder);
            ConfigureCollectionReference<CardEntity>(modelBuilder);
            ConfigureRepeatsScheduleReference<CardEntity>(modelBuilder);

            // RememberEntity

            modelBuilder.Entity<RememberEntity>()
                .HasKey(r => new {r.ParentUserId, r.ParentCollectionId, r.ParentCardId, r.Id});

            ConfigureUserReference<RememberEntity>(modelBuilder);
            ConfigureCollectionReference<RememberEntity>(modelBuilder);
            ConfigureCardReference<RememberEntity>(modelBuilder);

            // RepeatsScheduleEntity

            modelBuilder.Entity<RepeatsScheduleEntity>()
                .HasKey(s => new {s.ParentUserId, s.Id});

            //modelBuilder.Entity<RepeatsScheduleEntity>()
            //    .HasMany<CollectionEntity>()
            //    .WithOne()
            //    .OnDelete(DeleteBehavior.SetNull);

            //modelBuilder.Entity<RepeatsScheduleEntity>()
            //    .HasMany<CardEntity>()
            //    .WithOne()
            //    .OnDelete(DeleteBehavior.SetNull);

            ConfigureUserReference<RepeatsScheduleEntity>(modelBuilder);
            //ConfigureCollectionReference<RepeatsScheduleEntity>(modelBuilder);
            //ConfigureCardReference<RepeatsScheduleEntity>(modelBuilder);

            // PhaseEntity

            modelBuilder.Entity<PhaseEntity>()
                .HasKey(s => new { s.ParentUserId, s.ParentRepeatsScheduleId, s.Id });

            ConfigureUserReference<PhaseEntity>(modelBuilder);
            ConfigureRepeatsScheduleReference<PhaseEntity>(modelBuilder);
            //ConfigureCollectionReference<PhaseEntity>(modelBuilder);
            //ConfigureCardReference<PhaseEntity>(modelBuilder);

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

        private void ConfigureCollectionReference<TEntity>(
            ModelBuilder modelBuilder, 
            DeleteBehavior deleteBehavior = DeleteBehavior.NoAction)
            where TEntity : class, IParentCollectionReference
        {
            modelBuilder.Entity<TEntity>()
                .HasOne(r => r.ParentCollection)
                .WithMany()
                .HasForeignKey(e => new { e.ParentUserId, e.ParentCollectionId})
                .OnDelete(deleteBehavior);
        }

        private void ConfigureCardReference<TEntity>(
            ModelBuilder modelBuilder,
            DeleteBehavior deleteBehavior = DeleteBehavior.NoAction)
            where TEntity : class, IParentCardReference
        {
            modelBuilder.Entity<TEntity>()
                .HasOne(r => r.ParentCard)
                .WithMany()
                .HasForeignKey(c => new { c.ParentUserId, c.ParentCollectionId, c.ParentCardId })
                .OnDelete(deleteBehavior);
        }

        private void ConfigureRepeatsScheduleReference<TEntity>(
            ModelBuilder modelBuilder,
            DeleteBehavior deleteBehavior = DeleteBehavior.NoAction)
            where TEntity : class, IParentRepeatsScheduleReference
        {
            modelBuilder.Entity<TEntity>()
                .HasOne(r => r.ParentRepeatsSchedule)
                .WithMany()
                .HasForeignKey(c => new { c.ParentUserId, c.ParentRepeatsScheduleId})
                .OnDelete(deleteBehavior);
        }
    }
}