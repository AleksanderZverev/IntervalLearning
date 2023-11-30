using FluentResults;

namespace Infrastructure.Errors;

public class InternalError : Error
{
    public InternalError() : base("Internal error")
    {
    }
}