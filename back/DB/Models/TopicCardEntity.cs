using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DB.Models;

public class TopicCardEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public short Id { get; set; }

    [Required]
    [StringLength(255)]
    [Column("RememberingText")]
    public string FrontSideText { get; set; }

    [StringLength(255)]
    public string PromptText { get; set; }

    [Required]
    [StringLength(255)]
    [Column("MeaningText")]
    public string BackSideText { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [MaxLength(15)]
    [StringLength(255)]
    public List<string>? Examples { get; set; }

    public long ParentTopicCollectionId { get; set; }
    public TopicCollectionEntity? TopicCollection { get; set; }
}