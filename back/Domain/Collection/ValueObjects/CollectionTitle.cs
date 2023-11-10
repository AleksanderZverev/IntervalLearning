using FluentResults;

namespace Domain.Collection.ValueObjects;

public class CollectionTitle
{
    public string Value { get; }

    private CollectionTitle(string value)
    {
        Value = value;
    }

    public static Result<CollectionTitle> Create(string collectionName)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
            return Result.Fail("Name is empty");

        collectionName = collectionName.Trim();

        if (collectionName.Length > 100)
            return Result.Fail("Name is too long");

        return new CollectionTitle(collectionName);
    }
}