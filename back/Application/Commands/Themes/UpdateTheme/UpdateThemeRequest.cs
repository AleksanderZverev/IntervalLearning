using Domain.Theme.ValueObjects;

namespace Application.Commands.Themes.UpdateTheme;

public record UpdateThemeRequest(
    ThemeId Id,
    ThemeTitle Title
);
