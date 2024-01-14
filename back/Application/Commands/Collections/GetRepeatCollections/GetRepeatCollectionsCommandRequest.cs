using Domain.User.ValueObjects;

namespace Application.Commands.Collections.GetRepeatCollections;

public record GetRepeatCollectionsCommandRequest(
    UserId UserId,
    DateTime? UntilDate
);