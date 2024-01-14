using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Collections.DeleteCollection;

public record DeleteCollectionCommandRequest(
    UserId UserId,
    CollectionId CollectionId
);