using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DB.Models;

[Table("RememberWeights")]
public class RememberEntity : IParentCardReference
{
    [Key]
    public byte Id { get; set; }

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
    public byte PhaseStep { get; set; }

    public int PassedSecondsFromLastStep { get; set; }


    public RememberEntity(
        long parentUserId,
        short parentCollectionId,
        short parentCardId,
        float weight, 
        byte phaseStep, 
        int passedSecondsFromLastStep)
    {
        ParentUserId = parentUserId;
        ParentCollectionId = parentCollectionId;
        ParentCardId = parentCardId;
        Weight = weight;
        PhaseStep = phaseStep;
        PassedSecondsFromLastStep = passedSecondsFromLastStep;
    }

    public long ParentUserId { get; set; } 
    public UserEntity? ParentUser { get; set; }
    public short ParentCollectionId { get; set; }
    public CollectionEntity? ParentCollection { get; set; }
    public short ParentCardId { get; set; }
    public CardEntity? ParentCard { get; set; }
}