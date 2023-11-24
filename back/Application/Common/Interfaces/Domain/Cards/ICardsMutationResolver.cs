using Application.Common.Interfaces.DB;
using Domain.Card;
using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;
using FluentResults;

namespace Application.Common.Interfaces.Domain.Cards;

public interface ICardsMutationResolver : IMutationResolver<Card>
{
    public Result<CardId> GetUniqueId(UserId userId, CollectionId collectionId);
}