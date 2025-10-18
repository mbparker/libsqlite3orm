using LibSqlite3Orm.PInvoke.Types.Enums;

namespace LibSqlite3Orm.PInvoke.Types.Exceptions;

public class SqliteException : ApplicationException
{
    // For use only when ExtendedResultCodes flag is specified on the connection.
    public SqliteException (SqliteResult extendedResult, string message) 
        : this (extendedResult, extendedResult, message)
    {
    }
    
    public SqliteException (SqliteResult result, SqliteResult extendedResult, string message) 
        : base (message)
    {
        Result = result;
        ExtendedResult = extendedResult;
    }    
    
    public SqliteResult Result { get; }
    public SqliteResult ExtendedResult { get; }
}