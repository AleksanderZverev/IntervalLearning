using System.Diagnostics;
using Domain.Collection.ValueObjects;
using Domain.Common.ValueObjects;
using Domain.User.ValueObjects;
using FluentResults;

namespace Domain.Card.ValueObjects;

public class ComplexCardId : ValueObject
{
    private ComplexCardId(UserId userId, CollectionId collectionId, CardId cardId)
    {
        UserId = userId;
        CollectionId = collectionId;
        CardId = cardId;
    }

    public UserId UserId { get; }
    public CollectionId CollectionId { get; }
    public CardId CardId { get; }
    
    public static ComplexCardId Create(UserId userId, CollectionId collectionId, CardId cardId)
    {
        return new ComplexCardId(userId, collectionId, cardId);
    }
    
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return UserId.GetEqualityComponents();
        yield return CollectionId.GetEqualityComponents();
        yield return CardId.GetEqualityComponents();
    }
}

public class CardId : SingleValueObject<short>
{
    private CardId(short value) : base(value)
    {
    }

    public static Result<CardId> Create(short cardId)
    {
        if (cardId == default)
        {
            Debug.Fail("Default value passed");
            return Result.Fail("Card Id is not specified");
        }
        
        return new CardId(cardId);
    }
}