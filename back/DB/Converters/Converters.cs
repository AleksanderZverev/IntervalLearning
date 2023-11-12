using Domain.Collection.ValueObjects;
using Domain.Common.ValueObjects;
using Domain.User.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DB;

internal static class Converters
{
    public static readonly ValueConverter<UserId, long> UserId = new(
        from => from.Value,
        userId => Domain.User.ValueObjects.UserId.Create(userId).Value
    );

    public static ValueConverter<Counter, short> Counter = new(
        from => (short)from.Value,
        count => Domain.Common.ValueObjects.Counter.Create(count).Value
    );

    public static ValueConverter<CollectionId, short> CollectionId = new(
        from => from.Value,
        collectionId => Domain.Collection.ValueObjects.CollectionId.Create(collectionId).Value
    );
}