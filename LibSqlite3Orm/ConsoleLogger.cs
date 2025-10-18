namespace LibSqlite3Orm;

public static class ConsoleLogger
{
    private static string filename;
    private static Stream logStream;
    private static StreamWriter logStreamWriter;

    static ConsoleLogger()
    {
        Filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            $"{nameof(LibSqlite3Orm)}-ConsoleLog.txt");
    }
    
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

    public static void Dispose()
    {
        Filename = null;    
    }
    
    public static void WriteLine(string message)
    {
        WriteLine(null, message);
    }
    
    public static void WriteLine(ConsoleColor? color, string message)
    {
        if (!color.HasValue)
        {
            WriteLineInternal(message);    
            return;
        }
        
        var prevColor = Console.ForegroundColor;
        Console.ForegroundColor = color.Value;
        WriteLineInternal(message);
        Console.ForegroundColor = prevColor;
    }

    private static void WriteLineInternal(string s)
    {
        Console.WriteLine(s);
        logStreamWriter?.WriteLine($"{DateTimeOffset.Now:O}: {s}");
    }
}