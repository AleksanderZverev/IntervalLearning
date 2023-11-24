using FluentResults;

namespace Application;

public interface ICommand<in TRequest>
{
    public Task<Result> Handle(TRequest request);
}

public interface ICommand<in TRequest, TResponse>
{
    public Task<Result<TResponse>> Handle(TRequest request);
}