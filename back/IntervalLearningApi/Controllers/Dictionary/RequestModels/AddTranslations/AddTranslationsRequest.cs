using Domain.Language;
using Domain.Language.ValueObjects;
using FluentValidation;
using IntervalLearningApi.Extensions;

namespace IntervalLearningApi.Controllers;

public class AddTranslationsRequestValidator : AbstractValidator<AddTranslationsRequest>
{
    public AddTranslationsRequestValidator()
    {
        RuleFor(p => p.LanguageId).ShouldBeCreatable(LanguageId.Create);
        RuleFor(p => p.TranslationLanguageId).ShouldBeCreatable(LanguageId.Create);
        RuleFor(p => p.Text).NotNull().NotEmpty();
    }
}

public class AddTranslationsRequest
{
    public short LanguageId { get; set; }
    public short TranslationLanguageId { get; set; }
    public string Text { get; set; }
}