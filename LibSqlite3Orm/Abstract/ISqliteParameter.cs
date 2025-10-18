using LibSqlite3Orm.PInvoke.Types.Enums;

namespace LibSqlite3Orm.Abstract;

public interface ISqliteParameterDebug
{
    string Name { get; }
    int Index { get; }
    object DeserializedValue { get; }
    object SerialzedValue { get; }
    SqliteDataType SerializedTypeAffinity { get; }    
    string GetDebugValue(int truncateBlobsTo = 1024);
}

public interface ISqliteParameter
{
    string Name { get; }
    int Index { get; }
    object DeserializedValue { get; }
    object SerialzedValue { get; }
    SqliteDataType SerializedTypeAffinity { get; }
    void Set(object value);
    void Bind(IntPtr statement);
}