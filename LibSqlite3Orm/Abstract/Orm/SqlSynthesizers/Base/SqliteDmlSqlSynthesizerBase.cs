using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Abstract.Orm.SqlSynthesizers.Base;

public abstract class SqliteDmlSqlSynthesizerBase : SqliteSqlSynthesizerBase, ISqliteDmlSqlSynthesizer 
{
    protected SqliteDmlSqlSynthesizerBase(SqliteDbSchema schema) 
        : base(schema)
    {
    }

    public DmlSqlSynthesisResult Synthesize<TEntity>(SqliteDmlSqlSynthesisArgs args)
    {
        return Synthesize(typeof(TEntity), args);
    }

    public abstract DmlSqlSynthesisResult Synthesize(Type entityType, SqliteDmlSqlSynthesisArgs args);
}