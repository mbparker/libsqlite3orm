namespace LibSqlite3Orm.Abstract;

public interface ISqliteFieldValueSerialization
{
    bool IsSerializerRegisteredForModelType(Type modelType);
    void RegisterSerializer(ISqliteFieldSerializer serializer);
    void ReplaceSerializer(Type modelType, ISqliteFieldSerializer newSerializer);
    ISqliteFieldSerializer this[Type modelType] { get; }
}