using System.ComponentModel.DataAnnotations.Schema;
using Domain.User;
using Domain.User.ValueObjects;

namespace DB.Models;

[Table("Queue")]
public class CardRepeatQueueEntity : IParentCardReference
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public short Id { get; set; }
    public short PhaseIndex { get; set; }

    public DateTime Date { get; set; }

    public CardRepeatQueueEntity(
        UserId parentRepeatsScheduleUserId,
        short parentRepeatsScheduleId,
        UserId parentUserId, 
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

    public UserId ParentUserId { get; set; }

    public User? ParentUser { get; set; }

    public short ParentCollectionId { get; set; }

    public CollectionEntity? ParentCollection { get; set; }

    public short ParentCardId { get; set; }

    public CardEntity? ParentCard { get; set; }

    public UserId ParentRepeatsScheduleUserId { get; set; }
    public short ParentRepeatsScheduleId { get; set; }
    public RepeatsScheduleEntity? ParentRepeatsSchedule { get; set; }
}