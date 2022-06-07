using System.ComponentModel.DataAnnotations.Schema;

namespace DB.Models.Store;

[Table("PublicCollectionSubscriber")]
public class PublicCollectionSubscriber
{
    [Column("OwnerUserId")]
    public long CollectionOwnerId { get; set; }
    public UserEntity? CollectionOwner { get; set; }

    public short PublicCollectionId { get; set; }
    public PublicCollectionEntity? PublicCollection { get; set; }

    public long SubscriberUserId { get; set; }
    public UserEntity? SubscriberUser { get; set; }

    public bool IsLiked { get; set; }
    public bool IsDisliked { get; set; }
    public bool IsAdded { get; set; }
}