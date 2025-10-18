using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Abstract.Orm;

public interface ISqliteOrmDatabaseContext
{
    SqliteDbSchema Schema { get; }
}