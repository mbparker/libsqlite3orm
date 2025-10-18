using System.Runtime.Serialization;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Abstract.Orm.SqlSynthesizers;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.Concrete.Orm.EntityServices;

public class EntityGetter : IEntityGetter
{
    private readonly Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer> dmlSqlSynthesizerFactory;
    private readonly ISqliteParameterPopulator  parameterPopulator;
    private readonly ISqliteEntityWriter entityWriter;
    private readonly ISqliteOrmDatabaseContext context;

    public EntityGetter(
        Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer> dmlSqlSynthesizerFactory,
        ISqliteParameterPopulator parameterPopulator,
        Func<ISqliteOrmDatabaseContext, ISqliteEntityWriter> entityWriterFactory, ISqliteOrmDatabaseContext context)
    {
        this.dmlSqlSynthesizerFactory = dmlSqlSynthesizerFactory;
        this.parameterPopulator = parameterPopulator;
        entityWriter = entityWriterFactory(context);
        this.context = context;
    }

    public ISqliteQueryable<T> Get<T>(ISqliteConnection connection, bool loadNavigationProps) where T : new()
    {
        var entityTypeName = typeof(T).AssemblyQualifiedName;
        var table = context.Schema.Tables.Values.SingleOrDefault(x => x.ModelTypeName == entityTypeName);
        if (table is not null)
        {
            ISqliteDataReader ExecuteQuery(SynthesizeSelectSqlArgs args)
            {
                var synthesizer = dmlSqlSynthesizerFactory(SqliteDmlSqlSynthesisKind.Select, context.Schema);
                var synthesisResult = synthesizer.Synthesize<T>(new SqliteDmlSqlSynthesisArgs(args));
                using (var cmd = connection.CreateCommand())
                {
                    parameterPopulator.Populate<T>(synthesisResult, cmd.Parameters);
                    return cmd.ExecuteQuery(synthesisResult.SqlText);
                }
            }

            T DeserializeRow(ISqliteDataRow row)
            {
                return entityWriter.Deserialize<T>(context.Schema, table, row, loadNavigationProps, connection);
            }
            
            return new SqliteOrderedQueryable<T>(context.Schema, ExecuteQuery, DeserializeRow, loadNavigationProps);
        }
        
        throw new InvalidDataContractException($"Type {entityTypeName} is not mapped in the schema.");
    }
}