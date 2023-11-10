using Domain.User.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DB;

internal static class Converters
{
    public static readonly ValueConverter<UserId, long> UserId = new(
        from => from.Value,
        userId => Domain.User.ValueObjects.UserId.Create(userId).Value);
}