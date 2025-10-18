using System.Collections;
using LibSqlite3Orm.Abstract;

namespace LibSqlite3Orm.Concrete;

public class SqliteParameterCollection : ISqliteParameterCollection, ISqliteParameterCollectionDebug
{
    private readonly List<ISqliteParameter> parameters = new();
    private readonly Func<string, int, ISqliteParameter> parameterFactory;
    
    public SqliteParameterCollection(Func<string, int, ISqliteParameter> parameterFactory)
    {
        this.parameterFactory = parameterFactory;
    }
    
    public int Count => parameters.Count;

    public ISqliteParameter this[int index] => parameters[index];

    public ISqliteParameter this[string name] => parameters.FirstOrDefault(x => x.Name == name);
    
    ISqliteParameterDebug ISqliteParameterCollectionDebug.this[int index] => parameters[index] as ISqliteParameterDebug;

    ISqliteParameterDebug ISqliteParameterCollectionDebug.this[string name] => parameters.FirstOrDefault(x => x.Name == name) as ISqliteParameterDebug;    
    
    public ISqliteParameter Add(string name, object value)
    {
        var result = parameterFactory(name, parameters.Count + 1);
        result.Set(value);
        parameters.Add(result);
        return result;
    }

    public void BindAll(IntPtr statement)
    {
        foreach (var param in parameters)
            param.Bind(statement);
    }
    
    IEnumerator<ISqliteParameterDebug> IEnumerable<ISqliteParameterDebug>.GetEnumerator()
    {
        return parameters.Select(x => x as ISqliteParameterDebug).GetEnumerator();
    }

    IEnumerator<ISqliteParameter> IEnumerable<ISqliteParameter>.GetEnumerator()
    {
        return parameters.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return parameters.GetEnumerator();
    }
}