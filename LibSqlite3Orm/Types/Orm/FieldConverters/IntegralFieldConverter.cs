using System.Reflection;
using LibSqlite3Orm.Abstract.Orm;

namespace LibSqlite3Orm.Types.Orm.FieldConverters;

public class IntegralFieldConverter : ISqliteFailoverFieldConverter
{
    private static readonly Type TypeOfIConvertible = typeof(IConvertible);
    
    public bool CanConvert(Type typeFrom, Type typeTo)
    {
        return CanConvert(typeFrom, typeTo, out var dummy1, out var dummy2, out var dummy3);
    }

    public object Convert(Type typeFrom, object value, Type typeTo, IFormatProvider formatProvider = null)
    {
        if (!CanConvert(typeFrom, typeTo, out var realTypeFrom, out var realTypeTo, out var reason)) 
            throw new NotSupportedException($"{nameof(IntegralFieldConverter)} cannot convert {typeFrom.Name} ({realTypeFrom.Name}) to {typeTo.Name} ({realTypeTo.Name}): {reason}");
        if (value is null)
            return typeTo.IsValueType ? Activator.CreateInstance(typeTo) : null;
        try
        {
            return TypeOfIConvertible.InvokeMember(nameof(IConvertible.ToType),
                BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null,
                value, [realTypeTo, formatProvider]);
        }
        catch (TargetInvocationException e)
        {
            throw e.InnerException ?? e;
        }
    }
    
    private bool CanConvert(Type typeFrom, Type typeTo, out Type realTypeFrom, out Type realTypeTo, out string reason)
    {
        var objectType = typeof(object);
        if (typeFrom == objectType || typeTo == objectType)
        {
            realTypeFrom = typeFrom;
            realTypeTo = typeTo;
            reason = "Cannot convert to or from 'object'";
            return false;
        }
        
        realTypeFrom = Nullable.GetUnderlyingType(typeFrom) ?? typeFrom;
        realTypeTo = Nullable.GetUnderlyingType(typeTo) ?? typeTo;
        if (realTypeFrom == realTypeTo)
        {
            reason = "Types must be different";
            return false;
        }
        
        var result = IsConvertible(realTypeFrom) && IsConvertible(realTypeTo);
        reason = result ? "" : $"One or both types do not implement {nameof(IConvertible)}";
        return result;
    }

    private bool IsConvertible(Type type)
    {
        return TypeOfIConvertible.IsAssignableFrom(type);
    }
}