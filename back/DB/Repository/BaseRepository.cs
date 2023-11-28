using Application.Common.Interfaces.DB;
using Application.Common.Interfaces.DB.Repositories;
using FluentResults;
using Infrastructure.Errors;

namespace DB.Repository;

public class BaseRepository<TEntity> : IRepository<TEntity>
    where TEntity : class 
{
    protected readonly ApplicationContext db;

    public BaseRepository(ApplicationContext db)
    {
        this.db = db;
    }

    public IQueryable<TEntity> Query()
    {
        return db.Set<TEntity>().AsQueryable();
    }

    protected virtual void MarkAdded(TEntity entity)
    {
        db.Add(entity);
    }

    protected virtual void MarkUpdated(TEntity entity)
    {
        db.Update(entity);
    }

    protected virtual void MarkRemoved(TEntity entity)
    {
        db.Remove(entity);
    }

    public Result<TEntity> Add(TEntity entity)
    {
        MarkAdded(entity);
        return db.SoftSaveChanges()
            ? entity
            : new InternalError();
    }

    public Result<IList<TEntity>> AddRange(IList<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            MarkAdded(entity);
        }
        
        return db.SoftSaveChanges()
            ? entities.ToResult()
            : new InternalError();
    }

    public Result<TEntity> Update(TEntity entity)
    {
        MarkUpdated(entity);
        return db.SoftSaveChanges()
            ? entity
            : new InternalError();
    }

    public Result<IList<TEntity>> UpdateRange(IList<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            MarkUpdated(entity);
        }
        
        return db.SoftSaveChanges()
            ? entities.ToResult()
            : new InternalError();
    }

    public Result<TEntity> Delete(TEntity entity)
    {
        MarkRemoved(entity);
        return db.SoftSaveChanges()
            ? entity
            : new InternalError();
    }

    public Result<IList<TEntity>> DeleteRange(IList<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            MarkRemoved(entity);
        }
        
        return db.SoftSaveChanges()
            ? entities.ToResult()
            : new InternalError();
    }
}