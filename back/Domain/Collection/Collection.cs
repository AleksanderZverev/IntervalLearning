using Domain.Collection.ValueObjects;
using Domain.Common.ValueObjects;
using Domain.Deprecated.DbModels;
using Domain.Theme.ValueObjects;
using Domain.User.ValueObjects;
using FluentResults;

namespace Domain.Collection;

public class Collection : Entity<ComplexCollectionId>
{
    public CollectionId Id { get; set; }
    
    public CollectionTitle Title { get; set; }
    public bool IsDefaultBackSide { get; set; }
    
    public ThemeId ThemeId { get; set; }
    public virtual Theme.Theme? Theme { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public Counter CardsCount { get; set; } = Counter.CreateEmpty();
    public Counter NotStartedCardsCount { get; set; } = Counter.CreateEmpty();
    // public Counter StartedCards { get; set; }
    // public Counter FinishedCards { get; set; }

    // private readonly List<CardEntity> cards = new();
    // public virtual IReadOnlyCollection<CardEntity> Cards => cards.AsReadOnly();


    public bool IsPublic { get; private set; }

    public virtual CollectionPublicationEntity? CollectionPublicationEntity { get; set; }

    public UserId ParentUserId { get; set; }
    public virtual User.User? ParentUser { get; set; }

    protected Collection(UserId parentUserId, CollectionId id) 
        : base(ComplexCollectionId.Create(parentUserId, id).Value)
    {
        ParentUserId = parentUserId;
        Id = id;
    }

    public static Result<Collection> Create(UserId userId, CollectionId id, CollectionTitle title, ThemeId themeId)
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
    public void MakePublic()
    {
        IsPublic = true;
    }
}