namespace LibSqlite3Orm.Abstract;

public interface IGenericLogger : IDisposable
{
    void Info(string text);

    void Debug(string text);
        
    void Trace(string text);

    void Warn(string text);

    void Error(string text);
        
    void Error(Exception ex, string text);

    void Fatal(string text);
    
    void Fatal(Exception ex, string text);
}