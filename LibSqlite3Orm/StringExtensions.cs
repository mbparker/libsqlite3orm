using System.Text;

namespace LibSqlite3Orm;

public static class StringExtensions
{
    public static string UnicodeToUtf8(this string s)
    {
        var utf16Bytes = Encoding.Unicode.GetBytes(s);
        var utf8Bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, utf16Bytes);
        return Encoding.UTF8.GetString(utf8Bytes);
    }
    
    public static string Utf8ToUnicode(this string s)
    {
        var utf8Bytes = Encoding.UTF8.GetBytes(s);
        var utf16Bytes = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, utf8Bytes);
        return Encoding.Unicode.GetString(utf16Bytes);
    }
}