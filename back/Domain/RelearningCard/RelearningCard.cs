using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Domain.RelearningCard;

public class RelearningCard : Entity<ComplexCardId>
{
    public UserId UserId { get; }
    public CollectionId CollectionId { get; }
    public CardId CardId { get; }

    public RelearningCard(UserId userId, CollectionId collectionId, CardId cardId)
        : base(new ComplexCardId()
        {
            UserId = userId,
            CollectionId = collectionId,
            Id = cardId,
        })
    {
        UserId = userId;
        CollectionId = collectionId;
        CardId = cardId;
    }
}