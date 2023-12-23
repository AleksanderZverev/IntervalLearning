using Domain.Collection;
using Domain.Deprecated.DbModels;

namespace Application.Commands.Collections.SearchPublicCollection;

public record SearchPublicCollectionItem(
    Collection Collection,
    PublicCollectionSubscriber? Subscriber
);