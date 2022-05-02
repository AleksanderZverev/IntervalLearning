using System.ComponentModel.DataAnnotations.Schema;

namespace DB.Models;

[Table("Queue")]
public class CardRepeatQueueEntity : IParentCardReference
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public short Id { get; set; }
    public short PhaseIndex { get; set; }

    public DateTime Date { get; set; }

    public CardRepeatQueueEntity(
        long parentRepeatsScheduleUserId,
        short parentRepeatsScheduleId,
        long parentUserId, 
        short parentCollectionId, 
        short parentCardId,
        short phaseIndex, 
        DateTime date)
    {
        ParentUserId = parentUserId;
        ParentCollectionId = parentCollectionId;
        ParentCardId = parentCardId;
        PhaseIndex = phaseIndex;
        Date = date;
        ParentRepeatsScheduleUserId = parentRepeatsScheduleUserId;
        ParentRepeatsScheduleId = parentRepeatsScheduleId;
    }

    public long ParentUserId { get; set; }

    public UserEntity? ParentUser { get; set; }

    public short ParentCollectionId { get; set; }

    public CollectionEntity? ParentCollection { get; set; }

    public short ParentCardId { get; set; }

    public CardEntity? ParentCard { get; set; }

    public long ParentRepeatsScheduleUserId { get; set; }
    public short ParentRepeatsScheduleId { get; set; }
    public RepeatsScheduleEntity? ParentRepeatsSchedule { get; set; }
}