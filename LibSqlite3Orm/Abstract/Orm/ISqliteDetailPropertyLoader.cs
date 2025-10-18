using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Abstract.Orm;

public interface ISqliteDetailPropertyLoader
{
    void LoadDetailProperties<TEntity>(TEntity entity, SqliteDbSchemaTable table, ISqliteDataRow row,
        bool loadNavigationProps, ISqliteConnection connection) where TEntity : new();
}