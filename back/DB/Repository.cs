using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace DB;

public class Repository<T> where T : class, new()
{
    private readonly ApplicationContext db;

    public Repository(ApplicationContext db)
    {
        this.db = db;
    }

    public T? Find(object[] keyValues)
        => db.Set<T>().Find(keyValues);

    public ValueTask<T?> FindAsync(object[] keyValues)
        => db.Set<T>().FindAsync(keyValues);

    public Task<T> Create(object setProperties)
        => CreateOrUpdate(setProperties, null);

    public Task<T> Update(object setProperties, object[] keyValues)
    {
        if (keyValues.Length == 0)
            Debug.Fail("keyValues.Length == 0");

        return CreateOrUpdate(setProperties, keyValues);
    }

    private async Task<T> CreateOrUpdate(object setProperties, params object[]? keyValues)
    {
        var set = db.Set<T>();

        var entity = keyValues == null || keyValues.Length == 0
            ? new T()
            : await set.FindAsync(keyValues).ConfigureAwait(false);

        if (entity == null)
            return null;

        var entry = db.Entry(entity);
        entry.CurrentValues.SetValues(setProperties);

        if (keyValues == null || keyValues.Length == 0)
            entry.State = EntityState.Added;
        
        return entity;

        //try
        //{
        //    await db.SaveChangesAsync().ConfigureAwait(false);
        //    return entity;
        //}
        //catch
        //{
        //    return null;
        //}
    }

    public async Task<T?> Delete(object[] keyValues)
    {
        var set = db.Set<T>();

        var entity = await set.FindAsync(keyValues).ConfigureAwait(false);

        if (entity == null)
        {
            return null;
        }

        db.Entry(entity).State = EntityState.Deleted;

        try
        {
            await db.SaveChangesAsync();
            return entity;
        }
        catch
        {
            return null;
        }
    }

    public async Task<T?> UpdatePropertyAsync(object[] keyValues, Action<T?> changeProperty)
    {
        var set = db.Set<T>();

        var entity = await set.FindAsync(keyValues).ConfigureAwait(false);

        if (entity == null)
            return null;

        changeProperty(entity);

        await db.SaveChangesAsync().ConfigureAwait(false);
        return entity;
    }
}