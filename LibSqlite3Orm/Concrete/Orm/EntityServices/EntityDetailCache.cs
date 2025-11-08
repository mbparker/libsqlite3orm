using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Concrete.Orm.EntityServices;

public class EntityDetailCache : IEntityDetailCache
{
    private readonly Lock lockObj = new();
    private readonly Dictionary<string, EntityCacheItem> dictionary = new(StringComparer.OrdinalIgnoreCase);
    private readonly IOrmGenerativeLogicTracer tracer;
    private readonly ISqliteOrmDatabaseContext context;

    public EntityDetailCache(IOrmGenerativeLogicTracer tracer, ISqliteOrmDatabaseContext context)
    {
        this.tracer = tracer;
        this.context = context;
    }

    public object TryGet(object masterEntity, SqliteDbSchemaTableForeignKeyNavigationProperty navProp)
    {
        var newEntry = BuildCacheEntry(masterEntity, navProp);
        if (newEntry is null) return null;
        lock (lockObj)
        {
            var result = dictionary.GetValueOrDefault(newEntry.CacheKey);
            tracer.NotifyCachedGetAttempt(result is not null, masterEntity, navProp, result?.DetailEntity, newEntry.CacheKey);   
            return result?.DetailEntity;
        }
    }

    public void Upsert(object masterEntity, object detailEntity, SqliteDbSchemaTableForeignKeyNavigationProperty navProp)
    {
        var newEntry = BuildCacheEntry(masterEntity, navProp);
        if (newEntry is null) return;
        lock (lockObj)
        {
            if (!dictionary.ContainsKey(newEntry.CacheKey))
            {
                newEntry.DetailEntity = detailEntity;
                dictionary.Add(newEntry.CacheKey, newEntry);
            }
            else
                dictionary[newEntry.CacheKey].DetailEntity = detailEntity;
        }
    }

    public void Remove(object detailEntity)
    {
        lock (lockObj)
        {
            var newEntry = BuildCacheEntry(detailEntity);
            if (newEntry is null) return;
            dictionary.Remove(newEntry.CacheKey);
        }
    }
    
    public void Remove<T>(Expression<Func<T, bool>> predicate)
    {
        var pf = predicate?.Compile();
        lock (lockObj)
        {
            if (pf is null)
            {
                var keys = dictionary.Where(x => x.Value.DetailEntity is T).Select(x => x.Key).ToArray();
                foreach (var k in keys)
                    dictionary.Remove(k);
                return;
            }
            var key = dictionary.SingleOrDefault(x => x.Value.DetailEntity is T value && pf.Invoke(value)).Key;
            if (key is not null)
            {
                dictionary.Remove(key);
            }
        }
    }

    public void Clear()
    {
        lock (lockObj)
            dictionary.Clear();
    }
    
    private EntityCacheItem BuildCacheEntry(object masterEntity, SqliteDbSchemaTableForeignKeyNavigationProperty navProp)
    {
        var table = context.Schema.Tables.GetValueOrDefault(navProp.PropertyEntityTableName);
        if (table is null) return null;
        var fk = table.ForeignKeys.Single(x => x.Id == navProp.ForeignKeyId);
        var detailEntityType = Type.GetType(navProp.ReferencedEntityTypeName);
        if (detailEntityType is null) return null;
        var result = new EntityCacheItem();
        result.DetailType = detailEntityType;
        var entityType = masterEntity.GetType();
        foreach (var kf in fk.KeyFields)
        {
            var memb = entityType
                .GetMember(kf.TableModelProperty, BindingFlags.Instance | BindingFlags.Public)
                .Single();
            var membVal = memb.GetValue(masterEntity);
            if (membVal is not null)
                result.IdentityValues.Add(kf.ForeignTableModelProperty, membVal);
            else
                return null;
        }
        result.CacheKey = result.ToString();
        return result;
    }

    private EntityCacheItem BuildCacheEntry(object detailEntity)
    {
        var type = detailEntity.GetType();
        var item = dictionary.Values.FirstOrDefault(x => x.DetailType == type);
        if (item is null) return null;
        var newItem = new EntityCacheItem();
        newItem.DetailType = type;
        foreach (var iv in item.IdentityValues)
        {
            var memb = type
                .GetMember(iv.Key, BindingFlags.Instance | BindingFlags.Public)
                .Single();
            var membVal = memb.GetValue(detailEntity);
            if (membVal is not null)
                newItem.IdentityValues.Add(iv.Key, membVal);
            else
                return null;
        }
        newItem.CacheKey = item.ToString();
        return newItem;
    }

    private class EntityCacheItem
    {
        internal Dictionary<string, object> IdentityValues { get; } = new(StringComparer.OrdinalIgnoreCase);
        internal Type DetailType { get; set; }
        internal string CacheKey { get; set; }
        internal object DetailEntity { get; set; }

        public override string ToString()
        {
            var cacheKeyBuilder = new StringBuilder();
            cacheKeyBuilder.Append(DetailType.AssemblyQualifiedName);
            cacheKeyBuilder.Append(':');
            foreach (var iv in IdentityValues)
            {
                cacheKeyBuilder.Append(iv.Key);
                cacheKeyBuilder.Append('=');
                cacheKeyBuilder.Append($"{iv.Value};");
            }

            return cacheKeyBuilder.ToString();
        }
    }
}