using System.Linq.Expressions;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Models.Orm.OData;

namespace LibSqlite3Orm.Abstract.Orm;

public interface ISqliteObjectRelationalMapper<TContext> : IDisposable where TContext : ISqliteOrmDatabaseContext
{
    TContext Context { get; }
    ISqliteConnection Connection { get; }
    bool DisableCaching { get; set; }
    
    void UseConnection(ISqliteConnection connection);
    void CreateConnection(Func<ISqliteConnection> connectionFactory);
    void ReleaseConnection();

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
    
    ODataQueryResult<TEntity> ODataQuery<TEntity>(string odataQuery) where TEntity : new();
}