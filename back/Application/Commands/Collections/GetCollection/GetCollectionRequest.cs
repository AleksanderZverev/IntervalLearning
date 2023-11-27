using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Collections.GetCollection;

public record GetCollectionRequest(
    UserId UserId,
    CollectionId CollectionId
);