using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.PInvoke;
using LibSqlite3Orm.PInvoke.Types.Enums;
using LibSqlite3Orm.PInvoke.Types.Exceptions;

namespace LibSqlite3Orm.Concrete;

public class SqliteConnection : ISqliteConnection
{
    private readonly Func<ISqliteConnection, ISqliteCommand> commandFactory;
    private readonly Func<ISqliteConnection, ISqliteTransaction> transactionFactory;
    private readonly List<ISqliteTransaction> transactionStack = new(); // Can't be an actual stack object.
    private IntPtr dbHandle;
    private bool disposed;
    
    public SqliteConnection(Func<ISqliteConnection, ISqliteCommand> commandFactory,
        Func<ISqliteConnection, ISqliteTransaction> transactionFactory)
    {
        this.commandFactory = commandFactory;
        this.transactionFactory = transactionFactory;
    }

    ~SqliteConnection()
    {
        Dispose(false);
    }
    
    public event EventHandler ConnectionClosed;
    public event EventHandler BeforeDispose;
    
    public bool Connected => dbHandle != IntPtr.Zero;

    public int TransactionDepth => transactionStack.Count;
    
    public bool InTransaction => TransactionDepth > 0;

    public SqliteOpenFlags ConnectionFlags { get; private set; }
    
    public string VirtualFileSystemName {get; private set;}
    
    public string Filename { get; private set; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }

    public void Open(string filename, SqliteOpenFlags flags, bool noForeignKeyEnforcement = false, string virtualFileSystemName = null)
    {
        if (Connected) throw new InvalidOperationException("The database connection is already open.");
        ConnectionFlags = flags | SqliteOpenFlags.ExtendedErrorCodes;
        VirtualFileSystemName = string.IsNullOrWhiteSpace(virtualFileSystemName) ? null : virtualFileSystemName.UnicodeToUtf8();
        dbHandle = IntPtr.Zero;
        // Specifying the ExtendedErrorCodes flag will cause all API calls to return the extended code - including this one.
        var ret = SqliteExternals.Open2(filename?.UnicodeToUtf8(), out dbHandle, (int)flags, VirtualFileSystemName);
        if (ret != SqliteResult.OK)
            throw new SqliteException(ret, $"Cannot open database '{filename}', Code: {ret:X}");
        Filename = filename;
        // Foreign key enforcement is DISABLED BY DEFAULT in Sqlite3!! 
        //if (!noForeignKeyEnforcement)
        //    SetForeignKeyEnforcement(true);
    }

    public void OpenReadWrite(string filename, bool mustExist)
    {
        var flags = SqliteOpenFlags.ReadWrite;
        if (!mustExist) flags |= SqliteOpenFlags.Create;
        Open(filename, flags);
    }
    
    public void OpenInMemory()
    {
        var flags = SqliteOpenFlags.ReadWrite | SqliteOpenFlags.Memory;
        Open(null, flags);
    }
    
    public void OpenReadOnly(string filename)
    {
        Open(filename, SqliteOpenFlags.ReadOnly);
    }    
    
    public IntPtr GetHandle()
    {
        if (!Connected) throw new InvalidOperationException("The database connection is not open.");
        return dbHandle;
    }    

    public void Close()
    {
        if (!Connected) throw new InvalidOperationException("The database connection is not open.");
        if (TransactionDepth > 0) throw new InvalidOperationException($"The database connection has {TransactionDepth} transaction(s) in progress.");
        try
        {
            var ret = SqliteExternals.Close2(dbHandle);
            if (ret != SqliteResult.OK)
                throw new SqliteException(ret, SqliteExternals.ExtendedErrCode(dbHandle), $"Cannot close database '{Filename}', Code: {ret:X}");
        }
        finally
        {
            ConnectionFlags = SqliteOpenFlags.ExtendedErrorCodes;
            VirtualFileSystemName = null;
            dbHandle = IntPtr.Zero;
            Filename = string.Empty;
            ConnectionClosed?.Invoke(this, EventArgs.Empty);
        }
    }

    public void SetForeignKeyEnforcement(bool enabled)
    {
        SqliteExternals.SetForeignKeyEnforcement(dbHandle, enabled);
    }

    public ISqliteCommand CreateCommand()
    {
        return commandFactory(this);
    }

    public long GetLastInsertedId()
    {
        return SqliteExternals.LastInsertRowId(GetHandle());
    }

    public ISqliteTransaction BeginTransaction()
    {
        var transaction = transactionFactory(this);
        transaction.Committed += TransactionEnded;
        transaction.RolledBack += TransactionEnded;
        transactionStack.Insert(0, transaction);
        return transaction;
    }

    public ISqliteConnection GetReference() => new SqliteConnectionReference(this);

    private void Dispose(bool disposing)
    {
        if (disposed) return;
        
        try
        { 
            BeforeDispose?.Invoke(this, EventArgs.Empty);
            if (Connected)
                Close();
            disposed = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private void TransactionEnded(object sender, EventArgs e)
    {
        var transaction = (ISqliteTransaction)sender;
        transaction.Committed -= TransactionEnded;
        transaction.RolledBack -= TransactionEnded;
        transactionStack.Remove(transaction);
    }
}