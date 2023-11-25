using DB.Models.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Collections.SearchPublicCollection;

public record SearchPublicCollectionRequest(
    UserId MyUserId,
    ThemeId ThemeId,
    string SearchName,
    int Page,
    int Count
);