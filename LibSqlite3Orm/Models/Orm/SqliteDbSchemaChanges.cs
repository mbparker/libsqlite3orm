namespace LibSqlite3Orm.Models.Orm;

public class SqliteDbSchemaChanges
{
    internal SqliteDbSchemaChanges(SqliteDbSchema previousSchema, IEnumerable<SqliteDbSchemaTable> newTables,
        IEnumerable<SqliteDbSchemaTable> removedTables, IEnumerable<RenamedTable> renamedTables,
        IEnumerable<AlteredTable> alteredTables, IEnumerable<NonMigratableAlteredColumn> nonMigratableAlteredColumns)
    {
        PreviousSchema = previousSchema;
        NewTables = newTables.ToArray();
        RemovedTables = removedTables.ToArray();
        RenamedTables = renamedTables.ToArray();
        AlteredTables = alteredTables.ToArray();
        NonMigratableAlteredColumns = nonMigratableAlteredColumns.ToArray();
    }

    internal SqliteDbSchemaChanges()
        : this(null, [], [], [], [], [])
    {
    }

    public SqliteDbSchema PreviousSchema { get; }
    public IReadOnlyList<SqliteDbSchemaTable> NewTables { get; }
    public IReadOnlyList<SqliteDbSchemaTable> RemovedTables { get; }
    public IReadOnlyList<RenamedTable> RenamedTables { get; }
    public IReadOnlyList<AlteredTable> AlteredTables { get; }
    public IReadOnlyList<NonMigratableAlteredColumn> NonMigratableAlteredColumns { get; set; }

    public bool MigrationRequired =>
        NewTables.Any() || RemovedTables.Any() || RenamedTables.Any() || AlteredTables.Any() || ManualMigrationRequired;

    public bool ManualMigrationRequired => NonMigratableAlteredColumns.Any();
}

public class RenamedTable
{
    public RenamedTable(string oldName, string newName)
    {
        OldName = oldName;
        NewName = newName;
    }
    
    public string OldName { get; }
    public string NewName { get; }
}

public class AlteredTable
{
    public AlteredTable(SqliteDbSchemaTable oldTableSchema, SqliteDbSchemaTable newTableSchema,
        IEnumerable<string> newColumnNames, IEnumerable<string> removedColumnNames)
    {
        OldTableSchema = oldTableSchema;
        NewTableSchema = newTableSchema;
        NewColumnNames = newColumnNames.ToArray();
        RemovedColumnNames = removedColumnNames.ToArray();
    }

    public SqliteDbSchemaTable NewTableSchema { get; }
    public SqliteDbSchemaTable OldTableSchema { get; }
    public IReadOnlyList<string> NewColumnNames { get; }
    public IReadOnlyList<string> RemovedColumnNames { get; }
}

public class NonMigratableAlteredColumn
{
    public NonMigratableAlteredColumn(string tableName, string columnName, string reason)
    {
        TableName = tableName;
        ColumnName = columnName;
        Reason = reason;
    }
    
    public string TableName { get; }
    public string ColumnName { get; }
    public string Reason { get; }
}