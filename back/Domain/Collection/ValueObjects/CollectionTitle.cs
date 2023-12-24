using Domain.Common.ValueObjects;
using FluentResults;

namespace Domain.Collection.ValueObjects;

public class CollectionTitle : SingleValueObject<string>
{
    private CollectionTitle(string value) : base(value)
    {
    }

    public static Result<CollectionTitle> Create(string collectionName)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
            return Result.Fail("Collection name is empty");

        collectionName = collectionName.Trim();

        if (collectionName.Length > 100)
            return Result.Fail("Collection name is too long");

        return new CollectionTitle(collectionName);
    }
}