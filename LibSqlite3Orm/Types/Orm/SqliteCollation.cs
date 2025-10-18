using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LibSqlite3Orm.Types.Orm;

[JsonConverter(typeof(StringEnumConverter))]
public enum SqliteCollation
{
    Binary,
    AsciiLowercase,
    RightTrimmed
}