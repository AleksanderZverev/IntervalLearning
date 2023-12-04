using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Domain.User.ValueObjects;

namespace Domain.User.Entities;

public class RefreshTokenEntity
{
    public required short Id { get; init; }
    public required string Token { get; init; }
    public required DateTime Expires { get; init; }
    public required DateTime Created { get; init; }
    public required string CreatedByIp { get; init; }
    
    public DateTime? Revoked { get; private set; }
    public string? RevokedByIp { get; private set; }
    public string? ReplacedByToken { get; private set; }
    public string? ReasonRevoked { get; private set; }
    
    public bool IsExpired => DateTime.UtcNow > Expires;
    public bool IsRevoked => Revoked != null;
    public bool IsActive => !IsRevoked && !IsExpired;

    public UserId ParentUserId { get; set; }
    public User? ParentUser { get; set; }
    
    public void Revoke(
        DateTime revokeDate,
        string ipAddress, 
        string reason = null, 
        string replacedByToken = null)
    {
        Revoked = revokeDate;
        RevokedByIp = ipAddress;
        ReasonRevoked = reason;
        ReplacedByToken = replacedByToken;
    }
}