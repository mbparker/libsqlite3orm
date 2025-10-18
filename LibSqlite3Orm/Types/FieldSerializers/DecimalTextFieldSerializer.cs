using LibSqlite3Orm.Abstract;

namespace LibSqlite3Orm.Types.FieldSerializers;

public class DecimalTextFieldSerializer : ISqliteFieldSerializer
{
    public Type RuntimeType => typeof(decimal);
    public Type SerializedType => typeof(string);
    
    public object Serialize(object value)
    {
        return ((decimal)value).ToString("0.0###########################");
    }

    public object Deserialize(object value)
    {
        return decimal.Parse((string)value);
    }
}