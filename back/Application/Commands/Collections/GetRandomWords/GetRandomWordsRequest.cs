using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Collections.GetRandomWords;

public record GetRandomWordsRequest(
    UserId UserId, 
    CollectionId CollectionId
);