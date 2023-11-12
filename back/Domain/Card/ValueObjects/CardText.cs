using Domain.Common.ValueObjects;
using FluentResults;
using Infrastructure;

namespace Domain.Card.ValueObjects;

public class CardText : SingleValueObject<string>
{
    private CardText(string value) : base(value)
    {
    }

    public static Result<CardText> Create(string text)
    {
        text = TextMaster.RemoveWhiteSpaces(text);
        
        if (string.IsNullOrWhiteSpace(text))
            return Result.Fail("Name is empty");

        if (text.Length > 255)
            return Result.Fail("Name is too long");

        return new CardText(text);
    }
}