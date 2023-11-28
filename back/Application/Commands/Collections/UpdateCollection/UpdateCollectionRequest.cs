using DB.Models.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Collections.UpdateCollection;

public record UpdateCollectionRequest
{
    public required CollectionId CollectionId { get; init; }
    public required UserId ParentUserId { get; init; }
    public required ThemeId ThemeId { get; init; }
    public required CollectionTitle Title { get; init; }
    public bool IsDefaultBackSide { get; init; }
}