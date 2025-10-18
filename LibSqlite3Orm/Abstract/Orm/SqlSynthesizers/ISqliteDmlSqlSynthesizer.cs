using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Abstract.Orm.SqlSynthesizers;

public interface ISqliteDmlSqlSynthesizer
{
    DmlSqlSynthesisResult Synthesize<TEntity>(SqliteDmlSqlSynthesisArgs args);
    DmlSqlSynthesisResult Synthesize(Type entityType, SqliteDmlSqlSynthesisArgs args);
}