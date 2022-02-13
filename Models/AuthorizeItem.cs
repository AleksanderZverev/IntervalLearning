using System.ComponentModel.DataAnnotations;

namespace IntervalLearningApi.Models;

public class AuthorizeItem
{
    [Required]
    public string Email { get; set; }

    [Required] 
    public string Password { get; set; }
}