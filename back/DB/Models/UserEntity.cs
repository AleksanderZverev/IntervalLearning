using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DB.Models;

public interface IParentUserReference
{
    public long ParentUserId { get; set; }
    public UserEntity? ParentUser { get; set; }
}

public class UserEntity
{
    [Key]
    public long Id { get; set; }
        
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; }

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; }

    [Required]
    [StringLength(255)]
    public string Email { get; set; }

    [JsonIgnore]
    public UserPasswordsEntity? PasswordHash { get; set; }

    public bool EmailConfirmed { get; set; }

    [JsonIgnore] 
    public List<RefreshTokenEntity> RefreshTokens { get; set; } = new();

    public virtual List<CollectionEntity> Collections { get; set; } = new();
}

public class UserMetadataEntity : IParentUserReference
{
    public long ParentUserId { get; set; }
    public UserEntity? ParentUser { get; set; }

    public short NotStartedCollections { get; set; }
    public short StartedCollections { get; set; }
    public short FinishedCollections { get; set; }

    public short NotStartedCards { get; set; }
    public short StartedCards { get; set; }
    public short FinishedCards { get; set; }

    public UserMetadataEntity(long parentUserId)
    {
        ParentUserId = parentUserId;
    }
}