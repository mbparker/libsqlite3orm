using LibSqlite3Orm.Abstract;

namespace LibSqlite3Orm.Types.FieldSerializers;

public class UInt128TextFieldSerializer : ISqliteFieldSerializer
{
    public Type RuntimeType => typeof(UInt128);
    public Type SerializedType => typeof(string);
    
    public object Serialize(object value)
    {
        return ((UInt128)value).ToString("D40");
    }

    public object Deserialize(object value)
    {
        return UInt128.Parse((string)value);
    }
}