using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Abstract.Orm;

public interface ISqliteEntityPostInsertPrimaryKeySetter
{
    void SetAutoIncrementedPrimaryKeyOnEntityIfNeeded<T>(SqliteDbSchema schema, ISqliteConnection connection, T entity);
}