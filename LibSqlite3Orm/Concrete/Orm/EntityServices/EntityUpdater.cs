using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Abstract.Orm.SqlSynthesizers;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.Concrete.Orm.EntityServices;

public class EntityUpdater : IEntityUpdater
{
    private readonly Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer> dmlSqlSynthesizerFactory;
    private readonly ISqliteParameterPopulator  parameterPopulator;
    private readonly ISqliteOrmDatabaseContext context;

    public EntityUpdater(
        Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer> dmlSqlSynthesizerFactory,
        ISqliteParameterPopulator  parameterPopulator, ISqliteOrmDatabaseContext context)
    {
        this.dmlSqlSynthesizerFactory = dmlSqlSynthesizerFactory;
        this.parameterPopulator = parameterPopulator;
        this.context = context;
    }
    
    public bool Update<T>(ISqliteConnection connection, T entity)
    {
        var synthesisResult = SynthesizeSql<T>();
        return Update(connection, synthesisResult, entity);
    }

    public bool Update<T>(ISqliteConnection connection, DmlSqlSynthesisResult synthesisResult, T entity)
    {
        using (var cmd = connection.CreateCommand())
        {
            parameterPopulator.Populate(synthesisResult, cmd.Parameters, entity);
            return cmd.ExecuteNonQuery(synthesisResult.SqlText) == 1;
        }
    }
    
    public int UpdateMany<T>(ISqliteConnection connection, IEnumerable<T> entities)
    {
        var synthesisResult = SynthesizeSql<T>();
        using (var transaction = connection.BeginTransaction())
        {
            try
            {
                var cnt = 0;
                foreach (var entity in entities)
                {
                    if (Update(connection, synthesisResult, entity))
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
        var synthesizer = dmlSqlSynthesizerFactory(SqliteDmlSqlSynthesisKind.Update, context.Schema);
        return synthesizer.Synthesize<T>(SqliteDmlSqlSynthesisArgs.Empty);
    }
}