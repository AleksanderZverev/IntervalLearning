using DB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Account;

public class UserMetadataConfiguration : IEntityTypeConfiguration<UserMetadataEntity>
{
    public void Configure(EntityTypeBuilder<UserMetadataEntity> builder)
    {
        builder.HasKey(c => c.ParentUserId);

        builder.HasOne(m => m.ParentUser)
            .WithOne(u => u.Metadata)
            .HasForeignKey<UserMetadataEntity>(m => m.ParentUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(m => m.SuggestTranslationLanguage)
            .WithMany()
            .HasForeignKey(m => m.SuggestTranslationLanguageId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}