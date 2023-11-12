using DB.Models;
using DB.Models.Store;
using Domain.Collection.ValueObjects;
using Domain.Common.ValueObjects;
using Domain.User.ValueObjects;
using FluentResults;

namespace Domain.Collection;

// [Table("Collections")]
public class Collection : Entity<ComplexCollectionId>
{
    // [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public CollectionId Id { get; set; }
    
    // [Required]
    // [StringLength(100)]
    public CollectionTitle Title { get; set; }

    public bool IsDefaultBackSide { get; set; }

    public short ThemeId { get; set; }
    public virtual ThemeEntity? Theme { get; set; }

    // [Required]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public Counter CardsCount { get; set; } = Counter.CreateEmpty();
    public Counter NotStartedCardsCount { get; set; } = Counter.CreateEmpty();
    // public Counter StartedCards { get; set; }
    // public Counter FinishedCards { get; set; }

    // private readonly List<CardEntity> cards = new();
    // public virtual IReadOnlyCollection<CardEntity> Cards => cards.AsReadOnly();

    // public long ParentUserId { get; set; }

    public bool IsPublic { get; set; }

    public virtual CollectionPublicationEntity? CollectionPublicationEntity { get; set; }

    public UserId ParentUserId { get; set; }
    public virtual User.User? ParentUser { get; set; }

    protected Collection(UserId parentUserId, CollectionId id) 
        : base(ComplexCollectionId.Create(parentUserId, id).Value)
    {
        ParentUserId = parentUserId;
        Id = id;
    }

    public static Result<Collection> Create(UserId userId, CollectionId id, CollectionTitle title, short themeId)
    {
        return new Collection(userId, id)
        {
            Title = title,
            ThemeId = themeId
        };
    }
    //
    // public void AddCard(Card.Card card)
    // {
    //     // cards.Add(card);
    //     CardsCount.Increment();
    //     AddDomainEvent(new CardAdded(card));
    // }
    //
    //
    // public void Remove(Card.Card card)
    // {
    //     // cards.Remove(card);
    //     CardsCount.Decrement();
    //     AddDomainEvent(new CardRemoved(card));
    // }
}