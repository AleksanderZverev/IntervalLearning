using Domain.Theme.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Collections.SearchPublicCollection;

public record SearchPublicCollectionCommandRequest(
    UserId MyUserId,
    ThemeId ThemeId,
    string SearchName,
    int Page,
    int Count
);