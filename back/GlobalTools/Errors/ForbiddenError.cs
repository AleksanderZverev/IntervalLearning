using FluentResults;

namespace Infrastructure.Errors;

public class ForbiddenError : Error
{
    public ForbiddenError() : base("Forbidden")
    {
    }
    
    public ForbiddenError(string forbiddenAccessObjectName) 
        : base("Access for " + forbiddenAccessObjectName + " is forbidden")
    {
    }
}