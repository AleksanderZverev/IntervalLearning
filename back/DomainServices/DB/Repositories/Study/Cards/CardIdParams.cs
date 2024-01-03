using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace DomainServices.DB.Repositories.Study.Cards;

public record CardIdParams(UserId UserId, CollectionId CollectionId);