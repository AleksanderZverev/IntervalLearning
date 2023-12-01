using FluentResults;

namespace Infrastructure.Errors;

public class ValidationError : Error 
{
    public ValidationError() : base("Validation error")
    {
    }
    
    public ValidationError(string notValidProperty) : base(notValidProperty[..1].ToUpperInvariant() + $"{notValidProperty[1..]} property is not valid")
    {
    }
}