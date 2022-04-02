using DB.Models;
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
        public DbSet<ThemeEntity> Themes { get; set; }
        public DbSet<RepeatsScheduleEntity> RepeatsSchedules { get; set; }
        public DbSet<PhaseEntity> Phases { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
                .HasOne(c => c.DefaultRepeatsSchedule)
                .WithMany()
                .HasForeignKey(c => new {c.DefaultRepeatsScheduleParentUserId, c.DefaultRepeatsScheduleId})
                .OnDelete(DeleteBehavior.NoAction);

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

            modelBuilder.Entity<CardEntity>()
                .HasOne(r => r.ParentRepeatsSchedule)
                .WithMany()
                .HasForeignKey(c => new { c.ParentRepeatsScheduleUserId, c.ParentRepeatsScheduleId })
                .OnDelete(DeleteBehavior.NoAction);

            // RememberEntity

            modelBuilder.Entity<RememberEntity>()
                .HasKey(r => new {r.ParentUserId, r.ParentCollectionId, r.ParentCardId, r.Id});

            modelBuilder.Entity<RememberEntity>()
                .HasOne(r => r.ParentCard)
                .WithMany(r => r.Remembers)
                .HasForeignKey(c => new { c.ParentUserId, c.ParentCollectionId, c.ParentCardId })
                .OnDelete(DeleteBehavior.NoAction);
            
            ConfigureUserReference<RememberEntity>(modelBuilder);

            modelBuilder.Entity<RememberEntity>()
                .HasOne(r => r.ParentCollection)
                .WithMany()
                .HasForeignKey(e => new { e.ParentUserId, e.ParentCollectionId })
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