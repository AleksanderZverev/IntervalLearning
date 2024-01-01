using FluentResults;

namespace Infrastructure.Errors;

public class InternalError : Error
{
    public InternalError() : base("Internal error")
    {
    }

    public InternalError(string message) : base(message)
    {
    }
}