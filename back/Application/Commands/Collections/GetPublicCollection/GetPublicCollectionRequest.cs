using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Collections.GetPublicCollection;

public record GetPublicCollectionRequest(
    UserId UserId,
    CollectionId CollectionId
);