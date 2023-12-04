using Domain.User.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Account;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshTokenEntity>
{
    public void Configure(EntityTypeBuilder<RefreshTokenEntity> builder)
    {
        builder.HasKey(t => new {t.ParentUserId, t.Id});

        builder.HasOne(t => t.ParentUser)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(t => t.ParentUserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}