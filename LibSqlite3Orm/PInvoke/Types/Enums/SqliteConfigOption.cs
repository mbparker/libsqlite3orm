using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LibSqlite3Orm.PInvoke.Types.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum SqliteConfigOption
{
    SingleThread = 1,
    MultiThread = 2,
    Serialized = 3
}