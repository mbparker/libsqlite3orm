using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Models;
using LibSqlite3Orm.PInvoke.Types.Enums;
using LibSqlite3Orm.Types;

namespace LibSqlite3Orm.Concrete;

public class SqliteCustomCollationRegistry : ISqliteCustomCollationRegistry
{
    private readonly Lock lockObj = new();
    private int idCounter;
    private readonly Dictionary<string, CollationItem> collationByName = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<int, CollationItem> collationById = new();
    
    public int RegisterCustomCollation(string name, SqliteCustomCollation collation, SqliteTextEncoding encoding = SqliteTextEncoding.Utf8)
    {
        lock (lockObj)
        {
            if (collationByName.ContainsKey(name)) return 0;
            var item = new CollationItem(name, ++idCounter, collation, encoding);
            collationByName.Add(name, item);
            collationById.Add(item.Id, item);
            return item.Id;
        }
    }

    public SqliteCustomCollation GetCollation(int id)
    {
        lock(lockObj)
            return collationById.GetValueOrDefault(id)?.CollationFunc;
    }

    public int GetCollationId(string name)
    {
        lock(lockObj)
            return collationByName.GetValueOrDefault(name)?.Id ?? 0;
    }

    public CollationItem[] GetAllRegistrations()
    {
        lock(lockObj)
            return collationByName.Values.ToArray();
    }
}