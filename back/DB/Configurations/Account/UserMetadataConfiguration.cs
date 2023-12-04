using Domain.User.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Account;

public class UserMetadataConfiguration : IEntityTypeConfiguration<UserMetadata>
{
    public void Configure(EntityTypeBuilder<UserMetadata> builder)
    {
        builder.HasKey(c => c.ParentUserId);

        builder.Ignore(m => m.Id);
        builder.Ignore(m => m.DomainEvents);

        builder.HasOne(m => m.ParentUser)
            .WithOne(u => u.Metadata)
            .HasForeignKey<UserMetadata>(m => m.ParentUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(m => m.SuggestTranslationLanguage)
            .WithMany()
            .HasForeignKey(m => m.SuggestTranslationLanguageId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}