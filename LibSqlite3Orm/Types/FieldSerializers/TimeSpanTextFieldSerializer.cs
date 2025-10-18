using System.Globalization;
using LibSqlite3Orm.Abstract;

namespace LibSqlite3Orm.Types.FieldSerializers;

public class TimeSpanTextFieldSerializer : ISqliteFieldSerializer
{
    public Type RuntimeType => typeof(TimeSpan);
    public Type SerializedType => typeof(string);
    
    public object Serialize(object value)
    {
        return ((TimeSpan)value).ToString();
    }

    public object Deserialize(object value)
    {
        return TimeSpan.Parse((string)value);
    }
}