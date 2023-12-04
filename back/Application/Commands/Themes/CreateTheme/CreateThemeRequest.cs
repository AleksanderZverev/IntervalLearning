using Domain.Theme.ValueObjects;

namespace Application.Commands.Themes.CreateTheme;

public record CreateThemeRequest(
    ThemeTitle Title
);