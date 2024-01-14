using FluentResults;

namespace GlobalTools.Errors;

public class BadRequestError : Error
{
    public BadRequestError() : base("Bad request error")
    {
    }
    
    public BadRequestError(string message) : base(message)
    {
    }
}