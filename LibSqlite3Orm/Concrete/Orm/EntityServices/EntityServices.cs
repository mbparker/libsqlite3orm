using System.Linq.Expressions;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Concrete.Orm.EntityServices;

public class EntityServices : IEntityServices
{
    private readonly IEntityCreator creator;
    private readonly IEntityGetter getter;
    private readonly IEntityUpdater updater;
    private readonly IEntityDeleter deleter;
    private readonly IEntityUpserter upserter;
    
    public EntityServices(Func<ISqliteOrmDatabaseContext, IEntityCreator> entityCreatorFactory,
        Func<ISqliteOrmDatabaseContext, IEntityUpdater> entityUpdaterFactory,
        Func<ISqliteOrmDatabaseContext, IEntityUpserter> entityUpserterFactory,
        Func<ISqliteOrmDatabaseContext, IEntityGetter> entityGetterFactory,
        Func<ISqliteOrmDatabaseContext, IEntityDeleter> entityDeleterFactory,
        ISqliteOrmDatabaseContext context)
    {
        creator = entityCreatorFactory(context);
        getter = entityGetterFactory(context);
        updater = entityUpdaterFactory(context);
        deleter = entityDeleterFactory(context);
        upserter = entityUpserterFactory(context);
    }
    
    public bool Insert<T>(ISqliteConnection connection, T entity)
    {
        return creator.Insert(connection, entity);
    }

    public bool Insert<T>(ISqliteConnection connection, DmlSqlSynthesisResult synthesisResult, T entity)
    {
        return creator.Insert(connection, synthesisResult, entity);
    }
    
    public int InsertMany<T>(ISqliteConnection connection, IEnumerable<T> entities)
    {
        return creator.InsertMany(connection, entities);
    }
    
    public ISqliteQueryable<T> Get<T>(ISqliteConnection connection, bool loadNavigationProps = false) where T : new()
    {
        return getter.Get<T>(connection, loadNavigationProps);
    }
    
    public bool Update<T>(ISqliteConnection connection, T entity)
    {
        return updater.Update(connection, entity);
    }

    public bool Update<T>(ISqliteConnection connection, DmlSqlSynthesisResult synthesisResult, T entity)
    {
        return updater.Update(connection, synthesisResult, entity);
    }
    
    public int UpdateMany<T>(ISqliteConnection connection, IEnumerable<T> entities)
    {
        return updater.UpdateMany(connection, entities);
    }
    
    public int Delete<T>(ISqliteConnection connection, Expression<Func<T, bool>> predicate)
    {
        return deleter.Delete(connection, predicate);
    }
    
    public int DeleteAll<T>(ISqliteConnection connection)
    {
        return deleter.DeleteAll<T>(connection);
    }
    
    public UpsertResult Upsert<T>(ISqliteConnection connection, T entity)
    {
        return upserter.Upsert(connection, entity);
    }
    
    public UpsertManyResult UpsertMany<T>(ISqliteConnection connection, IEnumerable<T> entities)
    {
        return upserter.UpsertMany(connection, entities);
    }    
}