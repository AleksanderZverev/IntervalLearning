using DB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Account;

public class UserPasswordConfiguration : IEntityTypeConfiguration<UserPasswordsEntity>
{
    public void Configure(EntityTypeBuilder<UserPasswordsEntity> builder)
    {
        builder.HasKey(p => p.ParentUserId);

        builder.HasOne(p => p.ParentUser)
            .WithOne(u => u.PasswordHash)
            .HasForeignKey<UserPasswordsEntity>(p => p.ParentUserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}