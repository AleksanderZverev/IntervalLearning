using FluentResults;

namespace Application.Common.Interfaces.DB.Repositories;

public interface IBoundedContextQueryRepository
{
}

public interface IBoundedContextRepository
{
    Result SaveChanges();
    Task<Result> SaveChangesAsync();
}