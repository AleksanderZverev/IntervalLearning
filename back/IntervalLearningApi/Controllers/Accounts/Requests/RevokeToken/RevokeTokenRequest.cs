using FluentValidation;

namespace IntervalLearningApi.Controllers.Accounts.Requests.RevokeToken;

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