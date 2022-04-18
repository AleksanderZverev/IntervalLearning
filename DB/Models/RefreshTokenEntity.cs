using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace DB.Models;

public class RefreshTokenEntity : IParentUserReference
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
    public bool IsExpired => DateTime.UtcNow > Expires; // SystemClock.Instance.GetCurrentInstant() > Expires;
    public bool IsRevoked => Revoked != null;
    public bool IsActive => !IsRevoked && !IsExpired;

    [JsonIgnore]
    public long ParentUserId { get; set; }
    [JsonIgnore]
    public UserEntity? ParentUser { get; set; }
}