using System.ComponentModel;
using System.Reflection;
using System.Text;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.PInvoke;
using LibSqlite3Orm.PInvoke.Types.Enums;

namespace LibSqlite3Orm.Concrete;

public class SqliteParameter : ISqliteParameter, ISqliteParameterDebug
{
    private static readonly IntPtr NoDeallocator = new(-1);
    private readonly ISqliteFieldValueSerialization serialization;
    
    public SqliteParameter(string name, int index, ISqliteFieldValueSerialization serialization)
    {
        Name = name;
        Index = index;
        this.serialization = serialization;
    }
    
    public string Name { get; }
    public int Index { get; }
    
    public object DeserializedValue { get; private set; }
    public object SerialzedValue { get; private set; }
    public SqliteDataType SerializedTypeAffinity { get; private set; }

    public void Set(object value)
    {
        DeserializedValue = value;
        SerializeValue();
    }

    string ISqliteParameterDebug.GetDebugValue(int truncateBlobsTo)
    {
        if (SerialzedValue is null) return "NULL";
        if (SerialzedValue.GetType() == typeof(byte[]))
        {
            var array = (byte[])SerialzedValue;
            return Convert.ToHexString(array.Take(Math.Min(truncateBlobsTo, array.Length)).ToArray());
        }

        return SerialzedValue.ToString();
    }

    public void Bind(IntPtr statement)
    {
        switch (SerializedTypeAffinity)
        {
            case SqliteDataType.Integer:
                var intType = SerialzedValue.GetType();
                if (intType == typeof(long))
                    SqliteExternals.BindInt64(statement, Index, (long)SerialzedValue);
                else
                    SqliteExternals.BindInt64(statement, Index, Convert.ToInt64(SerialzedValue));
                break;  
            case SqliteDataType.Float:
                var realType = SerialzedValue.GetType();
                if (realType == typeof(double))
                    SqliteExternals.BindDouble(statement, Index, (double)SerialzedValue);
                else
                    SqliteExternals.BindDouble(statement, Index, Convert.ToDouble(SerialzedValue));
                break;
            case SqliteDataType.Text:
                var s = ((string)SerialzedValue).UnicodeToUtf8();
                var n = Encoding.UTF8.GetByteCount(s);
                SqliteExternals.BindText(statement, Index, s, n, NoDeallocator);
                break;
            case SqliteDataType.Blob:
                var blob = (byte[])SerialzedValue;
                SqliteExternals.BindBlob(statement, Index, blob, blob.Length, NoDeallocator);
                break;
            case SqliteDataType.Null:
                SqliteExternals.BindNull(statement, Index);
                break;
            default:
                throw new InvalidEnumArgumentException(nameof(SerializedTypeAffinity), (int)SerializedTypeAffinity,
                    typeof(SqliteDataType));
        }
    }

    private void SerializeValue()
    {
        if (DeserializedValue is null)
        {
            SerializedTypeAffinity = SqliteDataType.Null;
            SerialzedValue = null;
            return;
        }

        var serializer = serialization[DeserializedValue.GetType()];
        if (serializer is not null)
            SerialzedValue = serializer.Serialize(DeserializedValue);
        else
            SerialzedValue = DeserializedValue; // No serializer - might be storable as is - check below
        
        var type = SerialzedValue.GetType();
        type = Nullable.GetUnderlyingType(type) ?? type;

        var affinity = type.GetSqliteDataType();
        if (affinity.HasValue)
        {
            SerializedTypeAffinity = affinity.Value;
            return;
        }

        throw new InvalidOperationException(
            $"Type {type} is not supported. Consider registering an {nameof(ISqliteFieldSerializer)} implementation " +
            "to serialize that type to a type that can be stored.");
    }
}