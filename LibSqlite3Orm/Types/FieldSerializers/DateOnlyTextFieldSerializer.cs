using LibSqlite3Orm.Abstract;

namespace LibSqlite3Orm.Types.FieldSerializers;

public class DateOnlyTextFieldSerializer : ISqliteFieldSerializer
{
    private const string Format = "yyyy-MM-dd";
    
    public Type RuntimeType => typeof(DateOnly);
    public Type SerializedType => typeof(string);
    
    public object Serialize(object value)
    {
        return ((DateOnly)value).ToString(Format);
    }

    public object Deserialize(object value)
    {
        return DateOnly.ParseExact((string)value, Format, null);
    }
}