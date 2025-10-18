using LibSqlite3Orm.Abstract;

namespace LibSqlite3Orm.Types.FieldSerializers;

public class EnumLongFieldSerializer : ISqliteEnumFieldSerializer 
{
    public Type RuntimeType => EnumType;
    public Type SerializedType => typeof(long);
    public Type EnumType { get; }

    public EnumLongFieldSerializer(Type enumType)
    {
        EnumType = enumType;
    }

    public object Serialize(object value)
    {
        if (value is null) return null;
        var realType = Nullable.GetUnderlyingType(RuntimeType) ?? RuntimeType;
        if (realType.GetEnumUnderlyingType() == typeof(ulong))
            return BitConverter.ToInt64(BitConverter.GetBytes((ulong)value));
        return Convert.ToInt64(value);
    }

    public object Deserialize(object value)
    {
        if (value is null) return Activator.CreateInstance(RuntimeType);
        var realType = Nullable.GetUnderlyingType(RuntimeType) ?? RuntimeType;
        if (realType.GetEnumUnderlyingType() == typeof(ulong))
            value = BitConverter.ToUInt64(BitConverter.GetBytes((long)value));
        return Enum.Parse(RuntimeType, $"{value}");
    }
}