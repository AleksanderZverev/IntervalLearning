using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Infrastructure;

namespace DB.Models.Store;

public interface IEditModel
{
    public string Title { get; }
    public short ThemeId { get; }
    public string ShortDescription { get; }
}

public interface ICreateModel : IEditModel
{
    public long OwnerUserId { get; }
}

[Table("PublicCollections")]
public class PublicCollectionEntity : ICreateModel
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public short Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string ShortDescription { get; set; } = string.Empty;

    public short ThemeId { get; set; }
    public ThemeEntity? Theme { get; set; }

    [Required] 
    public DateOnly PublishDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public short CardsCount { get; set; }
    public List<PublicCardEntity> Cards { get; set; } = new();

    public long OwnerUserId { get; set; }
    public UserEntity? OwnerUser { get; set; }

    public uint SubscribersCount { get; set; }
    public uint LikesCount { get; set; }
    public uint DislikesCount { get; set; }
    public List<PublicCollectionSubscriber> Subscribers { get; set; } = new();
}

public class PatchPublicCollection : IEditModel
{
    public string Title { get; }
    public short ThemeId { get; }
    public string ShortDescription { get; }


    public PatchPublicCollection(string title, string shortDescription, short themeId)
    {
        Title = TextMaster.RemoveWhiteSpaces(title);
        ShortDescription = TextMaster.RemoveWhiteSpaces(shortDescription);
        ThemeId = themeId;
    }
}

public class CreatePublicCollection : PatchPublicCollection, ICreateModel
{
    public long OwnerUserId { get; }

    public CreatePublicCollection(
        long ownerUserId,
        string title,
        string shortDescription,
        short themeId)
        : base(
            title,
            shortDescription,
            themeId)
    {
        OwnerUserId = ownerUserId;
    }
}