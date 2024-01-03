using FluentResults;

namespace GlobalTools.Errors;

public class InternalError : Error
{
    public InternalError() : base("Internal error")
    {
    }

    public InternalError(string message) : base(message)
    {
    }
}