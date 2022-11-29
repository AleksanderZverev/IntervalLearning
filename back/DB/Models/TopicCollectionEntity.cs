using System.ComponentModel.DataAnnotations.Schema;

namespace DB.Models;

public class TopicCollectionEntity
{   
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    public string Name { get; set; }

    public long ParentTopicId { get; set; }
    public TopicEntity? Topic { get; set; }

    public List<TopicCardEntity> Cards { get; set; } = new();
}