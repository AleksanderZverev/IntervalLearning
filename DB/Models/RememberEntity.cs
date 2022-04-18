using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;

namespace DB.Models;

[Table("RememberWeights")]
public class RememberEntity : IParentCardReference
{
    [Key]
    public short Id { get; set; }

    /// <summary>
    /// from 0.00 to 1.00
    /// </summary>
    /// <remarks>
    /// 0 if is for future
    /// </remarks>
    [Required]
    [Range(0d, 1d)]
    public float Weight { get; set; }

    [Required]
    public short PhaseStep { get; set; }

    /// <summary>
    /// Remembered or repeated date
    /// </summary>
    public DateTime RepeatedDate { get; set; }


    public RememberEntity(
        long parentUserId,
        short parentCollectionId,
        short parentCardId,
        float weight,
        short phaseStep,
        DateTime repeatedDate)
    {
        ParentUserId = parentUserId;
        ParentCollectionId = parentCollectionId;
        ParentCardId = parentCardId;
        Weight = weight;
        PhaseStep = phaseStep;
        RepeatedDate = repeatedDate;
    }

    public long ParentUserId { get; set; } 
    public UserEntity? ParentUser { get; set; }
    public short ParentCollectionId { get; set; }
    public CollectionEntity? ParentCollection { get; set; }
    public short ParentCardId { get; set; }
    public CardEntity? ParentCard { get; set; }
}