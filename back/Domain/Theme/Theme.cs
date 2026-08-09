using Domain.Language.ValueObjects;
using Domain.Theme.ValueObjects;

namespace Domain.Theme;

public class Theme : Entity<ThemeId>
{
    public ThemeTitle Name { get; private set; }

    public LanguageId? LanguageId { get; private set; }
    public virtual Language.Language? Language { get; private set; }

    public Theme(ThemeId id, ThemeTitle name) : base(id)
    {
        Name = name;
    }

    public void Update(ThemeTitle name)
    {
        Name = name;
    }
}