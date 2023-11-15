using Domain.Card;
using Domain.Card.ValueObjects;

namespace DB.Models;

public interface IParentCardReference : IParentCollectionReference
{
    public CardId ParentCardId { get; set; }
    public Card? ParentCard { get; set; }
}