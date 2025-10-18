using LibSqlite3Orm.Abstract;

namespace LibSqlite3Orm.Concrete;

public class SqliteTransaction : ISqliteTransaction
{
    public SqliteTransaction(ISqliteConnection connection, ISqliteUniqueIdGenerator uniqueIdGenerator)
    {
        Connection = connection;
        Name = uniqueIdGenerator.NewUniqueId();
        using (var cmd = Connection.CreateCommand())
        {
            cmd.ExecuteNonQuery($"SAVEPOINT '{Name}';");
        }
    }

    ~SqliteTransaction()
    {
        Dispose();
    }

    public event EventHandler Committed;
    public event EventHandler RolledBack;
    
    public string Name { get; }
    
    public ISqliteConnection Connection { get; private set; }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (Connection is not null)
            Rollback();
    }
    
    public void Commit()
    {
        if (Connection is null) throw new InvalidOperationException("Transaction has already been disposed.");
        using (var cmd = Connection.CreateCommand())
        {
            cmd.ExecuteNonQuery($"RELEASE SAVEPOINT '{Name}';");
            Connection = null;
            Committed?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Rollback()
    {
        if (Connection is null) throw new InvalidOperationException("Transaction has already been disposed.");
        using (var cmd = Connection.CreateCommand())
        {
            cmd.ExecuteNonQuery($"ROLLBACK TRANSACTION TO SAVEPOINT '{Name}';");
            Connection = null;
            RolledBack?.Invoke(this, EventArgs.Empty);
        }
    }
}