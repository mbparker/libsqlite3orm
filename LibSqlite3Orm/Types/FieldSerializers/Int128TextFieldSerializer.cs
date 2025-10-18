using LibSqlite3Orm.Abstract;

namespace LibSqlite3Orm.Types.FieldSerializers;

public class Int128TextFieldSerializer : ISqliteFieldSerializer
{
    public Type RuntimeType => typeof(Int128);
    public Type SerializedType => typeof(string);
    
    public object Serialize(object value)
    {
        return ((Int128)value).ToString("D40");
    }

    public object Deserialize(object value)
    {
        return Int128.Parse((string)value);
    }
}