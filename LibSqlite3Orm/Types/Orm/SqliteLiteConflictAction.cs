using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LibSqlite3Orm.Types.Orm;

[JsonConverter(typeof(StringEnumConverter))]
public enum SqliteLiteConflictAction
{
    Rollback,
    Abort,
    Fail,
    Ignore,
    Replace
}