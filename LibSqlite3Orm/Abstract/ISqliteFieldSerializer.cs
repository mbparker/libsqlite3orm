namespace LibSqlite3Orm.Abstract;

public interface ISqliteFieldSerializer
{
    Type RuntimeType { get; }
    Type SerializedType { get; }
    
    object Serialize(object value);
    object Deserialize(object value);
}