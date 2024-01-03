using FluentResults;

namespace GlobalTools.Errors;

public class ConflictError : Error
{
    public ConflictError() : base("Conflict error")
    {
    }
    
    public ConflictError(string conflictObjectName) : base(
        "The same " +
        conflictObjectName[..1].ToUpperInvariant() + conflictObjectName[1..] +
        " is already existing")
    {
    }
}