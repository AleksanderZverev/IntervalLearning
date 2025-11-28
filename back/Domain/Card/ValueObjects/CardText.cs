using Domain.Common.ValueObjects;
using FluentResults;
using GlobalTools;

namespace Domain.Card.ValueObjects;

public class CardText : SingleValueObject<string>
{
    private CardText(string value) : base(value)
    {
    }

    public static Result<CardText> Create(string text)
    {
        text = TextMaster.RemoveWhiteSpacesExceptNewLines(text);
        
        if (string.IsNullOrWhiteSpace(text))
            return Result.Fail("Card text is empty");

        if (text.Length > 255)
            return Result.Fail("Card text is too long");

        return new CardText(text);
    }
}