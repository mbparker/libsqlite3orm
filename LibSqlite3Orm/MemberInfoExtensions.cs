using System.Reflection;
using System.Runtime.Serialization;

namespace LibSqlite3Orm;

public static class MemberInfoExtensions
{
    public static Type GetValueType(this MemberInfo member)
    {
        if (member is FieldInfo field)
            return field.FieldType;
        if (member is PropertyInfo property)
            return property.PropertyType;
        throw NewInvalidMemberException(member);
    }
    
    public static object GetValue(this MemberInfo member, object obj)
    {
        if (member is FieldInfo field)
            return field.GetValue(obj);
        if (member is PropertyInfo property)
            return property.GetValue(obj);
        throw NewInvalidMemberException(member);
    }

    public static void SetValue(this MemberInfo member, object obj, object value)
    {
        if (member is FieldInfo field)
            field.SetValue(obj, value);
        else if (member is PropertyInfo property)
            property.SetValue(obj, value);
        else
            throw NewInvalidMemberException(member);
    }

    private static InvalidDataContractException NewInvalidMemberException(MemberInfo member)
    {
        return new InvalidDataContractException($"Member {member.Name} on type {member.DeclaringType?.AssemblyQualifiedName} is neither a field nor a property.");
    }
}