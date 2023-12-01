using Domain.User.ValueObjects;

namespace Application.Commands.Collections.GetAllUserCollections;

public record GetAllUserCollectionsRequest(
    UserId UserId
);