using System.Runtime.InteropServices;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.PInvoke;
using LibSqlite3Orm.PInvoke.Types.Enums;
using LibSqlite3Orm.PInvoke.Types.Exceptions;
using LibSqlite3Orm.Types;

namespace LibSqlite3Orm.Concrete;

public class SqliteConnection : ISqliteConnection
{
    private static ISqliteCustomCollationRegistry collationRegistry;
    private readonly Func<ISqliteConnection, ISqliteCommand> commandFactory;
    private readonly Func<ISqliteConnection, ISqliteTransaction> transactionFactory;
    private readonly List<ISqliteTransaction> transactionStack = new(); // Can't be an actual stack object.
    private readonly HashSet<int> collationIdsRegisteredOnThisConnection = new();
    private IntPtr dbHandle;
    private bool caseSensitiveLike;
    private bool foreignKeysEnforced = true;
    private bool disposed;

    public SqliteConnection(ISqliteCustomCollationRegistry collationRegistry, Func<ISqliteConnection,
        ISqliteCommand> commandFactory, Func<ISqliteConnection, ISqliteTransaction> transactionFactory)
    {
        SqliteConnection.collationRegistry ??= collationRegistry;
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

    public bool CaseSensitiveLike
    {
        get => caseSensitiveLike;
        set 
        { 
            caseSensitiveLike = value;
            if (Connected)
                SetCaseSensitiveLike(caseSensitiveLike);
        }
    }
    
    public bool ForeignKeysEnforced
    {
        get => foreignKeysEnforced;
        set 
        { 
            foreignKeysEnforced = value;
            if (Connected)
                SetForeignKeyEnforcement(foreignKeysEnforced);
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }

    public static void ContainerDisposing()
    {
        collationRegistry = null;
    }

    public void Open(string filename, SqliteOpenFlags flags, string virtualFileSystemName = null)
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
        // Both of these default to false/off to be consistent with official raw SQLite native lib behavior. 
        SetCaseSensitiveLike(CaseSensitiveLike);
        SetForeignKeyEnforcement(ForeignKeysEnforced);
        RegisterCollationsWithConnection();
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
            Filename = string.Empty;
            ConnectionClosed?.Invoke(this, EventArgs.Empty);
            dbHandle = IntPtr.Zero;
            collationIdsRegisteredOnThisConnection.Clear();
        }
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

    private void CreateCustomCollationUtf8(string name, SqliteCustomCollation collation)
    {
        static int CollationFunc(IntPtr pArg, int len1, IntPtr s1, int len2, IntPtr s2)
        {
            var str1 = Marshal.PtrToStringUTF8(s1, len1);
            var str2 = Marshal.PtrToStringUTF8(s2, len2);
            return collationRegistry.GetCollation(pArg.ToInt32())?.Invoke(str1, str2) ?? 0;
        }

        var collationId = collationRegistry.GetCollationId(name);
        if (collationId > 0 && collationIdsRegisteredOnThisConnection.Add(collationId))
        {
            var ret = SqliteExternals.CreateCollation2(GetHandle(), name, SqliteTextEncoding.Utf8, new IntPtr(collationId),
                CollationFunc, IntPtr.Zero);
            if (ret != SqliteResult.OK)
                throw new SqliteException(ret, SqliteExternals.ExtendedErrCode(dbHandle),
                    $"Cannot create custom collation ({SqliteTextEncoding.Utf8}), Code: {ret:X}");
        }
    }
    
    private void CreateCustomCollationUtf16(string name, SqliteCustomCollation collation, SqliteTextEncoding encoding = SqliteTextEncoding.Utf16)
    {
        static int CollationFunc(IntPtr pArg, int len1, IntPtr s1, int len2, IntPtr s2)
        {
            var str1 = Marshal.PtrToStringUni(s1, len1 / 2);
            var str2 = Marshal.PtrToStringUni(s2, len2 / 2);
            return collationRegistry.GetCollation(pArg.ToInt32())?.Invoke(str1, str2) ?? 0;
        }

        if (encoding == SqliteTextEncoding.Utf8) throw new ArgumentOutOfRangeException(nameof(encoding));
        
        var collationId = collationRegistry.GetCollationId(name);
        if (collationId > 0 && collationIdsRegisteredOnThisConnection.Add(collationId))
        {
            var ret = SqliteExternals.CreateCollation16(GetHandle(), name, encoding, new IntPtr(collationId),
                CollationFunc);
            if (ret != SqliteResult.OK)
                throw new SqliteException(ret, SqliteExternals.ExtendedErrCode(dbHandle),
                    $"Cannot create custom collation ({encoding}), Code: {ret:X}");
        }
    }    

    private void RegisterCollationsWithConnection()
    {
        foreach (var regItem in collationRegistry.GetAllRegistrations())
        {
            switch (regItem.Encoding)
            {
                case SqliteTextEncoding.Utf8:
                    CreateCustomCollationUtf8(regItem.Name, regItem.CollationFunc);
                    break;
                default:
                    CreateCustomCollationUtf16(regItem.Name, regItem.CollationFunc, regItem.Encoding);
                    break;
            }
        }
    }

    private void Pragma(string name, bool enabled)
    {
        if (!Connected) throw new InvalidOperationException($"You must call {nameof(Open)} before calling {nameof(Pragma)}.");
        var onOffStr = enabled ? "ON" : "OFF";
        using (var cmd = CreateCommand())
            cmd.ExecuteNonQuery($"PRAGMA {name} = {onOffStr};");
    }
    
    private void SetCaseSensitiveLike(bool enabled)
    {
        Pragma("case_sensitive_like", enabled);
    }
    
    private void SetForeignKeyEnforcement(bool enabled)
    {
        Pragma("foreign_keys", enabled);
    }

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