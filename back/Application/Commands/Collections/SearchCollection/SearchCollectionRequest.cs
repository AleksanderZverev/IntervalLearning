using DB.Models.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Collections.SearchCollection;

public record SearchCollectionRequest(
    UserId UserId,
    ThemeId ThemeId, 
    string SearchName, 
    int Page, 
    int Count
);