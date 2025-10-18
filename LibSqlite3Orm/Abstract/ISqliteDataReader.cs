namespace LibSqlite3Orm.Abstract;

public interface ISqliteDataReader : IEnumerable<ISqliteDataRow>, IDisposable
{
    event EventHandler OnDispose; 
    ISqliteConnection Connection { get; }
}