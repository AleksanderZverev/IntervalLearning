using FluentResults;
using FluentValidation;

namespace IntervalLearningApi.Extensions;

public static class AutoMapperExtensions
{
    public static IRuleBuilderOptions<T, TProperty?> ShouldBeCreatableWhenNotNull<T, TProperty, TCreated>(
        this IRuleBuilder<T, TProperty?> ruleBuilder,
        Func<TProperty, Result<TCreated>> create)
        where TProperty : struct
    {
        return ruleBuilder.Must((parent, value, context) => 
        {
            if (value == null)
                return true;
            
            var creationResult = create(value.Value);
            
            if (creationResult.IsFailed)
            {
                context.AddFailure(creationResult.Errors.First().Message);
            }

            return creationResult.IsSuccess;
        });
    }
    
    public static IRuleBuilderOptions<T, TProperty> WhenNotNull<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> ruleBuilder)
    {
        return ruleBuilder.Configure(config => 
        {
            config.ApplyCondition(context => config.GetPropertyValue(context.InstanceToValidate) != null);
        });
    }
    
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