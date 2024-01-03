using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Common.Interfaces.DB.Repositories.Study.Cards;

public record CardIdParams(UserId UserId, CollectionId CollectionId);