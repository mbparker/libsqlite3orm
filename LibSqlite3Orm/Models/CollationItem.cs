using LibSqlite3Orm.PInvoke.Types.Enums;
using LibSqlite3Orm.Types;

namespace LibSqlite3Orm.Models;

public class CollationItem
{
    public CollationItem(string name, int id, SqliteCustomCollation collation, SqliteTextEncoding encoding)
    {
        Name = name;
        Id = id;
        CollationFunc = collation;
        Encoding = encoding;
    }

    public string Name { get; }
    public int Id { get; }
    public SqliteCustomCollation CollationFunc { get; } 
    public SqliteTextEncoding Encoding { get; }
}