using FluentResults;

namespace Application.Common.Interfaces.DB;

public interface IMutationResolver<TEntity>
{
    Result<TEntity> Add(TEntity entity);
    Result<IList<TEntity>> AddRange(IList<TEntity> entities);
    Result<TEntity> Update(TEntity entity);
    Result<IList<TEntity>> UpdateRange(IList<TEntity> entities);
    Result<TEntity> Delete(TEntity entity);
    Result<IList<TEntity>> DeleteRange(IList<TEntity> entity);
}