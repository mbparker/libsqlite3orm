using System.Collections;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.PInvoke;
using LibSqlite3Orm.PInvoke.Types.Enums;
using LibSqlite3Orm.PInvoke.Types.Exceptions;

namespace LibSqlite3Orm.Concrete;

public class SqliteDataReader : ISqliteDataReader
{
    private ISqliteConnection connection;
    private ISqliteCommand command;
    private IntPtr statement;
    private readonly Func<IntPtr, ISqliteDataRow> rowFactory;
    private bool disposed;
    private bool didEnumerate;

    public SqliteDataReader(ISqliteConnection connection, ISqliteCommand command, IntPtr statement,
        Func<IntPtr, ISqliteDataRow> rowFactory)
    {
        this.connection = connection;
        this.command = command;
        this.statement = statement;
        this.rowFactory = rowFactory;
    }

    ~SqliteDataReader()
    {
        Dispose(false);
    }

    public event EventHandler OnDispose; 
    
    public ISqliteConnection Connection => connection;

    public IEnumerator<ISqliteDataRow> GetEnumerator()
    {
        var enumerator = new SqlDataRowEnumerator(connection, statement, rowFactory);
        enumerator.RowEnumerated += EnumeratorOnRowEnumerated;
        if (didEnumerate)
            enumerator.Reset();
        return enumerator;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }

    private void Dispose(bool disposing)
    {
        if (disposed) return;
        
        SqliteExternals.Finalize(statement);
        statement = IntPtr.Zero;
        
        if (disposing)
        {
            command = null;
            connection = null;
            OnDispose?.Invoke(this, EventArgs.Empty);
        }

        disposed = true;
    }
    
    private void EnumeratorOnRowEnumerated(object sender, ISqliteDataRow e)
    {
        ((SqlDataRowEnumerator)sender).RowEnumerated -= EnumeratorOnRowEnumerated;
        didEnumerate = true;
    }

    private class SqlDataRowEnumerator : IEnumerator<ISqliteDataRow>
    {
        private ISqliteConnection connection;
        private IntPtr statement;
        private readonly Func<IntPtr, ISqliteDataRow> rowFactory;
        private ISqliteDataRow current;

        public SqlDataRowEnumerator(ISqliteConnection connection, IntPtr statement, Func<IntPtr, ISqliteDataRow> rowFactory)
        {
            this.connection = connection;
            this.connection.BeforeDispose += ConnectionOnBeforeDispose;
            this.statement = statement;
            this.rowFactory = rowFactory;
        }
        
        private IntPtr ConnectionHandle => connection?.GetHandle() ?? IntPtr.Zero;

        internal event EventHandler<ISqliteDataRow> RowEnumerated;

        public bool MoveNext()
        {
            var ret = SqliteExternals.Step(statement);
            if (ret != SqliteResult.OK && ret != SqliteResult.Done && ret != SqliteResult.Row)
                throw new SqliteException(ret, SqliteExternals.ExtendedErrCode(ConnectionHandle), SqliteExternals.ErrorMsg(ConnectionHandle));
            var result = ret != SqliteResult.Done;
            if (result)
            {
                current = rowFactory.Invoke(statement);
                RowEnumerated?.Invoke(this, current);
            }

            return result;
        }

        public void Reset()
        {
            SqliteExternals.Reset(statement);
            current = null;
        }

        ISqliteDataRow IEnumerator<ISqliteDataRow>.Current => current;

        object IEnumerator.Current => current;

        public void Dispose()
        {
            ReleaseConnection();
        }
    
        private void ConnectionOnBeforeDispose(object sender, EventArgs e)
        {
            ReleaseConnection();
        }

        private void ReleaseConnection()
        {
            if (connection is not null)
            {
                connection.BeforeDispose -= ConnectionOnBeforeDispose;
                connection = null;
            }
        }
    }
}