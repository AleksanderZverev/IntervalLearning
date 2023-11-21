using FluentResults;
using Infrastructure.Errors;
using Microsoft.AspNetCore.Mvc;

namespace IntervalLearningApi.Extensions;

public static class ResultsExtensions
{
    public static ActionResult ToErrorActionResult<TResponse>(this Result<TResponse> result)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException();

        return MapErrorToActionResult(result.Errors);
    }
    
    public static ActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
            return new OkResult();

        return MapErrorToActionResult(result.Errors);
    }
    
    public static ActionResult<TResponse> ToActionResult<TResponse>(this Result<TResponse> result)
    {
        return result.ToActionResult(_ => _);
    }
    
    public static ActionResult<TResponse> ToActionResult<T, TResponse>(this Result<T> result, Func<T, TResponse> map)
    {
        if (result.IsSuccess)
        {
            try
            {
                var response = map(result.Value);
                return new OkObjectResult(response);
            }
            catch (Exception e)
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
        
        return MapErrorToActionResult(result.Errors);
    }

    private static ActionResult MapErrorToActionResult(List<IError> errors)
    {
        var error = errors.First();
        return error switch
        {
            NotFoundResult => new NotFoundObjectResult(error.Message),
            BadRequestError => new BadRequestObjectResult(error.Message),
            InternalError => new StatusCodeResult(StatusCodes.Status500InternalServerError),
            ConflictError => new ConflictObjectResult(error.Message),
            ForbiddenError => new StatusCodeResult(StatusCodes.Status403Forbidden),
            _ => new BadRequestResult(),
        };
    }
}