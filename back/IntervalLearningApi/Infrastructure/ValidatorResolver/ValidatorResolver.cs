using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using Infrastructure.Errors;

namespace IntervalLearningApi.Infrastructure.ValidatorResolver;

public class ValidatorResolver
{
    private readonly IServiceProvider serviceProvider;

    public ValidatorResolver(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public IValidator<T> GetValidator<T>()
    {
        return serviceProvider.GetRequiredService<IValidator<T>>();
    }

    public Result Validate<T>(T item)
    {
        var validator = GetValidator<T>();
        var result = validator.Validate(item);
        return ToResult(result);
    }

    private Result ToResult(ValidationResult validationResult)
    {
        return validationResult.IsValid
            ? Result.Ok()
            : Result.Fail(new ValidationError(validationResult.Errors.First().ErrorMessage));
    }
}