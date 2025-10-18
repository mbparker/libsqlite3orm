using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Abstract.Orm.SqlSynthesizers;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.Concrete.Orm.EntityServices;

public class EntityCreator : IEntityCreator
{
    private readonly Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer> dmlSqlSynthesizerFactory;
    private readonly ISqliteParameterPopulator  parameterPopulator;
    private readonly ISqliteEntityPostInsertPrimaryKeySetter primaryKeySetter;
    private readonly ISqliteOrmDatabaseContext context;

    public EntityCreator(
        Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer> dmlSqlSynthesizerFactory,
        ISqliteParameterPopulator parameterPopulator, ISqliteEntityPostInsertPrimaryKeySetter primaryKeySetter,
        ISqliteOrmDatabaseContext context)
    {
        this.dmlSqlSynthesizerFactory = dmlSqlSynthesizerFactory;
        this.parameterPopulator = parameterPopulator;
        this.primaryKeySetter =  primaryKeySetter;
        this.context = context;
    }
    
    public bool Insert<T>(ISqliteConnection connection, T entity)
    {
        var synthesisResult = SynthesizeSql<T>();
        return Insert(connection, synthesisResult, entity);
    }    

    public bool Insert<T>(ISqliteConnection connection, DmlSqlSynthesisResult synthesisResult, T entity)
    {
        using (var cmd = connection.CreateCommand())
        {
            parameterPopulator.Populate(synthesisResult, cmd.Parameters, entity);
            if (cmd.ExecuteNonQuery(synthesisResult.SqlText) == 1)
            {
                primaryKeySetter.SetAutoIncrementedPrimaryKeyOnEntityIfNeeded(context.Schema, connection, entity);
                return true;
            }

            return false;
        }
    }
    
    public int InsertMany<T>(ISqliteConnection connection, IEnumerable<T> entities)
    {
        var synthesisResult = SynthesizeSql<T>();
        using (var transaction = connection.BeginTransaction())
        {
            try
            {
                var cnt = 0;
                foreach (var entity in entities)
                {
                    if (Insert(connection, synthesisResult, entity))
                        cnt++;
                }

                transaction.Commit();
                return cnt;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                transaction.Rollback();
                throw;
            }
        }   
    }    

    private DmlSqlSynthesisResult SynthesizeSql<T>()
    {
        var synthesizer = dmlSqlSynthesizerFactory(SqliteDmlSqlSynthesisKind.Insert, context.Schema);
        return synthesizer.Synthesize<T>(SqliteDmlSqlSynthesisArgs.Empty);
    }
}