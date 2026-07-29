using Domain.Card;
using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Study;

public class CardConfiguration  : IEntityTypeConfiguration<Card>
{ 
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        builder.HasKey(c => new {c.ParentUserId, c.ParentCollectionId, c.Id});

        builder.Property(c => c.Id)
            .HasConversion(Converters.CardId)
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(c => c.RememberingText)
            .HasMaxLength(255)
            .IsRequired()
            .HasConversion(Converters.CardText);

        builder.Property(c => c.PromptText)
            .HasMaxLength(255)
            .HasConversion(Converters.CardText.ToEmptyString());

        builder.Property(c => c.MeaningText)
            .HasMaxLength(255)
            .IsRequired()
            .HasConversion(Converters.CardText);

        builder.Property(c => c.Description)
            .HasMaxLength(500)
            .HasConversion(Converters.CardDescription.ToNullable());

        builder.Property(c => c.Examples)
            .IsRequired(false)
            .HasMaxLength(255)
            .HasPostgresArrayConversion<CardExample, string>(Converters.CardExample)
            .Metadata.SetValueComparer(new ValueComparer<List<CardExample>>(
                equalsExpression: (c1, c2) => c1.SequenceEqual(c2),
                hashCodeExpression: c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                snapshotExpression: c => c.ToList()));
        
        builder.Property(c => c.Tags)
            .HasMaxLength(255)
            .IsRequired(false)
            .HasPostgresArrayConversion<CardTag, string>(Converters.CardTag)
            .Metadata.SetValueComparer(new ValueComparer<List<CardTag>>(
                equalsExpression: (c1, c2) => c1.SequenceEqual(c2),
                hashCodeExpression: c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                snapshotExpression: c => c.ToList()));

        builder.HasOne(c => c.ParentUser)
            .WithMany()
            .HasForeignKey(c => c.ParentUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.ParentCollection)
            .WithMany() //c => c.Cards
            .HasForeignKey(c => new {c.ParentUserId, c.ParentCollectionId})
            .OnDelete(DeleteBehavior.NoAction);
    }
}