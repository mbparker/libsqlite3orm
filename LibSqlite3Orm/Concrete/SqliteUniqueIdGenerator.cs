using LibSqlite3Orm.Abstract;

namespace LibSqlite3Orm.Concrete;

public class SqliteUniqueIdGenerator : ISqliteUniqueIdGenerator
{
    public string NewUniqueId()
    {
        return Guid.NewGuid().ToString("N");
    }
}