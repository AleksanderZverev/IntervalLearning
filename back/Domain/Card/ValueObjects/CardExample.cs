using Domain.Common.ValueObjects;
using FluentResults;
using GlobalTools;

namespace Domain.Card.ValueObjects;

public class CardExample : SingleValueObject<string>
{
    private CardExample(string value) : base(value)
    {
    }

    public static Result<CardExample> Create(string example)
    {
        example = TextMaster.RemoveWhiteSpaces(example);

        if (example.Length > 255)
            return Result.Fail("Example is too long");

        return new CardExample(example);
    }
}