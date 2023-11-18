using System.Diagnostics;
using Domain.Collection.ValueObjects;
using Domain.Common.ValueObjects;
using Domain.User.ValueObjects;
using FluentResults;

namespace Domain.Card.ValueObjects;

public class ComplexCardId : ValueObject
{
    public required UserId UserId { get; init; }
    public required CollectionId CollectionId { get; init; }
    public required CardId Id { get; init; }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return UserId.GetEqualityComponents();
        yield return CollectionId.GetEqualityComponents();
        yield return Id.GetEqualityComponents();
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