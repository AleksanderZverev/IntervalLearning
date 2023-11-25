using DB.Models.ValueObjects;
using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Cards.StartLearnCards;

public record StartLearnCardsRequest(
    UserId UserId,
    CollectionId CollectionId,
    UserId ScheduleUserId,
    ScheduleId ScheduleId, 
    List<CardId> CardIds
);