using System.Linq.Expressions;
using Domain.Collection.ValueObjects;
using Domain.Common.Specifications;
using Domain.User.ValueObjects;

namespace Domain.Card.Specifications;

public class CardByIdSpecification : AbstractSpecification<Card>
{
    private readonly UserId userId;
    private readonly CollectionId collectionId;

    public CardByIdSpecification(UserId userId, CollectionId collectionId)
    {
        this.userId = userId;
        this.collectionId = collectionId;
    }

    protected override Expression<Func<Card, bool>> GetQuery()
    {
        return c => c.ParentUserId == userId && c.ParentCollectionId == collectionId;
    }
}