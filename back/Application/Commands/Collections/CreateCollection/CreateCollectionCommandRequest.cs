using Domain.Collection.ValueObjects;
using Domain.Theme.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Collections.CreateCollection;

public record CreateCollectionCommandRequest
{
    public required UserId ParentUserId { get; init; }
    public required ThemeId ThemeId { get; init; }
    public required CollectionTitle Title { get; init; }
    public bool IsDefaultBackSide { get; init; }
}