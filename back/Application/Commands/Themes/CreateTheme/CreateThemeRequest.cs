using DB.Models.ValueObjects;

namespace Application.Commands.Themes.CreateTheme;

public record CreateThemeRequest(
    ThemeTitle Title
);