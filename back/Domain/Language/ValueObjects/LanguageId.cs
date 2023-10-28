using Domain.Common.ValueObjects;
using FluentResults;

namespace Domain.Language.ValueObjects;

public class LanguageId : SingleValueObject<short>
{
    private LanguageId(short id) : base(id)
    {
    }
    
    public static LanguageId CreateEmpty()
    {
        return new LanguageId(0);
    }

    public static Result<LanguageId> Create(short id)
    {
        return new LanguageId(id);
    }
}