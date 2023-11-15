using System.Diagnostics;
using Domain.Card.ValueObjects;
using Domain.Common.ValueObjects;
using FluentResults;

namespace DB.Models.ValueObjects;

public class ThemeId : SingleValueObject<short>
{
    private ThemeId(short value) : base(value)
    {
    }
    
    public static ThemeId CreateEmpty()
    {
        return new ThemeId(0);
    }

    public static Result<ThemeId> Create(short id)
    {
        if (id == default)
        {
            Debug.Fail("Passed default id");
            return Result.Fail("Incorrect theme id");
        }

        return new ThemeId(id);
    }
}