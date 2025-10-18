using System.Linq.Expressions;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Concrete.Orm;

public class SqliteObjectRelationalMapper<TContext> : ISqliteObjectRelationalMapper<TContext> 
    where TContext : ISqliteOrmDatabaseContext
{
    private readonly Func<TContext> contextFactory;
    private readonly Func<ISqliteOrmDatabaseContext, IEntityServices> entityServicesFactory;
    private TContext _context;
    private IEntityServices _entityServices;
    private ISqliteTransaction _transaction;
    private ISqliteConnection _connection;

    public SqliteObjectRelationalMapper(Func<TContext> contextFactory,
        Func<ISqliteOrmDatabaseContext, IEntityServices> entityServicesFactory)
    {
        this.contextFactory = contextFactory;
        this.entityServicesFactory = entityServicesFactory;
    }
    
    private ISqliteConnection Connection {
        get
        {
            if (_connection == null)
                throw new InvalidOperationException("Connection not set.");
            return _connection;
        }
        set => _connection = value;
    }

    public TContext Context
    {
        get
        {
            if (_context is null)
                _context = contextFactory();
            return _context;
        }
    }
    
    private IEntityServices EntityServices
    {
        get
        {
            if (_entityServices is null)
                _entityServices = entityServicesFactory(Context);
            return _entityServices;
        }
    }

    public void UseConnection(ISqliteConnection connection)
    {
        Connection = connection.GetReference();
    }

    public virtual void Dispose()
    {
        if (Connection is not null)
        {
            if (_transaction is not null)
            {
                _transaction?.Dispose();
                _transaction = null;
            }
            
            Connection = null;
        }
    }
    
    public ISqliteCommand CreateSqlCommand() => Connection.CreateCommand();

    public void BeginTransaction()
    {
        if (_transaction is not null) throw new InvalidOperationException("The global transaction has already been started.");
        _transaction = Connection.BeginTransaction();
    }
    
    public void CommitTransaction()
    {
        if (_transaction is null) throw new InvalidOperationException("The global transaction has not been started.");
        try
        {
            _transaction.Commit();
        }
        finally
        {
            try
            {
                _transaction.Dispose();
            }
            finally
            {
                _transaction = null;
            }
        }
    }
    
    public void RollbackTransaction()
    {
        if (_transaction is null) throw new InvalidOperationException("The global transaction has not been started.");
        try
        {
            _transaction.Rollback();
        }
        finally
        {
            try
            {
                _transaction.Dispose();
            }
            finally
            {
                _transaction = null;
            }
        }
    }    

    public int ExecuteNonQuery(string sql, Action<ISqliteParameterCollectionAddTo> populateParamsAction = null)
    {
        using (var command = CreateSqlCommand())
        {
            populateParamsAction?.Invoke(command.Parameters);
            return command.ExecuteNonQuery(sql);
        }
    }

    public ISqliteDataReader ExecuteQuery(string sql, Action<ISqliteParameterCollectionAddTo> populateParamsAction = null)
    {
        using (var command = CreateSqlCommand())
        {
            populateParamsAction?.Invoke(command.Parameters);
            return command.ExecuteQuery(sql);
        }
    }
    
    public bool Insert<T>(T entity)
    {
        return EntityServices.Insert(Connection, entity);
    }
    
    public int InsertMany<T>(IEnumerable<T> entities)
    {
        return EntityServices.InsertMany(Connection, entities);
    }
    
    public bool Update<T>(T entity)
    {
        return EntityServices.Update(Connection, entity); 
    }
    
    public int UpdateMany<T>(IEnumerable<T> entities)
    {
        return EntityServices.UpdateMany(Connection, entities);
    }
    
    public UpsertResult Upsert<T>(T entity)
    {
        return EntityServices.Upsert(Connection, entity);
    }
    
    public UpsertManyResult UpsertMany<T>(IEnumerable<T> entities)
    {
        return EntityServices.UpsertMany(Connection, entities);
    }

    public ISqliteQueryable<T> Get<T>(bool loadNavigationProps = false) where T : new()
    {
        return EntityServices.Get<T>(Connection, loadNavigationProps);
    }
    
    public int Delete<T>(Expression<Func<T, bool>> predicate)
    {
        return EntityServices.Delete(Connection, predicate);
    }

    public int DeleteAll<T>()
    {
        return EntityServices.DeleteAll<T>(Connection);
    }
}