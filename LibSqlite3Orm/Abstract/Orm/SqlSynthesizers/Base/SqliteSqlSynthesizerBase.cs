using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Abstract.Orm.SqlSynthesizers.Base;

public class SqliteSqlSynthesizerBase
{
    protected SqliteSqlSynthesizerBase(SqliteDbSchema schema)
    {
        Schema = schema;
    }
    
    protected SqliteDbSchema Schema { get; }
}