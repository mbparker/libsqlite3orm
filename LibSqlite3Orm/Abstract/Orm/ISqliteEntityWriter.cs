using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Abstract.Orm;

public interface ISqliteEntityWriter
{
    TEntity Deserialize<TEntity>(SqliteDbSchema schema, SqliteDbSchemaTable table, ISqliteDataRow row,
        bool recursiveLoad, ISqliteConnection connection)
        where TEntity : new();
    TEntity Deserialize<TEntity>(SqliteDbSchema schema, SqliteDbSchemaTable table, ISqliteDataRow row)
        where TEntity : new();    
}