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

    public virtual TEntity Add(TEntity entity)
    {
        return db.Add(entity).Entity;
    }

    public virtual TEntity Update(TEntity entity)
    {
        return db.Update(entity).Entity;
    }

    public virtual TEntity Delete(TEntity entity)
    {
        return db.Remove(entity).Entity;
    }

    public Result<TEntity> AddAndSave(TEntity entity)
    {
        Add(entity);
        return db.SoftSaveChanges()
            ? entity
            : new InternalError();
    }

    public Result<IList<TEntity>> AddRange(IList<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            Add(entity);
        }
        
        return db.SoftSaveChanges()
            ? entities.ToResult()
            : new InternalError();
    }

    public Result<TEntity> UpdateAndSave(TEntity entity)
    {
        Update(entity);
        return db.SoftSaveChanges()
            ? entity
            : new InternalError();
    }

    public IList<TEntity> UpdateRange(IList<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            Update(entity);
        }
        
        return entities;
    }

    public Result<TEntity> DeleteAndSave(TEntity entity)
    {
        Delete(entity);
        return db.SoftSaveChanges()
            ? entity
            : new InternalError();
    }

    public IList<TEntity> DeleteRange(IList<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            Delete(entity);
        }

        return entities;
    }
}