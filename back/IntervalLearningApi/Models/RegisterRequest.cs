using System.ComponentModel.DataAnnotations;

namespace IntervalLearningApi.Models;

public class RegisterRequest
{
    [Required]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }

    [Required]
    public string FirstName { get; set; }

    [Required]
    public short SuggestLanguageId { get; set; }

    public string? LastName { get; set; }
}