using System.Text.Json.Serialization;
using DB.Models;
using Domain.Common.ValueObjects;
using Domain.User.ValueObjects;
using FluentResults;

namespace Domain.User;

public class User : AggregateRoot<UserId>
{
    protected User(UserId id) : base(id)
    {
    }
    
    public required UserName UserName { get; init; }
    public required EmailAddress Email { get; init; }
    
    public bool EmailConfirmed { get; private set; }
    
    public virtual UserMetadataEntity Metadata { get; set; }
    
    [JsonIgnore]
    public UserPasswordsEntity? PasswordHash { get; set; }
    [JsonIgnore] 
    public List<RefreshTokenEntity> RefreshTokens { get; set; } = new();
    
    
    private readonly List<CollectionEntity> collections = new();
    public IReadOnlyCollection<CollectionEntity> Collections => collections.AsReadOnly();
    

    public static Result<User> Create(UserId userId, EmailAddress email, UserName userName)
    {
        return new User(userId)
        {
            Email = email,
            UserName = userName
        };
    }

    
    // public Counter CollectionsCount { get; init; } = Counter.CreateEmpty();
    
    // public void AddCollection(Collection.Collection collection)
    // {
    //     collections.Add(collection);
    //     
    //     CollectionsCount.Increment();
    //     AddDomainEvent(new CollectionAdded(collection));
    // }
    //
    // public void MoveCardToAnotherCollection(CardId movingCardId, CollectionId newCollectionId)
    // {
    //
    //     // movingCardId.CollectionId
    //     // AddDomainEvent();
    // }
    //
    // public Result<Collection.Collection> GetCollection(CollectionId collectionId)
    // {
    //     throw new NotImplementedException();
    // } 
}

