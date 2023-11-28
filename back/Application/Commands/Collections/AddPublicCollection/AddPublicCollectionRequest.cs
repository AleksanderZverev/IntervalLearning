using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Collections.AddPublicCollection;

public record AddPublicCollectionRequest(
    UserId PublicCollectionUserId,
    CollectionId PublicCollectionId,
    UserId MyUserId,
    CollectionId? MyCollectionId, 
    string? NewCollectionName,
    bool CheckUnique
);