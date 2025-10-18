namespace LibSqlite3Orm.Models.Orm.Events;

public class GenerativeLogicTraceEventArgs : EventArgs
{
    public GenerativeLogicTraceEventArgs(Lazy<string> message)
    {
        Message = message;
    }
    
    public Lazy<string> Message { get; }
}