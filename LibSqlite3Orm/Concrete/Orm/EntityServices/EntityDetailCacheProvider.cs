using System.Linq.Expressions;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Concrete.Orm.EntityServices;

public class EntityDetailCacheProvider : IEntityDetailCacheProvider, IEntityDetailCache
{
    private readonly Lock lockObj = new();
    private readonly Func<ISqliteOrmDatabaseContext, IEntityDetailCache> entityCacheFactory;
    private readonly Dictionary<long, Dictionary<Type, IEntityDetailCache>>  entityCaches = new();

    public EntityDetailCacheProvider(Func<ISqliteOrmDatabaseContext, IEntityDetailCache> entityCacheFactory)
    {
        this.entityCacheFactory = entityCacheFactory;
    }
    
    public bool DisableCaching { get; set; } = true;
    
    public IEntityDetailCache GetCache(ISqliteOrmDatabaseContext context, ISqliteConnection connection)
    {
        // This is a little weird. This class implements a dummy cache that does nothing. 
        // So when caching is disabled, it will just return itself.
        // Ultimately, it makes the calling code cleaner by keeping all the conditional logic in here.
        if (DisableCaching) return this;
        
        lock (lockObj)
        {
            IEntityDetailCache result;
            var connHandle = connection.GetHandle().ToInt64();
            var contextType = context.GetType();
            var cacheSet = entityCaches.GetValueOrDefault(connHandle);
            if (cacheSet is null)
            {
                connection.ConnectionClosed += ConnectionOnConnectionClosed;
                cacheSet = new Dictionary<Type, IEntityDetailCache>();
                entityCaches.Add(connHandle, cacheSet);
            }

            result = cacheSet.GetValueOrDefault(contextType);
            if (result is null)
            {
                result = entityCacheFactory(context);
                cacheSet.Add(contextType, result);
            }

            return result;
        }
    }

    public void ClearAllCaches()
    {
        lock (lockObj)
        {
            foreach (var cacheSet in entityCaches.Values)
            {
                foreach (var cache in cacheSet.Values)
                {
                    cache.Clear();
                }
                
                cacheSet.Clear();
            }
            
            entityCaches.Clear();
        }
    }

    private void ConnectionOnConnectionClosed(object sender, EventArgs e)
    {
        if (sender is ISqliteConnection connection)
        {
            connection.ConnectionClosed -= ConnectionOnConnectionClosed;
            var connHandle = connection.GetHandle().ToInt64();
            lock (lockObj)
            {
                var cacheSet = entityCaches.GetValueOrDefault(connHandle);
                if (cacheSet is null) return;
                foreach (var kp in cacheSet)
                    kp.Value.Clear();
                cacheSet.Clear();
                entityCaches.Remove(connHandle);
            }
        }
    }

    // Null implementation of the cache here
    object IEntityDetailCache.TryGet(object masterEntity, SqliteDbSchemaTableForeignKeyNavigationProperty navProp)
    {
        return null;
    }

    void IEntityDetailCache.Upsert(object masterEntity, object detailEntity, SqliteDbSchemaTableForeignKeyNavigationProperty navProp)
    {
    }

    void IEntityDetailCache.Remove(object detailEntity)
    {
    }

    void IEntityDetailCache.Remove<T>(Expression<Func<T, bool>> predicate)
    {
    }

    void IEntityDetailCache.Clear()
    {
    }
}