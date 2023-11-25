using FluentResults;

namespace Application;

public interface ICommand
{
}

public interface ICommand<in TRequest> : ICommand
{
    public Task<Result> Handle(TRequest request);
}

public interface ICommand<in TRequest, TResponse> : ICommand
{
    public Task<Result<TResponse>> Handle(TRequest request);
}