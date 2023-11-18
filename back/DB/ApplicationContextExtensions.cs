
using Microsoft.EntityFrameworkCore;

namespace DB
{
    public static class ApplicationContextExtensions
    {
        public static T? UpdateByProperties<T>(
            this DbSet<T> set,
            Action<T> setProperties,
            params object[] keyValues)
            where T : class, new()
        {
            if (keyValues == null || keyValues.Length == 0)
                throw new ArgumentNullException("Unable to update entity");

            var entity = set.Find(keyValues);

            if (entity == null)
                return null;

            setProperties(entity);

            return entity;
        }

        public static T? UpdateByProperties<T>(
            this ApplicationContext db,
            object setProperties,
            params object[] keyValues)
            where T : class, new()
        {
            if (keyValues == null || keyValues.Length == 0)
                throw new ArgumentNullException("Unable to update entity");

            var set = db.Set<T>();
            var entity = set.Find(keyValues);

            if (entity == null)
                return null;

            var entry = db.Entry(entity);
            entry.CurrentValues.SetValues(setProperties);

            return entity;
        }

        public static T CreateByProperties<T>(
            this ApplicationContext db,
            object setProperties)
            where T : class, new()
        {
            var entity = new T();

            var entry = db.Entry(entity);
            entry.CurrentValues.SetValues(setProperties);

            entry.State = EntityState.Added;
            return entity;
        }

        public static bool SoftSaveChanges(this ApplicationContext db)
        {
            try
            {
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public static async Task<bool> SoftSaveChangesAsync(this ApplicationContext db)
        {
            try
            {
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
