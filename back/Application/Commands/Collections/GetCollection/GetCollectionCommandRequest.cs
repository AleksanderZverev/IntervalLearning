using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Collections.GetCollection;

public record GetCollectionCommandRequest(
    UserId UserId,
    CollectionId CollectionId
);