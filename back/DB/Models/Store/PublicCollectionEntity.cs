using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DB.Models.Store;

[Table("PublicCollections")]
public class PublicCollectionEntity
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