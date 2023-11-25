using DB.Models.Store;
using Domain.Collection;

namespace Application.Commands.Collections.SearchPublicCollection;

public record SearchPublicCollectionItem(
    Collection Collection,
    PublicCollectionSubscriber? Subscriber
);