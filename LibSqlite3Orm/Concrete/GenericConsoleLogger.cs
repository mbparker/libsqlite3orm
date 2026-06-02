using LibSqlite3Orm.Abstract;

namespace LibSqlite3Orm.Concrete;

public class GenericConsoleLogger : IGenericLogger
{
    public void Dispose()
    {
        ConsoleLogger.Filename = null;
    }
    
    public void Info(string text)
    {
        ConsoleLogger.WriteLine($"[INFO] {text}");
    }

    public void Debug(string text)
    {
        ConsoleLogger.WriteLine(ConsoleColor.DarkGreen, $"[DEBUG] {text}");
    }

    public void Trace(string text)
    {
        ConsoleLogger.WriteLine(ConsoleColor.DarkCyan, $"[TRACE] {text}");
    }

    public void Warn(string text)
    {
        ConsoleLogger.WriteLine(ConsoleColor.Yellow, $"[WARN] {text}");
    }

    public void Error(string text)
    {
        ConsoleLogger.WriteError($"[ERROR] {text}");
    }

    public void Error(Exception ex, string text)
    {
        ConsoleLogger.WriteError($"[ERROR] {text}\n\n{ex}");
    }

    public void Fatal(string text)
    {
        ConsoleLogger.WriteError($"[FATAL] {text}");
    }

    public void Fatal(Exception ex, string text)
    {
        ConsoleLogger.WriteError($"[FATAL] {text}\n\n{ex}");
    }
}