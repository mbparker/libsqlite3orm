namespace LibSqlite3Orm.Abstract;

public interface ISqliteParameterCollectionAddTo
{
    ISqliteParameter Add(string name, object value);
}

public interface ISqliteParameterCollectionDebug : IEnumerable<ISqliteParameterDebug>
{
    int Count { get; }
    ISqliteParameterDebug this[int index] { get; }
    ISqliteParameterDebug this[string name] { get; }    
}

public interface ISqliteParameterCollection : IEnumerable<ISqliteParameter>, ISqliteParameterCollectionAddTo
{
    int Count { get; }
    ISqliteParameter this[int index] { get; }
    ISqliteParameter this[string name] { get; }
    void BindAll(IntPtr statement);
}