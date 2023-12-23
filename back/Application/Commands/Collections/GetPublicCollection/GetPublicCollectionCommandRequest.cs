using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Collections.GetPublicCollection;

public record GetPublicCollectionCommandRequest(
    UserId UserId,
    CollectionId CollectionId
);