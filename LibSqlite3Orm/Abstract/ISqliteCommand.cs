namespace LibSqlite3Orm.Abstract;

public interface ISqliteCommand : IDisposable
{
    ISqliteParameterCollection Parameters { get; }
    
    int ExecuteNonQuery(IEnumerable<string> sql);
    int ExecuteNonQuery(string sql);
    ISqliteDataReader ExecuteQuery(IEnumerable<string> sql);
    ISqliteDataReader ExecuteQuery(string sql);
}