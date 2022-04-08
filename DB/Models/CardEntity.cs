using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;

namespace DB.Models;

public interface IParentCardReference : IParentCollectionReference
{
    public short ParentCardId { get; set; }
    public CardEntity? ParentCard { get; set; }
}

[Table("Cards")]
public class CardEntity : IParentCollectionReference, IParentRepeatsScheduleReference
{
    public short Id { get; set; }

    [Required]
    [StringLength(255)]
    public string FrontSideText { get; set; }

    [Required]
    [StringLength(255)]
    public string BackSideText { get; set; }

    [Required]
    public Instant CreatedDate { get; set; } = SystemClock.Instance.GetCurrentInstant();

    [StringLength(500)]
    public string? Description { get; set; }

    [MaxLength(15)]
    [StringLength(255)]
    public List<string>? Examples { get; set; }

    /// <summary>
    /// null - not started, false - started, true - finished
    /// </summary>
    public bool? IsFinished { get; set; }

    public virtual List<RememberEntity> Remembers { get; set; } = new();

    public CardEntity(
        long parentUserId,
        short parentCollectionId,
        string frontSideText,
        string backSideText,
        long parentRepeatsScheduleUserId,
        short parentRepeatsScheduleId,
        string? description = null,
        List<string>? examples = null)
    {
        ParentUserId = parentUserId;
        ParentCollectionId = parentCollectionId;
        FrontSideText = frontSideText;
        BackSideText = backSideText;
        ParentRepeatsScheduleUserId = parentRepeatsScheduleUserId;
        ParentRepeatsScheduleId = parentRepeatsScheduleId;
        Description = description;
        Examples = examples;
    }

    public long ParentRepeatsScheduleUserId { get; set; }
    public short ParentRepeatsScheduleId { get; set; }
    public virtual RepeatsScheduleEntity? ParentRepeatsSchedule { get; set; }

    public long ParentUserId { get; set; }
    public UserEntity? ParentUser { get; set; }

    public short ParentCollectionId { get; set; }
    public CollectionEntity? ParentCollection { get; set; }
}

[Table("Queue")]
public class CardRepeatQueueEntity : IParentCardReference
{
    public byte Id { get; set; }

    public byte PhaseStep { get; set; }
    public Instant Date { get; set; }

    public long ParentUserId { get; set; }
    public UserEntity? ParentUser { get; set; }
    public short ParentCollectionId { get; set; }
    public CollectionEntity? ParentCollection { get; set; }
    public short ParentCardId { get; set; }
    public CardEntity? ParentCard { get; set; }
}