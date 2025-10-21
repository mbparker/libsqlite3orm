using LibSqlite3Orm.PInvoke.Types.Enums;

namespace LibSqlite3Orm.Abstract;

public interface ISqliteConnection : IDisposable
{
    event EventHandler ConnectionClosed;
    
    event EventHandler BeforeDispose;
    
    bool Connected { get; }
    
    int TransactionDepth { get; }
    
    bool InTransaction { get; }

    SqliteOpenFlags ConnectionFlags { get; }

    string VirtualFileSystemName { get; }
    
    string Filename { get; }
    
    /// <summary>
    /// Sets the case_sensitive_like pragma value to ON or OFF. Default is OFF for consistency with SQLite native behavior.
    /// If connected to a DB, it will be applied immediately, otherwise when the connection is opened to a DB.
    /// </summary>
    bool CaseSensitiveLike { get; set; }
    
    /// <summary>
    /// Sets the foreign_keys pragma value to ON or OFF. Default is ON because the native SQLite behavior (OFF) doesn't make sense at this point in time.
    /// If connected to a DB, it will be applied immediately, otherwise when the connection is opened to a DB.
    /// </summary>
    bool ForeignKeysEnforced { get; set; }

    /// <summary>
    /// Open a database with full control over the connection flags, and optionally specify a VFS module name.
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="flags"></param>
    /// <param name="virtualFileSystemName"></param>
    void Open(string filename, SqliteOpenFlags flags, string virtualFileSystemName = null);
    
    /// <summary>
    /// This overload will open a database for read and write. Optionally, the database must already exist. Extended error codes are enabled.
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="mustExist"></param>
    void OpenReadWrite(string filename, bool mustExist);
    
    /// <summary>
    /// This overload opens an existing database, in read-only mode, with extended error codes.
    /// </summary>
    /// <param name="filename"></param>
    void OpenReadOnly(string filename);

    void OpenInMemory();
    
    /// <summary>
    /// Gets the internal Sqlite database handle. For internal library use only to pass to P/Invoke calls.
    /// </summary>
    /// <returns>The native connection handle</returns>
    IntPtr GetHandle();
    
    void Close();

    ISqliteCommand CreateCommand();
    
    long GetLastInsertedId();

    ISqliteTransaction BeginTransaction();

    /// <summary>
    /// In order to allow all parts of the ORM to operate with the same connection, this returns a "pass-thru" connection
    /// that maintains an internal reference to the real connection. The interface is the same, but is implemented by a
    /// the wrapper class.
    /// </summary>
    /// <returns>The connection reference instance</returns>
    ISqliteConnection GetReference();
}