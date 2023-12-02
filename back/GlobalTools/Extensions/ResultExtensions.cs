using System.Runtime.CompilerServices;
using FluentResults;

namespace Infrastructure.Extensions;

public static class ResultExtensions
{
    // public static Task<Result<TNewValue>> ErrorIfNull<TNewValue>(
    //     this Task<TNewValue?> task,
    //     IError error)
    //     where TNewValue : class
    //     => ErrorIf(task, v => v == null, error);

    // public static async Task<Result<TNewValue?>> ErrorIf<TNewValue>(
    //     this Task<TNewValue?> task,
    //     Func<TNewValue?, bool> isError,
    //     IError error)
    // {
    //     var value = await task;
    //     return isError(value)
    //         ? Result.Fail(error)
    //         : value;
    // }

    public static bool HasAnyError(this ITuple tuple)
    {
        for (var i = 0; i < tuple.Length; i++)
        {
            var item = tuple[i];
            
            if (item is IResultBase { IsFailed: true }) 
                return true;
        }

        return false;
    }

    public static Task<Result<TNewValue>> ErrorIfNull<TNewValue>(
        this Task<Result<TNewValue?>> task,
        IError error)
        where TNewValue : class
        => ErrorIf(task, v => v == null, error);
    
    public static async Task<Result<TNewValue?>> ErrorIf<TNewValue>(
        this Task<Result<TNewValue?>> task,
        Func<TNewValue?, bool> isError,
        IError error)
    {
        var valueResult = await task;

        if (valueResult.IsFailed)
            return valueResult;

        var value = valueResult.Value;
        return isError(value)
            ? Result.Fail(error)
            : value;
    }

    public static async Task<Result<TNewValue>> ToResultAsync<TNewValue>(this Task<TNewValue> taskValue)
    {
        var value = await taskValue;
        return Result.Ok(value);
    }


    public static Task<Result<TNewValue>> BindAsync<TNewValue>(this Result result, Func<Task<Result<TNewValue>>> bind)
    {
        return result.Bind(bind);
    }
    
    public static Task<Result<TNewValue>> BindAsync<TOldValue, TNewValue>(this Result<TOldValue> result, Func<TOldValue, Task<Result<TNewValue>>> bind)
    {
        return result.Bind(bind);
    }
    
    public static Result<TNewValue> TryBind<TNewValue>(this Result result, Func<TNewValue> bind)
    {
        return result.Bind(() => Result.Try(bind));
    }
    
    public static Result<TNewValue> TryBind<TOldValue, TNewValue>(this Result<TOldValue> result, Func<TOldValue, TNewValue> bind)
    {
        return result.Bind((v) => Result.Try(() => bind(v)));
    }
}