using FluentResults;

namespace DomainServices.DB.Repositories;

public interface IBoundedContextQueryRepository
{
}

public interface IBoundedContextRepository
{
    Result SaveChanges();
    Task<Result> SaveChangesAsync();
}