using Domain.Common.ValueObjects;
using FluentResults;
using GlobalTools;

namespace Domain.Card.ValueObjects;

public class CardTag : SingleValueObject<string>
{
    private CardTag(string value) : base(value)
    {
    }

    public static Result<CardTag> Create(string text)
    {
        text = TextMaster.RemoveWhiteSpaces(text);
        
        if (string.IsNullOrWhiteSpace(text))
            return Result.Fail("CardTag cannot be null or empty");
        
        if (text.Length > 255)
            return Result.Fail("CardTag text is too long");

        return new CardTag(text);

    }
}