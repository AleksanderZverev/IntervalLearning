using Domain.Card.ValueObjects;
using Domain.Common.ValueObjects.Text.SingleLine;
using Domain.Schedule.Entities.Remember.ValueObjects;

namespace Application.Commands.Cards.RememberCard;

public class RememberItem
{
    public required CardId CardId { get; init; }
    public required RememberWeight  Weight { get; init; }
    public MediumSingleLineString? Comment { get; init; }
}