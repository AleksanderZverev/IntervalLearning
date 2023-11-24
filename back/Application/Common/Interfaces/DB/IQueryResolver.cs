namespace Application.Common.Interfaces.DB;

public interface IQueryResolver<out TEntity>
{
    IQueryable<TEntity> Query();
}