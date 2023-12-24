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
        var addedEntity = Add(entity);
        return db.SoftSaveChanges()
            ? addedEntity
            : new InternalError();
    }

    public Result<IList<TEntity>> AddRange(IList<TEntity> entities)
    {
        var addedEntities = new List<TEntity>(entities.Count);
        
        foreach (var entity in entities)
        {
            addedEntities.Add(Add(entity));
        }
        
        return db.SoftSaveChanges()
            ? addedEntities
            : new InternalError();
    }

    public Result<TEntity> UpdateAndSave(TEntity entity)
    {
        var updatedEntity = Update(entity);
        
        return db.SoftSaveChanges()
            ? updatedEntity
            : new InternalError();
    }

    public IList<TEntity> UpdateRange(IList<TEntity> entities)
    {
        var updatedEntities = new List<TEntity>(entities.Count);
        
        foreach (var entity in entities)
        {
            updatedEntities.Add(Update(entity));
        }

        return updatedEntities;
    }

    public Result<TEntity> DeleteAndSave(TEntity entity)
    {
        var deletedEntity = Delete(entity);
        return db.SoftSaveChanges()
            ? deletedEntity
            : new InternalError();
    }

    public IList<TEntity> DeleteRange(IList<TEntity> entities)
    {
        var deletedEntities = new List<TEntity>(entities.Count);
        
        foreach (var entity in entities)
        {
            deletedEntities.Add(Delete(entity));
        }

        return deletedEntities;
    }
}