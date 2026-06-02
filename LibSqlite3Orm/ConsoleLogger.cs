namespace LibSqlite3Orm;

/// <summary>
/// Don't use this class directly except for "main" before container availability, or after container disposal.
/// </summary>
public static class ConsoleLogger
{
    private static string filename;
    private static Stream logStream;
    private static StreamWriter logStreamWriter;
    
    public static string Filename
    {
        set
        {
            if (filename != value)
            {
                filename = null;
                logStreamWriter?.Flush();
                logStreamWriter?.Dispose();
                logStreamWriter = null;
                logStream?.Dispose();
                logStream = null;

                if (!string.IsNullOrWhiteSpace(value))
                {
                    var path = Path.GetDirectoryName(value);
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);
                        filename = value;
                        logStream = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                        logStreamWriter = new StreamWriter(logStream);
                    }
                }
            }
        } 
    }
    
    public static void WriteLine(string message)
    {
        WriteLine(null, message);
    }
    
    public static void WriteLine(ConsoleColor? color, string message)
    {
        WriteLineInternal(color, message, Console.Out);
    }

    public static void WriteError(Exception ex)
    {
        WriteError(ex, "Unhandled exception");    
    }
    
    public static void WriteError(string s)
    {
        WriteLineInternal(ConsoleColor.Red, s, Console.Error);              
    }
    
    public static void WriteError(Exception ex, string message)
    {
        WriteError($"{message}:\n{ex}");
    }    
    
    private static void WriteLineInternal(ConsoleColor? color, string message, TextWriter writer)
    {
        if (!color.HasValue)
        {
            WriteLineInternal(message, writer);    
            return;
        }
        
        var prevColor = Console.ForegroundColor;
        Console.ForegroundColor = color.Value;
        WriteLineInternal(message, writer);
        Console.ForegroundColor = prevColor;
    }    
    
    private static void WriteLineInternal(string s, TextWriter writer)
    {
        writer.WriteLine(s);
        WriteToLogStream(s);
    }    

    private static void WriteToLogStream(string s)
    {
        logStreamWriter?.WriteLine($"{DateTimeOffset.Now:O}: {s}");
    }
}