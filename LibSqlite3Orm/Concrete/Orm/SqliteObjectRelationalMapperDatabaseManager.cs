using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.PInvoke.Types.Enums;

namespace LibSqlite3Orm.Concrete.Orm;

public class SqliteObjectRelationalMapperDatabaseManager<TContext> : ISqliteObjectRelationalMapperDatabaseManager<TContext>
    where TContext : ISqliteOrmDatabaseContext
{
    private readonly ISqliteFileOperations fileOperations;
    private readonly ISqliteDbFactory dbFactory;
    private readonly Func<TContext> contextFactory;
    private ISqliteDbSchemaMigrator<TContext> migrator;
    private TContext _context;
    private ISqliteConnection _connection;

    public SqliteObjectRelationalMapperDatabaseManager(
        Func<TContext> contextFactory,
        Func<ISqliteDbSchemaMigrator<TContext>> migratorFactory,
        ISqliteFileOperations fileOperations,
        ISqliteDbFactory dbFactory)
    {
        this.fileOperations = fileOperations;
        this.dbFactory = dbFactory;
        this.contextFactory = contextFactory;
        migrator = migratorFactory();
    }
    
    private ISqliteConnection Connection {
        get
        {
            if (_connection == null)
                throw new InvalidOperationException("Connection not set.");
            return _connection;
        }
        set => _connection = value;
    }
    
    private TContext Context
    {
        get
        {
            if (_context is null)
                _context = contextFactory();
            return _context;
        }
    }

    public SqliteDbSchemaChanges DetectedSchemaChanges { get; private set; } = new();

    public void UseConnection(ISqliteConnection connection)
    {
        Connection = connection.GetReference();
        migrator.UseConnection(Connection);
    }

    public void Dispose()
    {
        migrator?.Dispose();
        migrator = null;
        Connection = null;
    }

    public bool IsDatabaseInitialized()
    {
        return dbFactory.IsDatabaseAlreadyInitialized(Connection);
    }

    public void CreateDatabase()
    {
        dbFactory.Create(Context.Schema, Connection);
        migrator.InitializeOrmState();
    }

    public bool Migrate()
    {
        DetectedSchemaChanges = migrator.CheckForSchemaChanges();
        ThrowIfManualMigrationRequired();
        if (!DetectedSchemaChanges.MigrationRequired) return false;
        migrator.Migrate(DetectedSchemaChanges);
        return true;
    }

    public void DeleteDatabase()
    {
        if (Connection is not null && !string.IsNullOrWhiteSpace(Connection.Filename) &&
            !Connection.ConnectionFlags.HasFlag(SqliteOpenFlags.Memory))
        {
            if (fileOperations.FileExists(Connection.Filename))
            {
                var filename = Connection.Filename;
                if (Connection.Connected) Connection.Close();
                ConsoleLogger.WriteLine(ConsoleColor.Red, "DELETING DATABASE!!");
                fileOperations.DeleteFile(filename);
            }
        }
    }
    
    private void ThrowIfManualMigrationRequired()
    {
        if (DetectedSchemaChanges.ManualMigrationRequired)
        {
            var reasons = string.Join('\n',
                DetectedSchemaChanges.NonMigratableAlteredColumns.Select(x =>
                    $"{x.TableName}.{x.ColumnName}: {x.Reason}"));
            throw new InvalidDataException(
                $"The database cannot be automatically migrated for the following reason(s):\n\n{reasons}\n\nManual migration is required.");
        }
    }
}