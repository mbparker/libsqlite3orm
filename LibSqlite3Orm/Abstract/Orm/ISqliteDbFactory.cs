using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Abstract.Orm;

public interface ISqliteDbFactory
{
    bool IsDatabaseAlreadyInitialized(ISqliteConnection connection);
    void Create(SqliteDbSchema schema, ISqliteConnection connection);
}