using Domain.Common.ValueObjects;
using FluentResults;
using Infrastructure;

namespace Domain.Card.ValueObjects;

public class CardDescription : SingleValueObject<string>
{
    private CardDescription(string value) : base(value)
    {
    }

    public static Result<CardDescription> Create(string description)
    {
        description = TextMaster.RemoveWhiteSpaces(description);

        if (description.Length > 500)
            return Result.Fail("Card description is too long");

        return new CardDescription(description);
    }
}