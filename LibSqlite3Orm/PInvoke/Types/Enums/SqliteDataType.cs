using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LibSqlite3Orm.PInvoke.Types.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum SqliteDataType
{
    Integer = 1,
    Float = 2,
    Text = 3,
    Blob = 4,
    Null = 5
}