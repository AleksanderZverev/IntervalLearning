using FluentResults;

namespace DomainServices.DB.Repositories;


public interface IRepository<TEntity>
{
    IQueryable<TEntity> Query();
    
    TEntity Add(TEntity entity);
    Result<TEntity> AddAndSave(TEntity entity);
    Result<IList<TEntity>> AddRange(IList<TEntity> entities);
    
    TEntity Update(TEntity entity);
    Result<TEntity> UpdateAndSave(TEntity entity);
    IList<TEntity> UpdateRange(IList<TEntity> entities);

    TEntity Delete(TEntity entity);
    Result<TEntity> DeleteAndSave(TEntity entity);
    IList<TEntity> DeleteRange(IList<TEntity> entity);
}

public interface IRepository<TEntity, TId, in TIdParams> : IRepository<TEntity>
{
    Result<TId> GetUniqueId(TIdParams param);
}