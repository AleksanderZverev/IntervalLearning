using Domain.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations;

internal static class Extensions
{
    public static void ConfigureUserReference<TEntity>(
        this EntityTypeBuilder<TEntity> modelBuilder,
        DeleteBehavior deleteBehavior = DeleteBehavior.NoAction)
        where TEntity : class, IParentUserReference
    {
        modelBuilder.HasOne(r => r.ParentUser)
            .WithMany()
            .HasForeignKey(e => e.ParentUserId)
            .OnDelete(deleteBehavior);
    }
}