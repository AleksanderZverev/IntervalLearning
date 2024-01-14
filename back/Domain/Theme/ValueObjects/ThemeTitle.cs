using Domain.Common.ValueObjects;
using FluentResults;
using GlobalTools;

namespace Domain.Theme.ValueObjects;

public class ThemeTitle : SingleValueObject<string>
{
    private ThemeTitle(string value) : base(value)
    {
    }

    public static Result<ThemeTitle> Create(string text)
    {
        text = TextMaster.RemoveWhiteSpaces(text);
        
        if (string.IsNullOrWhiteSpace(text))
            return Result.Fail("Theme title is empty");

        if (text.Length > 100)
            return Result.Fail("Theme title is too long");

        return new ThemeTitle(text.ToLowerInvariant());
    }
}