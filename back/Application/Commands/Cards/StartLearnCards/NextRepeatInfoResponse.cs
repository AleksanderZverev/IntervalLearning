using Domain.Card.ValueObjects;
using Domain.Schedule.Entities.Phase;

namespace Application.Commands.Cards.StartLearnCards;

public record NextRepeatInfoResponse
{
    public DateTime? NextRepeatDate { get; init; }
    public int NextPhaseIndex { get; init; }
    public Phase? NextPhase { get; init; }
    public required List<CardMovementInfo> CardMovementInfos { get; init; }
}

public record CardMovementInfo(List<CardId> CardIds, DateTime NextRepetitionDate);