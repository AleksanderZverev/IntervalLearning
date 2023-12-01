using FluentValidation;

namespace IntervalLearningApi.Controllers.Accounts.Models.RevokeToken;

public class RevokeTokenRequestValidator : AbstractValidator<RevokeTokenRequest>
{
    public RevokeTokenRequestValidator()
    {
    }
}

public class RevokeTokenRequest
{
    public string? Token { get; set; }
}