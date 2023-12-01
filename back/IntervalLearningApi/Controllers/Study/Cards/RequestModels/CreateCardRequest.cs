using System.ComponentModel.DataAnnotations;

namespace IntervalLearningApi.Controllers;

public class CreateCardRequest
{
    public short? CardId { get; set; }
    [Required]
    [StringLength(255)]
    public string FrontText { get; set; }

    [StringLength(255)] 
    public string? PromptText { get; set; }

    [Required]
    [StringLength(255)]
    public string BackText { get; set; }
    [StringLength(500)]
    public string? Description { get; set; }

    [MaxLength(15)]
    public List<string>? Examples { get; set; }
}