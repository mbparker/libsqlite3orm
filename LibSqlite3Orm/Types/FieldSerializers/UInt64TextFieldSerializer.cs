using LibSqlite3Orm.Abstract;

namespace LibSqlite3Orm.Types.FieldSerializers;

public class UInt64TextFieldSerializer : ISqliteFieldSerializer
{
    public Type RuntimeType => typeof(UInt64);
    public Type SerializedType => typeof(string);
    
    public object Serialize(object value)
    {
        return ((UInt64)value).ToString("D25");
    }

    public object Deserialize(object value)
    {
        return UInt64.Parse((string)value);
    }
}