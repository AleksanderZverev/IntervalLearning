using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Collections.AddCardToCollection;

public record AddCardToCollectionRequest(
    UserId UserId,
    CollectionId CollectionId,
    CardText FrontText,
    CardText? PromptText,
    CardText BackText,
    CardDescription? Description,
    List<CardExample> Examples
);