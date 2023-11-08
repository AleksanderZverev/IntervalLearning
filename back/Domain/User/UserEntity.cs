using Domain.Language;
using Domain.Language.ValueObjects;
using Domain.User;
using Domain.User.ValueObjects;

namespace DB.Models;

public interface IParentUserReference
{
    public UserId ParentUserId { get; set; }
    public User? ParentUser { get; set; }
}

// public class UserEntity
// {
//     [Key]
//     public long Id { get; set; }
//         
//     [Required]
//     [MaxLength(50)]
//     public string FirstName { get; set; }
//
//     [Required]
//     [MaxLength(50)]
//     public string LastName { get; set; }
//
//     [Required]
//     [StringLength(255)]
//     public string Email { get; set; }
//
//     [JsonIgnore]
//     public UserPasswordsEntity? PasswordHash { get; set; }
//
//     public bool EmailConfirmed { get; set; }
//
//     [JsonIgnore] 
//     public List<RefreshTokenEntity> RefreshTokens { get; set; } = new();
//
//     public virtual List<CollectionEntity> Collections { get; set; } = new();
//     public virtual UserMetadataEntity Metadata { get; set; }
// }

public class UserMetadataEntity
{
    public UserId ParentUserId { get; set; }
    public User? ParentUser { get; set; }

    public LanguageId SuggestTranslationLanguageId { get; set; }
    public Language? SuggestTranslationLanguage { get; set; }

    public short NotStartedCollections { get; set; }
    public short StartedCollections { get; set; }
    public short FinishedCollections { get; set; }

    public short NotStartedCards { get; set; }
    public short StartedCards { get; set; }
    public short FinishedCards { get; set; }

    public UserMetadataEntity(UserId parentUserId, LanguageId suggestTranslationLanguageId)
    {
        ParentUserId = parentUserId;
        SuggestTranslationLanguageId = suggestTranslationLanguageId;
    }
}