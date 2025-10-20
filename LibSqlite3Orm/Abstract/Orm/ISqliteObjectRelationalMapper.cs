using System.Linq.Expressions;
using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Abstract.Orm;

public interface ISqliteObjectRelationalMapper<TContext> : IDisposable where TContext : ISqliteOrmDatabaseContext
{
    TContext Context { get; }
    
    void UseConnection(ISqliteConnection connection);

    ISqliteCommand CreateSqlCommand();

    void BeginTransaction();
    void CommitTransaction();
    void RollbackTransaction();

    int ExecuteNonQuery(string sql, Action<ISqliteParameterCollectionAddTo> populateParamsAction = null);
    ISqliteDataReader ExecuteQuery(string sql, Action<ISqliteParameterCollectionAddTo> populateParamsAction = null);
    
    bool Insert<T>(T entity);
    int InsertMany<T>(IEnumerable<T> entities);
    bool Update<T>(T entity);
    int UpdateMany<T>(IEnumerable<T> entities);
    UpsertResult Upsert<T>(T entity);
    UpsertManyResult UpsertMany<T>(IEnumerable<T> entities);
    ISqliteQueryable<T> Get<T>(bool recursiveLoad = false) where T : new();
    int Delete<T>(Expression<Func<T, bool>> predicate);
    int DeleteAll<T>();
}