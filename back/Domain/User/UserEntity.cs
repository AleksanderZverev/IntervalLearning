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