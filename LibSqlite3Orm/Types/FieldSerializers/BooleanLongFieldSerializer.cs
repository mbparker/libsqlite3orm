using LibSqlite3Orm.Abstract;

namespace LibSqlite3Orm.Types.FieldSerializers;

public class BooleanLongFieldSerializer : ISqliteFieldSerializer
{
    public Type RuntimeType => typeof(bool);
    public Type SerializedType => typeof(long);
    
    public object Serialize(object value)
    {
        return (bool)value ? 1L : 0L;
    }

    public object Deserialize(object value)
    {
        return (long)value != 0;
    }
}