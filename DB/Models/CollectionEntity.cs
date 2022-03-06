using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;

namespace DB.Models;

public interface IParentCollectionReference : IParentUserReference
{
    public short ParentCollectionId { get; set; }
    public CollectionEntity? ParentCollection { get; set; }
}

[Table("Collections")]
public class CollectionEntity : IParentUserReference
{
    public short Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Title { get; set; }

    public bool IsDefaultBackSide { get; set; }

    public short ThemeId { get; set; }
    public virtual ThemeEntity? Theme { get; set; }

    [Required]
    public Instant CreatedDate { get; set; } = SystemClock.Instance.GetCurrentInstant();

    public virtual List<CardEntity> Cards { get; set; } = new();


    public short DefaultRepeatsScheduleId { get; set; }
    public virtual RepeatsScheduleEntity? DefaultRepeatsSchedule { get; set; }

    public long ParentUserId { get; set; }
    public virtual UserEntity? ParentUser { get; set; }

    public CollectionEntity(
        long parentUserId,
        short defaultRepeatsScheduleId,
        short themeId,
        string title, 
        bool isDefaultBackSide)
    {
        ParentUserId = parentUserId;
        DefaultRepeatsScheduleId = defaultRepeatsScheduleId;
        Title = title;
        IsDefaultBackSide = isDefaultBackSide;
        ThemeId = themeId;
    }
}