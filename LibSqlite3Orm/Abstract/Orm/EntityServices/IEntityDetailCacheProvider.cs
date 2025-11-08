namespace LibSqlite3Orm.Abstract.Orm.EntityServices;

public interface IEntityDetailCacheProvider
{
    bool DisableCaching { get; set; }
    
    IEntityDetailCache GetCache(ISqliteOrmDatabaseContext context, ISqliteConnection connection);
    void ClearAllCaches();
}