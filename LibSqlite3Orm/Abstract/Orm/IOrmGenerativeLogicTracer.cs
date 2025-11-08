using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Models.Orm.Events;

namespace LibSqlite3Orm.Abstract.Orm;

public interface IOrmGenerativeLogicTracer
{
    event EventHandler<SqlStatementExecutingEventArgs> SqlStatementExecuting;
    event EventHandler<GenerativeLogicTraceEventArgs> WhereClauseBuilderVisit;
    event EventHandler<CacheAccessAttemptEventArgs> CachedGetAttempt;
    
    void NotifySqlStatementExecuting(string sqlStatement, ISqliteParameterCollectionDebug parameters);
    void NotifyWhereClauseBuilderVisit(Lazy<string> message);
    void NotifyCachedGetAttempt(bool isHit, object masterEntity, SqliteDbSchemaTableForeignKeyNavigationProperty navProp, object detailEntity, string cacheKey);
}