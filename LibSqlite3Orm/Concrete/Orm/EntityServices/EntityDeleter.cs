using System.Linq.Expressions;
using System.Runtime.Serialization;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Abstract.Orm.SqlSynthesizers;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.Concrete.Orm.EntityServices;

public class EntityDeleter : IEntityDeleter
{
    private readonly Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer> dmlSqlSynthesizerFactory;
    private readonly ISqliteParameterPopulator  parameterPopulator;
    private readonly IEntityDetailCacheProvider _entityDetailCacheProvider;
    private readonly ISqliteOrmDatabaseContext context;

    public EntityDeleter(
        Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer> dmlSqlSynthesizerFactory,
        ISqliteParameterPopulator parameterPopulator, IEntityDetailCacheProvider entityDetailCacheProvider,
        ISqliteOrmDatabaseContext context)
    {
        this.dmlSqlSynthesizerFactory = dmlSqlSynthesizerFactory;
        this.parameterPopulator = parameterPopulator;
        this._entityDetailCacheProvider = entityDetailCacheProvider;
        this.context = context;
    }
    
    public int Delete<T>(ISqliteConnection connection, Expression<Func<T, bool>> predicate)
    {
        if (predicate is null) throw new ArgumentNullException(nameof(predicate));
        return DeleteInternal(connection, predicate);  
    }
    
    public int DeleteAll<T>(ISqliteConnection connection)
    {
        return DeleteInternal<T>(connection, null);
    }
    
    private int DeleteInternal<T>(ISqliteConnection connection, Expression<Func<T, bool>> predicate)
    {
        var synthesizer = dmlSqlSynthesizerFactory(SqliteDmlSqlSynthesisKind.Delete, context.Schema);
        var synthesisResult =
            synthesizer.Synthesize<T>(new SqliteDmlSqlSynthesisArgs(new SynthesizeDeleteSqlArgs(predicate)));
        var type = typeof(T);
        var table = context.Schema.Tables.Values.SingleOrDefault(x => x.ModelTypeName == type.AssemblyQualifiedName);
        if (table is not null)
        {
            using (var cmd = connection.CreateCommand())
            {
                _entityDetailCacheProvider.GetCache(context, connection).Remove(predicate);
                parameterPopulator.Populate<T>(synthesisResult, cmd.Parameters);
                return cmd.ExecuteNonQuery(synthesisResult.SqlText);
            }
        }

        throw new InvalidDataContractException($"Type {type.AssemblyQualifiedName} is not mapped in the schema.");    
    }
}