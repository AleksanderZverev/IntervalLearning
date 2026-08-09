using Domain.Theme.ValueObjects;

namespace Application.Commands.Themes.DeleteTheme;

public record DeleteThemeRequest(ThemeId Id);
