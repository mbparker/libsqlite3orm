using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Abstract.Orm;

public interface ISqliteDbFactory
{
    void Create(SqliteDbSchema schema, ISqliteConnection connection);
}