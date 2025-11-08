namespace LibSqlite3Orm.Models.Orm.Events;

public class CacheAccessAttemptEventArgs : GenerativeLogicTraceEventArgs
{
    public CacheAccessAttemptEventArgs(bool isHit, object masterEntity,
        SqliteDbSchemaTableForeignKeyNavigationProperty navProp, object detailEntity, string cacheKey)
        : base(BuildMessage(isHit, cacheKey))
    {
        IsHit = isHit;
        MasterEntity = masterEntity;
        NavProp = navProp;
        DetailEntity = detailEntity;
        CacheKey = cacheKey;
    }

    public bool IsHit { get; }
    public object MasterEntity { get; }
    public SqliteDbSchemaTableForeignKeyNavigationProperty NavProp { get; }
    public object DetailEntity { get; }
    public string CacheKey { get; }

    private static Lazy<string> BuildMessage(bool isHit, string cacheKey)
    {
        return new Lazy<string>(() => "[Entity Cache Access] " + (isHit ? "CACHE HIT" :  "CACHE MISS") + $" {cacheKey}");
    }
}