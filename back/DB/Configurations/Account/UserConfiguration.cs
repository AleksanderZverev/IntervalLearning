using DB.Models;
using Domain.Collection;
using Domain.User;
using Domain.User.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Account;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public static string GetIdSequence() => "user_id_sequence";

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Id)
            .IsRequired()
            .UseSequence(GetIdSequence())
            .HasConversion(Converters.UserId);

        builder.OwnsOne(u => u.UserName, b =>
        {
            b.Property(u => u.FirstName)
                .HasConversion(n => n.Value, firstName => PartedName.Create(firstName).Value)
                .HasMaxLength(50)
                .IsRequired()
                .HasColumnName("FirstName");

            b.Property(u => u.LastName)
                .HasConversion(n => n.Value, lastName => PartedName.Create(lastName).Value)
                .HasMaxLength(50)
                .IsRequired()
                .HasColumnName("LastName");
        });

        builder.Property(u => u.Email)
            .HasConversion(e => e.Value, email => EmailAddress.Create(email).Value)
            .HasMaxLength(255)
            .IsRequired();

        // builder.Property(u => u.Collections)
        //     .HasField("collections")
        //     .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany<Collection>()
            .WithOne(c => c.ParentUser)
            .HasForeignKey(c => c.ParentUserId);

        builder.HasOne(c => c.Metadata)
            .WithOne(m => m.ParentUser)
            .HasForeignKey<UserMetadataEntity>(m => m.ParentUserId);
    }
}