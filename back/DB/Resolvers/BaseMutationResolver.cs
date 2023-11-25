using Application.Common.Interfaces.DB;
using FluentResults;
using Infrastructure.Errors;

namespace DB.Resolvers;

public abstract class BaseMutationResolver<TEntity> : IMutationResolver<TEntity>
{
    protected readonly ApplicationContext db;

    protected BaseMutationResolver(ApplicationContext db)
    {
        this.db = db;
    }

    protected abstract void MarkAdded(TEntity entity);
    protected abstract void MarkUpdated(TEntity entity);
    protected abstract void MarkRemoved(TEntity entity);

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