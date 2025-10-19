using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.PInvoke.Types.Enums;

namespace LibSqlite3Orm.Concrete;

public class SqliteConnectionReference : ISqliteConnection
{
    private ISqliteConnection connection;
    
    public SqliteConnectionReference(ISqliteConnection connection)
    {
        this.connection = connection;
        HookEvents();
    }

    public void Dispose()
    {
        UnhookEvents();
        connection = null;
    }

    public event EventHandler ConnectionClosed;
    public event EventHandler BeforeDispose;
    
    public bool Connected => connection.Connected;
    public int TransactionDepth => connection.TransactionDepth;
    public bool InTransaction => connection.InTransaction;
    public SqliteOpenFlags ConnectionFlags => connection.ConnectionFlags;
    public string VirtualFileSystemName => connection.VirtualFileSystemName;
    public string Filename => connection.Filename;

    public void Open(string filename, SqliteOpenFlags flags, string virtualFileSystemName = null)
    {
        connection.Open(filename, flags, virtualFileSystemName);
    }

    public void OpenReadWrite(string filename, bool mustExist)
    {
        connection.OpenReadWrite(filename, mustExist);
    }

    public void OpenReadOnly(string filename)
    {
        connection.OpenReadOnly(filename);
    }

    public void OpenInMemory()
    {
        connection.OpenInMemory();
    }

    public IntPtr GetHandle() => connection.GetHandle();

    public void Close()
    {
        connection.Close();
    }

    public ISqliteCommand CreateCommand() => connection.CreateCommand();

    public long GetLastInsertedId() => connection.GetLastInsertedId();

    public ISqliteTransaction BeginTransaction() =>  connection.BeginTransaction();

    public ISqliteConnection GetReference() => this;
    
    private void HookEvents()
    {
        connection.BeforeDispose += ConnectionOnBeforeDispose;
        connection.ConnectionClosed += ConnectionOnConnectionClosed;
    }

    private void UnhookEvents()
    {
        connection.BeforeDispose -= ConnectionOnBeforeDispose;
        connection.ConnectionClosed -= ConnectionOnConnectionClosed;
    }
    
    private void ConnectionOnConnectionClosed(object sender, EventArgs e)
    {
        ConnectionClosed?.Invoke(this, e);
    }

    private void ConnectionOnBeforeDispose(object sender, EventArgs e)
    {
        BeforeDispose?.Invoke(this, e);
    }
}