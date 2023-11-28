using FluentResults;

namespace Application.Common.Interfaces.DB.Repositories;


public interface IRepository<TEntity>
{
    IQueryable<TEntity> Query();

    Result<TEntity> Add(TEntity entity);
    Result<IList<TEntity>> AddRange(IList<TEntity> entities);
    
    Result<TEntity> Update(TEntity entity);
    Result<IList<TEntity>> UpdateRange(IList<TEntity> entities);
    
    Result<TEntity> Delete(TEntity entity);
    Result<IList<TEntity>> DeleteRange(IList<TEntity> entity);
}

public interface IRepository<TEntity, TId, in TIdParams> : IRepository<TEntity>
{
    Result<TId> GetUniqueId(TIdParams param);
}