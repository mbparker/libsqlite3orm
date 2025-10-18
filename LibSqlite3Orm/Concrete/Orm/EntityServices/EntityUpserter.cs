using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Abstract.Orm.SqlSynthesizers;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.Concrete.Orm.EntityServices;

public class EntityUpserter : IEntityUpserter
{
    private readonly ISqliteDmlSqlSynthesizer insertSqlSynthesizer;
    private readonly ISqliteDmlSqlSynthesizer updateSqlSynthesizer;
    private readonly IEntityCreator entityCreator;
    private readonly IEntityUpdater entityUpdater;
    private readonly ISqliteOrmDatabaseContext context;

    public EntityUpserter(
        Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer> dmlSqlSynthesizerFactory,
        Func<ISqliteOrmDatabaseContext, IEntityCreator> entityCreatorFactory,
        Func<ISqliteOrmDatabaseContext, IEntityUpdater> entityUpdaterFactory,
        ISqliteOrmDatabaseContext context)
    {
        this.context = context;
        insertSqlSynthesizer = dmlSqlSynthesizerFactory(SqliteDmlSqlSynthesisKind.Insert, this.context.Schema);
        updateSqlSynthesizer = dmlSqlSynthesizerFactory(SqliteDmlSqlSynthesisKind.Update, this.context.Schema);
        entityCreator = entityCreatorFactory(this.context);
        entityUpdater = entityUpdaterFactory(this.context);
    }
    
    public UpsertResult Upsert<T>(ISqliteConnection connection, T entity)
    {
        if (entityUpdater.Update(connection, entity))
            return UpsertResult.Updated;
        if (entityCreator.Insert(connection, entity))
            return UpsertResult.Inserted;
        return UpsertResult.Failed;
    }
    
    public UpsertManyResult UpsertMany<T>(ISqliteConnection connection, IEnumerable<T> entities)
    { 
        var updateCount = 0;
        var insertCount = 0;
        var failedCount = 0;
        var updateSynthesisResult = updateSqlSynthesizer.Synthesize<T>(SqliteDmlSqlSynthesisArgs.Empty);        
        var insertSynthesisResult = insertSqlSynthesizer.Synthesize<T>(SqliteDmlSqlSynthesisArgs.Empty);                
        using (var transaction = connection.BeginTransaction())
        {
            try
            {
                foreach (var entity in entities)
                {
                    if (entityUpdater.Update(connection, updateSynthesisResult, entity))
                        updateCount++;
                    else if (entityCreator.Insert(connection, insertSynthesisResult, entity))
                        insertCount++;
                    else
                        failedCount++;
                }

                transaction.Commit();
                return new UpsertManyResult(updateCount, insertCount, failedCount);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                transaction.Rollback();
                throw;
            }
        }
    }
}