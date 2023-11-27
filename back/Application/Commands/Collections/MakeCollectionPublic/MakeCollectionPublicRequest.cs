using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Collections.MakeCollectionPublic;

public record MakeCollectionPublicRequest(
    UserId UserId,
    CollectionId CollectionId
);