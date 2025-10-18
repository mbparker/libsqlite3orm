namespace LibSqlite3Orm.Abstract;

public interface ISqliteEnumFieldSerializer : ISqliteFieldSerializer
{
    Type EnumType { get; }
}