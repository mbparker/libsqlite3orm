using LibSqlite3Orm.PInvoke.Types.Enums;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.Models.Orm;

[Serializable]
public class SqliteDbSchema
{
    public long FormatVersion { get; set; } = OrmConstants.CurrentSchemaFormatVersion;
    public Dictionary<string, SqliteDbSchemaTable> Tables { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, SqliteDbSchemaIndex> Indexes { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    
    public static void ThrowIfIncompatibleWithLibrary(SqliteDbSchema schema)
    {
        if (schema.FormatVersion < OrmConstants.OldestCompatibleSchemaFormatVersion)
            throw new InvalidDataException(
                $"The database is not compatible with this version of {nameof(LibSqlite3Orm)}.\n\n" +
                $"Database ORM Schema Format Version: {schema.FormatVersion}\n" +
                $"Oldest ORM Schema Format Version Supported By Library: {OrmConstants.OldestCompatibleSchemaFormatVersion}");        
    }
}

public class SqliteDbSchemaTable
{
    public string Name { get; set; }
    public string ModelTypeName { get; set; }
    public Dictionary<string, SqliteDbSchemaTableColumn> Columns { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public SqliteDbSchemaTablePrimaryKeyColumn PrimaryKey { get; set; }
    public List<SqliteDbSchemaTableForeignKey> ForeignKeys { get; set; } = [];
    public string[] CompositePrimaryKeyFields { get; set; } = [];
    public List<SqliteDbSchemaTableForeignKeyNavigationProperty> NavigationProperties { get; set; } = [];
}

public enum SqliteDbSchemaTableForeignKeyNavigationPropertyKind
{
    OneToOne,
    OneToMany
}

public class SqliteDbSchemaTableForeignKeyNavigationProperty
{
    public string ForeignKeyTableName { get; set; }
    public int ForeignKeyId { get; set; }
    public SqliteDbSchemaTableForeignKeyNavigationPropertyKind Kind { get; set; }
    public string ReferencedEntityTypeName { get; set; }
    public string ReferencedEntityTableName { get; set; }
    public string PropertyEntityTypeName { get; set; }
    public string PropertyEntityTableName { get; set; }
    public string PropertyEntityMember { get; set; }
}

public class SqliteDbSchemaTableColumn
{
    public string Name { get; set; }
    public string ModelFieldName { get; set; }
    public string ModelFieldTypeName { get; set; }
    public string SerializedFieldTypeName { get; set; }
    public SqliteDataType DbFieldTypeAffinity { get; set; }
    public bool IsNotNull { get; set; }
    public SqliteLiteConflictAction? IsNotNullConflictAction { get; set; }
    public bool IsUnique { get; set; }
    public bool IsImmutable { get; set; }
    public SqliteLiteConflictAction? IsUniqueConflictAction { get; set; }
    public SqliteCollation? Collation { get; set; }
    public string CustomCollation { get; set; }
    public string DefaultValueLiteral { get; set; }
}

public class SqliteDbSchemaTablePrimaryKeyColumn
{
    public string FieldName { get; set; }
    public bool Ascending { get; set; }
    public SqliteLiteConflictAction? PrimaryKeyConflictAction { get; set; }
    public bool AutoIncrement { get; set; }
    public bool AutoGuid { get; set; }
}

public class SqliteDbSchemaTableForeignKeyFieldPair
{
    public string TableModelProperty { get; set; }
    public string TableFieldName { get; set; }
    public string ForeignTableModelProperty { get; set; }
    public string ForeignTableFieldName { get; set; }
}

public class SqliteDbSchemaTableForeignKey
{
    public int Id { get; set; }
    public SqliteDbSchemaTableForeignKeyFieldPair[] KeyFields { get; set; }
    public string ForeignTableName { get; set; }
    public string ForeignTableModelTypeName { get; set; }
    public SqliteForeignKeyAction? UpdateAction { get; set; }
    public SqliteForeignKeyAction? DeleteAction { get; set; }
    public bool Optional { get; set; }
}

public class SqliteDbSchemaIndex
{
    public string TableName { get; set; }
    public string IndexName { get; set; }
    public bool IsUnique { get; set; }
    public List<SqliteDbSchemaIndexColumn> Columns { get; set; } = new();
}

public class SqliteDbSchemaIndexColumn
{
    public string Name { get; set; }
    public SqliteCollation? Collation { get; set; }
    public string CustomCollation { get; set; }
    public bool SortDescending { get; set; }
}