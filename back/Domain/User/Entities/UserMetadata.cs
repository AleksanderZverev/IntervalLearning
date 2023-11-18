using Domain.Language.ValueObjects;
using Domain.User.ValueObjects;

namespace Domain.User.Entities;

public class UserMetadata : Entity<UserId>
{
    public LanguageId SuggestTranslationLanguageId { get; set; }
    public Language.Language? SuggestTranslationLanguage { get; set; }

    public short NotStartedCollections { get; set; }
    public short StartedCollections { get; set; }
    public short FinishedCollections { get; set; }

    public short NotStartedCards { get; set; }
    public short StartedCards { get; set; }
    public short FinishedCards { get; set; }

    protected UserMetadata(UserId id) : base(id)
    {
    }
}