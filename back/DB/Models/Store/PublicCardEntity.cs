using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Infrastructure;

namespace DB.Models.Store;

[Table("PublicCards")]
public class PublicCardEntity : ICreatePublicCardModel
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public short Id { get; set; }

    [Required]
    [StringLength(255)]
    public string RememberingText { get; set; } = string.Empty;

    [StringLength(255)]
    public string PromptText { get; set; } = string.Empty;

    [StringLength(255)]
    public string MeaningText { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [MaxLength(15)]
    [StringLength(255)]
    public List<string>? Examples { get; set; }

    public long OwnerUserId { get; set; }
    public UserEntity? OwnerUser { get; set; }

    public short PublicCollectionId { get; set; }
    public PublicCollectionEntity? PublicCollection { get; set; }
}

public interface IEditPublicCardModel
{
    public string RememberingText { get; }
    public string PromptText { get; }
    public string MeaningText { get; }
    public string? Description { get; }
    public List<string>? Examples { get; }
}

public interface ICreatePublicCardModel : IEditPublicCardModel
{
    public long OwnerUserId { get; }
    public short PublicCollectionId { get; }
}

public class PatchPublicCard : IEditPublicCardModel
{
    public string RememberingText { get; }
    public string PromptText { get; }
    public string MeaningText { get; }
    public string? Description { get; }
    public List<string>? Examples { get; }

    public PatchPublicCard(
        string rememberingText,
        string promptText,
        string meaningText,
        string? description,
        List<string>? examples)
    {
        RememberingText = TextMaster.RemoveWhiteSpaces(rememberingText);
        PromptText = TextMaster.RemoveWhiteSpaces(promptText);
        MeaningText = TextMaster.RemoveWhiteSpaces(meaningText);
        Description = TextMaster.RemoveWhiteSpaces(description);
        Examples = examples?
            .Select(e => TextMaster.RemoveWhiteSpaces(e))
            .Where(e => !string.IsNullOrEmpty(e))
            .ToList();
    }
}

public class CreatePublicCard : PatchPublicCard, ICreatePublicCardModel
{
    public long OwnerUserId { get; }
    public short PublicCollectionId { get; }

    public CreatePublicCard(
        long ownerUserId, 
        short publicCollectionId,
        string rememberingText,
        string promptText,
        string meaningText,
        string? description,
        List<string>? examples) : base(
        rememberingText,
        promptText,
        meaningText,
        description,
        examples)
    {
        OwnerUserId = ownerUserId;
        PublicCollectionId = publicCollectionId;
    }
}