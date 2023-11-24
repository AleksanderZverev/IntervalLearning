using FluentResults;

namespace Application.Common.Interfaces.DB;

public interface IMutationResolver<TEntity>
{
    Result<TEntity> Add(TEntity entity);
    Result<TEntity> Update(TEntity entity);
    Result<TEntity> Delete(TEntity entity);
}