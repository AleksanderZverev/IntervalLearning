using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace DB.Models;

public class RefreshTokenEntity : IParentUserReference
{
    [JsonIgnore]
    public byte Id { get; set; }

    public string Token { get; set; }
    public Instant Expires { get; set; }
    public Instant Created { get; set; }
    [StringLength(15)]
    public string CreatedByIp { get; set; }
    public Instant? Revoked { get; set; }
    [StringLength(15)]
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? ReasonRevoked { get; set; }
    public bool IsExpired => SystemClock.Instance.GetCurrentInstant() > Expires;
    public bool IsRevoked => Revoked != null;
    public bool IsActive => !IsRevoked && !IsExpired;

    [JsonIgnore]
    public long ParentUserId { get; set; }
    [JsonIgnore]
    public UserEntity? ParentUser { get; set; }
}