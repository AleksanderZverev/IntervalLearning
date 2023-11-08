using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Domain.User;
using Domain.User.ValueObjects;

namespace DB.Models;

public class RefreshTokenEntity
{
    [JsonIgnore]
    public short Id { get; set; }

    public string Token { get; set; }
    public DateTime Expires { get; set; }
    public DateTime Created { get; set; }
    [StringLength(15)]
    public string CreatedByIp { get; set; }
    public DateTime? Revoked { get; set; }
    [StringLength(15)]
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? ReasonRevoked { get; set; }
    public bool IsExpired => DateTime.UtcNow > Expires;
    public bool IsRevoked => Revoked != null;
    public bool IsActive => !IsRevoked && !IsExpired;

    [JsonIgnore]
    public UserId ParentUserId { get; set; }
    [JsonIgnore]
    public User? ParentUser { get; set; }
}