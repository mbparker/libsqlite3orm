using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LibSqlite3Orm.Models.Orm;

[JsonConverter(typeof(StringEnumConverter))]
public enum UpsertResult
{
    Updated,
    Inserted,
    Failed
}