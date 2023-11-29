using DB.Models.ValueObjects;
using Domain.Card.ValueObjects;

namespace Application.Commands.Cards.RememberCard;

public class RememberItem
{
    public required CardId CardId { get; init; }
    public required RememberWeight  Weight { get; init; }
}