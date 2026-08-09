using Domain.Common.ValueObjects;
using FluentValidation;
using IntervalLearningApi.Extensions;

namespace IntervalLearningApi.Controllers.Dictionary.Requests.Language;

public class LanguageRequestValidator : AbstractValidator<LanguageRequest>
{
    public LanguageRequestValidator()
    {
        RuleFor(p => p.Name).ShouldBeCreatable(ShortString.Create);
        RuleFor(p => p.NativeLanguageName).ShouldBeCreatable(ShortString.Create);
        When(p => !string.IsNullOrEmpty(p.TranslationLinkTitle), () =>
        {
            RuleFor(p => p.TranslationLinkTitle).ShouldBeCreatable(ShortString.Create);
        });
    }
}

public class LanguageRequest
{
    public string Name { get; set; }
    public string NativeLanguageName { get; set; }
    public string? TranslationLink { get; set; }
    public string? TranslationLinkTitle { get; set; }
}
