using System.ComponentModel;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.PInvoke;
using LibSqlite3Orm.PInvoke.Types.Enums;

namespace LibSqlite3Orm.Concrete;

public class SqliteDataColumn : ISqliteDataColumn
{
    private readonly IntPtr statement;
    private readonly ISqliteFieldValueSerialization serialization;
    private object serializedValue;
    
    public SqliteDataColumn(int index, string name, IntPtr statement, ISqliteFieldValueSerialization serialization)
    {
        Index = index;
        this.statement = statement;
        this.serialization = serialization;
        Name = name;
        TypeAffinity = SqliteExternals.ColumnType(this.statement, Index);
        ReadSerializedValue();
    }
    
    public string Name { get; }
    public int Index { get; }
    public SqliteDataType TypeAffinity { get; }

    public object Value()
    {
        return serializedValue;
    }

    public T ValueAs<T>()
    {
        return (T)ValueAs(typeof(T));
    }
    
    public object ValueAs(Type type)
    {
        if (serializedValue is null) return null;
        var nullableType = Nullable.GetUnderlyingType(type);
        type = nullableType ?? type;        
        var value = DeserializeValue(type);
        return value;
    }    

    private void ReadSerializedValue()
    {
        switch (TypeAffinity)
        {
            case SqliteDataType.Integer:
                serializedValue = SqliteExternals.ColumnInt64(statement, Index);
                break;
            case SqliteDataType.Float:
                serializedValue = SqliteExternals.ColumnDouble(statement, Index);
                break;
            case SqliteDataType.Text:
                serializedValue = SqliteExternals.ColumnText(statement, Index);
                if (serializedValue is not null)
                    serializedValue = ((string)serializedValue).Utf8ToUnicode();
                break;
            case SqliteDataType.Blob:
                serializedValue = SqliteExternals.ColumnBlob(statement, Index);
                break;
            case SqliteDataType.Null:
                serializedValue = null;
                break;
            default:
                throw new InvalidEnumArgumentException(nameof(TypeAffinity), (int)TypeAffinity,
                    typeof(SqliteDataType));
        }
    }

    private object DeserializeValue(Type targetType)
    {
        object result = null;
        switch (TypeAffinity)
        {
            case SqliteDataType.Integer:
                result = DeserializeInteger(targetType);
                break;
            case SqliteDataType.Float:
                result = DeserializeDouble(targetType);
                break;
            case SqliteDataType.Text:
                result = DeserializeText(targetType);
                break;
            case SqliteDataType.Blob:
                result = DeserializeBlob(targetType);
                break;
        }

        if (result is null)
            throw new InvalidOperationException(
                $"Type {serializedValue.GetType()} could not be converted to {targetType} on column {Name}. Consider registering an {nameof(ISqliteFieldSerializer)} implementation.");
        return result;
    }
    
    private object DeserializeInteger(Type targetType)
    {
        if (targetType.GetSqliteDataType() == SqliteDataType.Integer)
        {
            var value = (long)serializedValue;
            if (targetType == typeof(sbyte)) return Convert.ToSByte(value);
            if (targetType == typeof(short)) return Convert.ToInt16(value);
            if (targetType == typeof(int)) return Convert.ToInt32(value);
            if (targetType == typeof(byte)) return Convert.ToByte(value);
            if (targetType == typeof(ushort)) return Convert.ToUInt16(value);
            if (targetType == typeof(uint)) return Convert.ToUInt32(value);
            return value;
        }
        
        return serialization[targetType]?.Deserialize(serializedValue);
    }
    
    private object DeserializeDouble(Type targetType)
    {
        if (targetType.GetSqliteDataType() == SqliteDataType.Float)
        {
            var value = (double)serializedValue;
            if (targetType == typeof(float)) return Convert.ToSingle(value);
            return value;
        }
        
        return serialization[targetType]?.Deserialize(serializedValue);
    }

    private object DeserializeText(Type targetType)
    {
        if (targetType.GetSqliteDataType() == SqliteDataType.Text)
        {
            var value = (string)serializedValue;
            if (targetType == typeof(char)) return value.ToCharArray().FirstOrDefault();
            return value;
        }
        
        return serialization[targetType]?.Deserialize(serializedValue);
    }    

    private object DeserializeBlob(Type targetType)
    {
        if (targetType.GetSqliteDataType() == SqliteDataType.Blob) return (byte[])serializedValue;
        return serialization[targetType]?.Deserialize(serializedValue);
    }
}