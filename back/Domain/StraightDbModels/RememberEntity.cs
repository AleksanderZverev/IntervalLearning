using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        short parentRepeatsScheduleId,
        UserId parentUserId,
        short parentCollectionId,
        short parentCardId,
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
    public short ParentCollectionId { get; set; }
    public CollectionEntity? ParentCollection { get; set; }
    public short ParentCardId { get; set; }
    public CardEntity? ParentCard { get; set; }

    public UserId ParentRepeatsScheduleUserId { get; set; }
    public short ParentRepeatsScheduleId { get; set; }
    public RepeatsScheduleEntity? ParentRepeatsSchedule { get; set; }

    public bool IsRemembered()
    {
        return Weight >= 0.70f;
    }

    public bool IsNotClearRemember()
    {
        return Weight >= 0.40f && Weight < 0.70f;
    }
}