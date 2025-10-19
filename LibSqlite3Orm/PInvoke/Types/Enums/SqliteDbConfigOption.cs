using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LibSqlite3Orm.PInvoke.Types.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum SqliteDbConfigOption
{
    EnableForeignKeys = 1002
}