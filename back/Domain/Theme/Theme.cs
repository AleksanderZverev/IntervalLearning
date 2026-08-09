using Domain.Language.ValueObjects;
using Domain.Theme.ValueObjects;

namespace Domain.Theme;

public class Theme : Entity<ThemeId>
{
    public required ThemeTitle Name { get; set; }

    public LanguageId? LanguageId { get; set; }
    public virtual Language.Language? Language { get; set; }

    public Theme(ThemeId id) : base(id)
    {
    }
}