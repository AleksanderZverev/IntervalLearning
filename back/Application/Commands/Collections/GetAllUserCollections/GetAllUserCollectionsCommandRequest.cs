using Domain.User.ValueObjects;

namespace Application.Commands.Collections.GetAllUserCollections;

public record GetAllUserCollectionsCommandRequest(
    UserId UserId
);