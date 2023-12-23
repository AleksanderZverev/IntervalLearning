using FluentResults;

namespace Infrastructure.Errors;

public class ValidationError : Error 
{
    public ValidationError() : base("Validation error")
    {
    }
    
    public ValidationError(string message) : base(message)
    {
    }
}