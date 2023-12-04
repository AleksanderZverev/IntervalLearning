using System.ComponentModel.DataAnnotations;
using Domain.Common.ValueObjects;
using Domain.Common.ValueObjects.Text.SingleLine;
using Domain.User.ValueObjects;
using FluentValidation;
using IntervalLearningApi.Extensions;

namespace IntervalLearningApi.Controllers.Accounts.Requests.Authenticate;

public class AuthenticateRequestValidator : AbstractValidator<AuthenticateRequest>
{
    public AuthenticateRequestValidator()
    {
        RuleFor(r => r.Email).ShouldBeCreatable(EmailAddress.Create);
        RuleFor(r => r.Password).ShouldBeCreatable(MediumSingleLineString.Create);
    }
}

public class AuthenticateRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}