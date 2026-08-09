using Domain.Theme.ValueObjects;
using FluentValidation;
using IntervalLearningApi.Extensions;

namespace IntervalLearningApi.Controllers.Study.Themes.Requests;

public class ThemeRequestValidator : AbstractValidator<ThemeRequest>
{
    public ThemeRequestValidator()
    {
        RuleFor(p => p.Name).ShouldBeCreatable(ThemeTitle.Create);
    }
}

public class ThemeRequest
{
    public string Name { get; set; }
}
