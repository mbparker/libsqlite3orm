using LibSqlite3Orm.Abstract;

namespace LibSqlite3Orm.Types.FieldSerializers;

public class CharTextFieldSerializer : ISqliteFieldSerializer
{
    public Type RuntimeType => typeof(char);
    public Type SerializedType => typeof(string);
    
    public object Serialize(object value)
    {
        return ((char)value).ToString();
    }

    public object Deserialize(object value)
    {
        return ((string)value).ToCharArray().First();
    }
}