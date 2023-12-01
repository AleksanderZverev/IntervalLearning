using FluentResults;
using FluentValidation;

namespace IntervalLearningApi.Extensions;

public static class AutoMapperExtensions
{
    public static IRuleBuilderOptions<T, TProperty> ShouldBeCreatable<T, TProperty, TCreated>(
        this IRuleBuilder<T, TProperty> ruleBuilder,
        Func<TProperty, Result<TCreated>> create)
    {
        return ruleBuilder.Must((parent, value, context) => 
        {
            var creationResult = create(value);
            
            if (creationResult.IsFailed)
            {
                context.AddFailure(creationResult.Errors.First().Message);
            }

            return creationResult.IsSuccess;
        });
    }
}