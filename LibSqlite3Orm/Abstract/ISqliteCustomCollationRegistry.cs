using LibSqlite3Orm.Models;
using LibSqlite3Orm.PInvoke.Types.Enums;
using LibSqlite3Orm.Types;

namespace LibSqlite3Orm.Abstract;

public interface ISqliteCustomCollationRegistry
{
    int RegisterCustomCollation(string name, SqliteCustomCollation collation, SqliteTextEncoding encoding = SqliteTextEncoding.Utf8);
    SqliteCustomCollation GetCollation(int id);
    int GetCollationId(string name);
    CollationItem[] GetAllRegistrations();
}