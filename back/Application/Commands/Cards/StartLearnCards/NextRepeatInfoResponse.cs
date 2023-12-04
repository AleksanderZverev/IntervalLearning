using Domain.Schedule.Entities.Phase;

namespace Application.Commands.Cards.StartLearnCards;

public record NextRepeatInfoResponse
{
    public DateTime? NextRepeatDate { get; init; }
    public int NextPhaseIndex { get; init; }
    public Phase? NextPhase { get; init; }
}