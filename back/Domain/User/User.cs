using System.Text.Json.Serialization;
using Domain.Common.ValueObjects;
using Domain.User.Entities;
using Domain.User.ValueObjects;
using FluentResults;
using UserMetadata = Domain.User.Entities.UserMetadata;

namespace Domain.User;

public class User : AggregateRoot<UserId>
{
    public User(UserId id) : base(id)
    {
    }
    
    public required UserName UserName { get; init; }
    public required EmailAddress Email { get; init; }
    
    public bool EmailConfirmed { get; private set; }
    
    public virtual UserMetadata Metadata { get; set; }
    
    [JsonIgnore]
    public UserPassword? PasswordHash { get; set; }
    [JsonIgnore] 
    public List<RefreshTokenEntity> RefreshTokens { get; set; } = new();
}

