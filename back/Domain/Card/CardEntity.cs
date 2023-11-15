using DB.Models;
using Domain.Card.ValueObjects;
using Domain.Collection;

namespace Domain.Card;

public interface IParentCardReference : IParentCollectionReference
{
    public CardId ParentCardId { get; set; }
    public Card? ParentCard { get; set; }
}