using System.ComponentModel.DataAnnotations;
using Domain.Common.ValueObjects;
using Domain.Language.ValueObjects;
using Domain.User.ValueObjects;
using FluentValidation;
using IntervalLearningApi.Extensions;

namespace IntervalLearningApi.Controllers.Accounts.Models.Register;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(p => p.Email).ShouldBeCreatable(EmailAddress.Create);
        RuleFor(p => p.Password).ShouldBeCreatable(MediumSingleLineString.Create);
        RuleFor(p => p.FirstName).ShouldBeCreatable(PartedName.Create);
        RuleFor(p => p.LastName).ShouldBeCreatable(PartedName.Create);
        RuleFor(p => p.SuggestLanguageId).ShouldBeCreatable(LanguageId.Create);
    }
}

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