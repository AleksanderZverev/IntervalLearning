using DB.Models.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Collections.CreateCollection;

public record CreateCollectionRequest
{
    public required UserId ParentUserId { get; init; }
    public required ThemeId ThemeId { get; init; }
    public required ThemeTitle Title { get; init; }
    public bool IsDefaultBackSide { get; init; }
}