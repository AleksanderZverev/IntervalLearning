using DB.Models;
using Domain.User.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Account;

public class UserPasswordConfiguration : IEntityTypeConfiguration<UserPassword>
{
    public void Configure(EntityTypeBuilder<UserPassword> builder)
    {
        builder.ToTable("UsersPasswords");
        
        builder.HasKey(p => p.ParentUserId);

        builder.Ignore(p => p.Id);
        builder.Ignore(p => p.DomainEvents);

        builder.Property(p => p.PasswordHash)
            .HasMaxLength(60)
            .HasColumnType("varchar(60)");

        builder.HasOne(p => p.ParentUser)
            .WithOne(u => u.PasswordHash)
            .HasForeignKey<UserPassword>(p => p.ParentUserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}