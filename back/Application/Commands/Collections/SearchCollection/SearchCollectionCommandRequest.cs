using Domain.Theme.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Collections.SearchCollection;

public record SearchCollectionCommandRequest(
    UserId UserId,
    ThemeId ThemeId, 
    string SearchName, 
    int Page, 
    int Count
);