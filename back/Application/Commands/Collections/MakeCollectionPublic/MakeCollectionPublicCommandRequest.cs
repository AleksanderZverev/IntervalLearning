using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Collections.MakeCollectionPublic;

public record MakeCollectionPublicCommandRequest(
    UserId UserId,
    CollectionId CollectionId
);