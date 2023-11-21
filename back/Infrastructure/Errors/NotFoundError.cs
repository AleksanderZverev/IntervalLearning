using FluentResults;

namespace Infrastructure.Errors;

public class NotFoundError : Error
{
    public NotFoundError() : base("Not found error")
    {
    }
    
    public NotFoundError(string notFoundObjectName) : base(notFoundObjectName[..1].ToUpperInvariant() + $"{notFoundObjectName[1..]} is not found error")
    {
    }
}