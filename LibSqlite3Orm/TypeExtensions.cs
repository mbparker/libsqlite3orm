using LibSqlite3Orm.PInvoke.Types.Enums;

namespace LibSqlite3Orm;

public static class TypeExtensions
{
    /// <summary>
    /// Determines what the SQLite data type is for this tyme, if one applies.
    /// </summary>
    /// <returns>The SQLite data type when one is compatible. Otherwise, null</returns>
    public static SqliteDataType? GetSqliteDataType(this Type type)
    {
        // These are the only fundamental types that can be stored in SQLite. Anything else must be serialized to one of these.
        type = Nullable.GetUnderlyingType(type) ?? type;
        if (type.IsEnum) return null;
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            //case TypeCode.UInt64: - Values can overflow. Must be converted to string to be useful in a stored state.
                return SqliteDataType.Integer;
            case TypeCode.Double:
            case TypeCode.Single:
                return SqliteDataType.Float;
            case TypeCode.String:
            case TypeCode.Char:
                return SqliteDataType.Text;
            default:
                if (type.IsArray && Type.GetTypeCode(type.GetElementType()) == TypeCode.Byte)
                    return SqliteDataType.Blob;
                return null;
        }
    }

    /// <summary>
    /// Checks to see if the passed in type can have null assigned to an instance of that type.
    /// NOTE: Do not call Nullable.GetUnderlyingType and call this method on that result type. Otherwise this will
    /// always return true.
    /// </summary>
    /// <returns>True if the type (or it's underlying type) is a value type. Otherwise false.</returns>
    public static bool IsNotNullable(this Type type)
    {
        // Surely there is a better way of determining if a field or property can be set to null or not???
        var realType =  Nullable.GetUnderlyingType(type) ?? type;
        return realType == type && realType.IsValueType;
    }
}