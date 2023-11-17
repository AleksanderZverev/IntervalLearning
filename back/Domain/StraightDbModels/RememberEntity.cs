using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DB.Models.ValueObjects;
using Domain.Card;
using Domain.Card.ValueObjects;
using Domain.Collection;
using Domain.Collection.ValueObjects;
using Domain.Schedule;
using Domain.User;
using Domain.User.ValueObjects;

namespace DB.Models;

[Table("RememberWeights")]
public class RememberEntity : IParentCardReference
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public short Id { get; set; }

    /// <summary>
    /// from 0.00 to 1.00
    /// </summary>
    [Required]
    [Range(0d, 1d)]
    public float Weight { get; set; }

    [Required]
    public short PhaseIndex { get; set; }

    /// <summary>
    /// Remembered or repeated date
    /// </summary>
    public DateTime RepeatedDate { get; set; }
    
    public RememberEntity(
        UserId parentRepeatsScheduleUserId, 
        ScheduleId parentRepeatsScheduleId,
        UserId parentUserId,
        CollectionId parentCollectionId,
        CardId parentCardId,
        float weight,
        short phaseIndex,
        DateTime repeatedDate)
    {
        ParentUserId = parentUserId;
        ParentCollectionId = parentCollectionId;
        ParentCardId = parentCardId;
        Weight = weight;
        PhaseIndex = phaseIndex;
        RepeatedDate = repeatedDate;
        ParentRepeatsScheduleUserId = parentRepeatsScheduleUserId;
        ParentRepeatsScheduleId = parentRepeatsScheduleId;
    }

    public UserId ParentUserId { get; set; } 
    public User? ParentUser { get; set; }
    public CollectionId ParentCollectionId { get; set; }
    public Collection? ParentCollection { get; set; }
    public CardId ParentCardId { get; set; }
    public Card? ParentCard { get; set; }

    public UserId ParentRepeatsScheduleUserId { get; set; }
    public ScheduleId ParentRepeatsScheduleId { get; set; }
    public RepeatsSchedule? ParentRepeatsSchedule { get; set; }

    public bool IsRemembered()
    {
        return Weight >= 0.70f;
    }

    public bool IsNotClearRemember()
    {
        return Weight >= 0.40f && Weight < 0.70f;
    }
}