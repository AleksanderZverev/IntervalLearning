using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DB.Models;

public class TopicCardEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    [StringLength(255)]
    public string RememberingText { get; set; }

    [StringLength(255)]
    public string PromptText { get; set; }

    [Required]
    [StringLength(255)]
    public string MeaningText { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [MaxLength(15)]
    [StringLength(255)]
    public List<string>? Examples { get; set; }

    public long ParentTopicCollectionId { get; set; }
    public TopicCollectionEntity? TopicCollection { get; set; }
}