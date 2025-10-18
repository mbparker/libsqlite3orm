using System.Globalization;
using LibSqlite3Orm.Abstract;

namespace LibSqlite3Orm.Types.FieldSerializers;

public class DateTimeTextFieldSerializer : ISqliteFieldSerializer
{
    private const string Format = "yyyy-MM-dd HH:mm:ss.FFFFFFF";
    
    public Type RuntimeType => typeof(DateTime);
    public Type SerializedType => typeof(string);
    
    public object Serialize(object value)
    {
        return ((DateTime)value).ToString(Format);
    }

    public object Deserialize(object value)
    {
        return DateTime.ParseExact((string)value, Format, null, DateTimeStyles.None);
    }
}