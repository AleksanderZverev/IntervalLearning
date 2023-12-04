using Domain.Card.ValueObjects;
using Domain.Schedule.Entities.Remember.ValueObjects;

namespace Application.Commands.Cards.RememberCard;

public class RememberItem
{
    public required CardId CardId { get; init; }
    public required RememberWeight  Weight { get; init; }
}