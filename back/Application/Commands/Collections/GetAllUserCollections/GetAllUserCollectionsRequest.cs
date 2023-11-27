using Domain.User.ValueObjects;

namespace Application.Commands.Collections.GetAll;

public record GetAllUserCollectionsRequest(
    UserId UserId
);