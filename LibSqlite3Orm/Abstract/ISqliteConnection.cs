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
    /// Open a database with full control over the connection flags, default foreign key enforcement, and optionally specify a VFS module name.
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="flags"></param>
    /// <param name="noForeignKeyEnforcement"></param>
    /// <param name="virtualFileSystemName"></param>
    void Open(string filename, SqliteOpenFlags flags, bool noForeignKeyEnforcement = false, string virtualFileSystemName = null);
    
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
    
    IntPtr GetHandle();
    
    void Close();

    void SetForeignKeyEnforcement(bool enabled);

    ISqliteCommand CreateCommand();

    long GetLastInsertedId();

    ISqliteTransaction BeginTransaction();

    ISqliteConnection GetReference();
}