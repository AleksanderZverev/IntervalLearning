using Domain.User.ValueObjects;

namespace Application.Commands.Collections.GetRepeatCollections;

public record GetRepeatCollectionsRequest(
    UserId UserId
);